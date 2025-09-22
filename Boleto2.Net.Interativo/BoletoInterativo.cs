using Boleto2.Net.Interativo.Entidades;
using Boleto2Net;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boleto2.Net.Interativo
{
    public class BoletoInterativo
    {
        private List<String> mOperacao;
        private String mDirBase = @"C:\Interativo\PDFExp";

        private List<String> mListaPathPDFS = new List<string>();

        private List<Pendfin> pendfins;

        private String[] mBancosSuportados = new string[] { "753", "748", "341", "237", "041" };

        private static Boolean HabilitaLogs = true;

        public BoletoInterativo(List<String> operacao)
        {
            this.mOperacao = operacao;
        }

        public void SetDirBase(String dirBase)
        {
            this.mDirBase = dirBase;
        }

        public static List<String> ObterOperacoes(String transacoes)
        {
            var conn = ObterConexaoBanco();
            List<String> operacoes = new List<string>();

            String sql = "select pfin_operacao " +
           " from pendfin " +
           $" where pfin_transacao in ('{transacoes}')";

            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                operacoes.Add(reader.GetSafeValue<String>("pfin_operacao"));
            }

            reader.Close();
            conn.Close();
            return operacoes;
        }

        public void Gerar()
        {
            
            pendfins = LerDadosPendfin(this.mOperacao);

            List<String> pendentes = new List<string>(this.mOperacao);

            //if (pendfins.Count != mOperacao.Count)
            //    throw new Exception("Não foi possível localizar algumas das operações: (" + String.Join("|", pendfins) + ")");
            foreach (var pendfin in pendfins)
            {

                RegistrarLog($"Processando OP: {pendfin.pfin_operacao}");

                Empresa empresa = ObterEmpresa(pendfin.pfin_empr_codigo);
                if (empresa == null)
                    throw new Exception("Empresa não localizada");

                Cliente clientes = ObterDestinatario(pendfin);

                if (clientes == null)
                    throw new Exception("Clientes não localizados");

                IntegracaoBancariaDet det = ObterConfigDet(pendfin.pger_itbc_codigo);

                if (det == null)
                    throw new Exception("IntegracaoBancariaDet não localizado");

                IntegracaoBancaria integracaoBancaria = ObterIntegracaoBancaria(pendfin.pger_itbc_codigo);

                if (integracaoBancaria == null)
                    throw new Exception("Integracao Bancaria não localizado");

                GerarBoletoPDF(pendfin, empresa, clientes, det, integracaoBancaria);

                bool ok = pendentes.Remove(pendfin.pfin_operacao);
            }

            if (pendentes.Count > 0)
            {
                RegistrarLog($"** Erro ao gerar boletos das operações: {String.Join("|", pendentes)}");
                throw new Exception("Não foi possível gerar boletos das operações: (" + String.Join("|", pendentes) + ")");
            }
        }

        public void EnviarEmail()
        {
            try
            {
                if (EnviarEmailInterno())
                {
                    var conn = ObterConexaoBanco();

                    if (pendfins.Count > 0)
                    {
                        String sqlOperacoes = String.Join(",", pendfins.Select(t => $"'{t.pfin_operacao}'").ToArray());
                        String sql = $"update pendfin set pfin_envemail = 'S' where pfin_operacao = ({sqlOperacoes})";

                        conn.Open();

                        NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                        cmd.ExecuteScalar();

                        conn.Close();
                    }
                }
            }catch(Exception e)
            {
                MessageBox.Show("Erro ao enviar email: " + e.ToString(), "Erro ao enviar email", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private Boolean EnviarEmailInterno()
        {
            var conn = ObterConexaoBanco();
            conn.Open();

            String sql = $"SELECT * FROM configgeral";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);

            var reader = cmd.ExecuteReader();

            var cnfg_emailsmtp = String.Empty;
            var cnfg_emailportasmtp = 0L;
            var cnfg_emailssl = String.Empty;
            var cnfg_emailendereco = String.Empty;
            var cnfg_emailuser = String.Empty;
            var cnfg_emailsenha = String.Empty;

            while (reader.Read())
            {
                cnfg_emailsmtp = reader.GetSafeValue<String>("cnfg_emailsmtp");
                cnfg_emailportasmtp = reader.GetSafeValue<Int64>("cnfg_emailportasmtp");
                cnfg_emailssl = reader.GetSafeValue<String>("cnfg_emailssl");
                cnfg_emailendereco = reader.GetSafeValue<String>("cnfg_emailendereco");
                cnfg_emailuser = reader.GetSafeValue<String>("cnfg_emailuser");
                cnfg_emailsenha = reader.GetSafeValue<String>("cnfg_emailsenha");
                break;
            }

            reader.Close();
            conn.Close();

            //var cliente = ObterCliente(pendfins[0].pfin_codagente);
            Cliente cliente = ObterDestinatario(pendfins[0]);
            var empresa = ObterEmpresa(pendfins[0].pfin_empr_codigo);

            //cliente.clie_email = "marco@interativosistemas.com.br";
            //cnfg_emailuser = "interativosrv123@gmail.com";
            //cnfg_emailendereco = "interativosrv123@gmail.com";
            //cnfg_emailsenha = "sistemas123@";

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(cnfg_emailendereco);

            var splitDest1 = cliente.clie_email.Split(';').ToList();
            var splitDest2 = cliente.clie_cobremail.Split(';').ToList();


            //System.Windows.Forms.MessageBox.Show($"Enviando emails: {String.Join(Environment.NewLine, splitDest1)}");
            //System.Windows.Forms.MessageBox.Show($"Enviando emails: {String.Join(Environment.NewLine, splitDest2)}");
            //splitDest1.Clear();
            //splitDest2.Clear();
            //splitDest1.Add("marco@interativosistemas.com.br");

            //MessageBox.Show("Mandando por email para: " + cliente.clie_email + cliente.clie_cobremail);
            for (int i = 0; i < splitDest1.Count; i++)
                if (!String.IsNullOrEmpty(splitDest1[i]))
                    mail.To.Add(new MailAddress(splitDest1[i]));

            for (int i = 0; i < splitDest2.Count; i++)
                if (!String.IsNullOrEmpty(splitDest2[i]))
                    mail.To.Add(new MailAddress(splitDest2[i]));

            

            if (mail.To.Count == 0)
                return false;

            mail.Subject = $"Boleto {empresa.empr_nome}";
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;
            mail.BodyEncoding = Encoding.UTF8;

            const String newLine = "<br/>";
            mail.Body = "Segue o boleto em anexo" + newLine + newLine;

            for (int i = 0; i < pendfins.Count; i++)
            {
                String file = Path.Combine(mDirBase, "blt" + pendfins[i].pfin_operacao + ".pdf");
                if (!File.Exists(file))
                    throw new Exception("Arqiuvo não encontrado:" + file);

                mail.Body += "Número da Operação: " + pendfins[i].pfin_operacao + newLine;
                mail.Body += "Valor: " + pendfins[i].pfin_valor.ToString("C") + newLine;
                mail.Body += "Data Vencimento: " + pendfins[i].pfin_datavcto.Value.ToString("dd/MM/yyyy") + newLine;
                mail.Body += "Linha Digitável: " + pendfins[i].pfin_linhadig + newLine;

                if (pendfins[i].pfin_documento > 0)
                    mail.Body += "Boleto referente a nota fiscal nº " + pendfins[i].pfin_documento + newLine;

                if (String.IsNullOrEmpty(pendfins[i].pfin_observacao) == false)
                    mail.Body += "OBS:" + pendfins[i].pfin_observacao + newLine;

                mail.Body += "--------------------------------------------" + newLine;


                mail.Attachments.Add(new Attachment(file));
            }
            //mail.Body += "<font size=\"-3\">";
            mail.Body += "<b>";
            mail.Body += "Documento emitido pela empresa: " + empresa.empr_nome + " CNPJ:" + empresa.empr_cnpj + "." + newLine; ;
            mail.Body += $"{empresa.empr_cidade}/{empresa.empr_uf} - {empresa.empr_bairro},{empresa.empr_endereco} Nº {empresa.empr_numero} " + Environment.NewLine; ;
            mail.Body += "</b>";
            //mail.Body += "</font>";


            SmtpClient objSmtpClient = new SmtpClient();

            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object s,
                                    System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                    System.Security.Cryptography.X509Certificates.X509Chain chain,
                                    System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };
            objSmtpClient.Host = cnfg_emailsmtp;
            objSmtpClient.Port = Convert.ToInt32(cnfg_emailportasmtp);
            objSmtpClient.EnableSsl = cnfg_emailssl.Equals("2") || cnfg_emailssl.Equals("3") || cnfg_emailssl.Equals("4");
            objSmtpClient.UseDefaultCredentials = false;
            objSmtpClient.Credentials = new System.Net.NetworkCredential(cnfg_emailuser, cnfg_emailsenha);
            objSmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            objSmtpClient.Timeout = 10000;

            if (objSmtpClient.Host == "smtp.gmail.com")
            {
                objSmtpClient.Port = 587;
            }

            objSmtpClient.Send(mail);

            for (int i = 0; i < mail.Attachments.Count; i++)
                mail.Attachments[i]?.Dispose();

            mail.Dispose();
            objSmtpClient.Dispose();

            //System.Windows.Forms.MessageBox.Show($"Enviando emails: OK");
            return true;
        }

        public void GerarBoletoPDF(Pendfin pendfin, Empresa empresa, Cliente cliente, IntegracaoBancariaDet det, IntegracaoBancaria integracaoBancaria)
        {
            Boletos boletos = new Boletos();

            RegistrarLog($"Gerando PDF OP: {pendfin.pfin_operacao}");
            //Cabeçalho
            //if (pendfin.itbc_banco.Equals("748") == false && pendfin.itbc_banco.Equals("341") == false)
            if (mBancosSuportados.Contains(pendfin.itbc_banco) == false)
                 return;
            //throw new Exception("Bancos suportados por esta DLL Sicredi / Itau / NCBBank / Bradesco");

            var enuBmanco = (Bancos)Convert.ToUInt16(pendfin.itbc_banco);

            //boletos.Banco = Banco.Instancia(enumBanco);
            boletos.Banco = GerarBoletoInfoCedente(integracaoBancaria, det);
            
            boletos.Banco.Cedente.Endereco = new Boleto2Net.Endereco
            {
                LogradouroEndereco = empresa.empr_endereco,
                LogradouroNumero = empresa.empr_numero,
                LogradouroComplemento = empresa.empr_complemento,
                Bairro = empresa.empr_bairro,
                Cidade = empresa.empr_cidade,
                UF = empresa.empr_uf,
                CEP = empresa.empr_cep,
            };

            if (enuBmanco == Bancos.Nbcbank)
            {
                boletos.Banco.Cedente.Endereco.LogradouroEndereco = "<BR/>R URUGUAI 155 - CONJ 1308 - CENTRO, PORTO ALEGRE - RS CEP 90010-140";
                boletos.Banco.Cedente.Endereco.LogradouroNumero = String.Empty;
                boletos.Banco.Cedente.Endereco.LogradouroComplemento = String.Empty;
                boletos.Banco.Cedente.Endereco.Bairro = String.Empty;
                boletos.Banco.Cedente.Endereco.Cidade = String.Empty;
                boletos.Banco.Cedente.Endereco.UF = String.Empty;
                boletos.Banco.Cedente.Endereco.CEP = String.Empty;

            }
            //Títulos

            var boleto = new Boleto(boletos.Banco);
            boleto.Sacado = new Sacado
            {
                CPFCNPJ = cliente.clie_cnpj,
                Nome = cliente.clie_razaosocial,
                Observacoes = string.Empty,
                Endereco = new Boleto2Net.Endereco
                {
                    LogradouroEndereco = cliente.clie_endereco,
                    LogradouroNumero = cliente.clie_numero,
                    LogradouroComplemento = String.Empty,
                    Bairro = cliente.clie_bairro,
                    Cidade = cliente.clie_cidade,
                    UF = cliente.clie_uf,
                    CEP = cliente.clie_cep,
                }
            };

            boleto.CodigoOcorrencia = "01"; //Registrar remessa
            boleto.DescricaoOcorrencia = "Remessa Registrar";

            boleto.NumeroDocumento = pendfin.pfin_documento.ToString();
            boleto.NumeroControleParticipante = String.Empty;


            boleto.DataEmissao = pendfin.pfin_datalanc.Value;
            boleto.DataVencimento = pendfin.pfin_datavcto.Value;
            //ano/bytegeracao/sequencial
            int seq = 0;
            boleto.NossoNumero = pendfin.pfin_nossonumero.Contains(".") ? pendfin.pfin_nossonumero.Substring(0, pendfin.pfin_nossonumero.IndexOf(".")) : pendfin.pfin_nossonumero;

            if (enuBmanco == Bancos.Bradesco)
            {
                boleto.NossoNumero = pendfin.pfin_nossonumero.Substring(2, 11);
                boleto.NossoNumeroDV = pendfin.pfin_nossonumero.Substring(13, 1);
            }

            boleto.ValorTitulo = pendfin.pfin_valor;
            boleto.Aceite = "N";
            boleto.EspecieDocumento = TipoEspecieDocumento.DM;
            boleto.Carteira = boletos.Banco.Cedente.ContaBancaria.CarteiraPadrao ;
            boleto.VariacaoCarteira = String.Empty;

            boleto.DataDesconto = DateTime.Today;
            boleto.ValorDesconto = pendfin.pfin_desconto;

            Decimal percMulta = 0;
            if(enuBmanco == Bancos.Sicredi || enuBmanco == Bancos.Bradesco)
                percMulta = Convert.ToDecimal(det["008"].Replace('.', ',') ?? "0");
            else if (enuBmanco == Bancos.Itau)
                percMulta = Convert.ToDecimal(det["015"].Replace('.', ',') ?? "0");
            
            if (percMulta > 0)
            {
                boleto.DataMulta = boleto.DataVencimento.AddDays(1);
                boleto.PercentualMulta = percMulta;
                boleto.ValorMulta = boleto.ValorTitulo * (percMulta / 100);

                boleto.MensagemInstrucoesCaixa = $"Cobrar Multa de {boleto.ValorMulta.ToString("C")} após o vencimento.";
            }

            Decimal percentualMora = 0;
            // 0 = valor diario
            // 1 = valor mensaal
            Int32 tipoMora = 1; 

            if (enuBmanco == Bancos.Sicredi)
                percentualMora = Convert.ToDecimal(det["011"].Replace('.', ',') ?? "0");
            else if (enuBmanco == Bancos.Itau)
            {
                tipoMora = Convert.ToInt32(det["010"]);
                percentualMora = Convert.ToDecimal(det["011"].Replace('.', ',') ?? "0");
            }
            else if (enuBmanco == Bancos.Nbcbank)
            {
                tipoMora = 1;
                percentualMora = Convert.ToDecimal(det["008"].Replace('.', ',') ?? "0");
            }
            else if (enuBmanco == Bancos.Banrisul)
            {
                tipoMora = Convert.ToInt32(det["012"]);
                
            }


            if (percentualMora > 0)
            {
                if (tipoMora == 1)
                {
                    boleto.DataJuros = boleto.DataVencimento.AddDays(1);

                    boleto.PercentualJurosDia = percentualMora;
                    if (enuBmanco == Bancos.Itau)
                        boleto.PercentualJurosDia = (percentualMora / 30);

                    boleto.ValorJurosDia = boleto.ValorTitulo * (boleto.PercentualJurosDia / 100);

                    //string instrucao = $"Cobrar juros de {boleto.PercentualJurosDia.ToString("0.00")} % por dia de atraso";
                    string instrucao = $"Cobrar juros de {boleto.ValorJurosDia.ToString("C")} por dia de atraso";

                    if (string.IsNullOrEmpty(boleto.MensagemInstrucoesCaixa))
                        boleto.MensagemInstrucoesCaixa = instrucao;
                    else
                        boleto.MensagemInstrucoesCaixa += "<br/>" + instrucao;
                }
                else
                {
                    string instrucao = $"Cobrar juros de {percentualMora.ToString("C")} por dia de atraso";

                    if (string.IsNullOrEmpty(boleto.MensagemInstrucoesCaixa))
                        boleto.MensagemInstrucoesCaixa = instrucao;
                    else
                        boleto.MensagemInstrucoesCaixa += "<br/>" + instrucao;
                }

            }

            //if (string.IsNullOrEmpty(boleto.MensagemInstrucoesCaixa))
            //    boleto.MensagemInstrucoesCaixa = det["101"];
            //else
            if (string.IsNullOrEmpty(boleto.MensagemInstrucoesCaixa))
                boleto.MensagemInstrucoesCaixa += det["101"];
            else
                boleto.MensagemInstrucoesCaixa +=  "<br/>" +det["101"];


            /*
            boleto.CodigoInstrucao1 = string.Empty;
            boleto.ComplementoInstrucao1 = string.Empty;

            boleto.CodigoInstrucao2 = string.Empty;
            boleto.ComplementoInstrucao2 = string.Empty;

            boleto.CodigoInstrucao3 = string.Empty;
            boleto.ComplementoInstrucao3 = string.Empty;                
            */

            boleto.CodigoProtesto = TipoCodigoProtesto.NaoProtestar;
            if (det["009"] == "06" && enuBmanco == Bancos.Sicredi)
            {
                boleto.CodigoProtesto = TipoCodigoProtesto.ProtestarDiasUteis;
                boleto.DiasProtesto = Convert.ToInt32(det["010"]);
            }
            //nbcbank sempre vai ser protesto
            else if (enuBmanco == Bancos.Nbcbank || enuBmanco == Bancos.Bradesco)
            {
                boleto.CodigoProtesto = TipoCodigoProtesto.ProtestarDiasUteis;
                boleto.DiasProtesto = Convert.ToInt32(det["013"]);
            }
            else if (det["016"] == "1" && enuBmanco == Bancos.Banrisul)
            {
                boleto.CodigoProtesto = TipoCodigoProtesto.ProtestarDiasUteis;
                boleto.DiasProtesto = Convert.ToInt32(det["017"]);
            }

            //nbcbank semore vai ter protesto
            if (enuBmanco == Bancos.Nbcbank)
            {
                if (String.IsNullOrEmpty(boleto.MensagemInstrucoesCaixa) == false)
                    boleto.MensagemInstrucoesCaixa += "</br>";

                boleto.MensagemInstrucoesCaixa += $"Protestar após {boleto.DiasProtesto.ToString("00")} dias do vencimento";
            }
            //boleto.CodigoProtesto = this.Conta.DiasProtesto == 0 ? TipoCodigoProtesto.NaoProtestar : TipoCodigoProtesto.ProtestarDiasuteis;
            //boleto.DiasProtesto = this.Conta.DiasProtesto;

            boleto.CodigoBaixaDevolucao = TipoCodigoBaixaDevolucao.NaoBaixarNaoDevolver;
            boleto.DiasBaixaDevolucao = 0;

            if (enuBmanco == Bancos.Nbcbank)
            {
                boleto.Avalista.Nome = empresa.empr_nome;
                boleto.Avalista.CPFCNPJ = empresa.empr_cnpj;
                boleto.Avalista.Endereco = new Endereco();
                boleto.Avalista.Endereco.Bairro = empresa.empr_bairro;
                boleto.Avalista.Endereco.CEP = empresa.empr_cep;
                boleto.Avalista.Endereco.Cidade = empresa.empr_cidade;
                boleto.Avalista.Endereco.LogradouroNumero = empresa.empr_numero;
                boleto.Avalista.Endereco.UF = empresa.empr_uf;
                boleto.Avalista.Endereco.LogradouroEndereco = empresa.empr_endereco;
   
            }
            
            boleto.ValidarDadosChaves(pendfin.pfin_codbarras, pendfin.pfin_linhadig);
            boletos.Add(boleto);

            var boletoBancario = new BoletoBancario() { Boleto = boleto };
            var pdf = boletoBancario.MontaBytesPDF(false);

            if (!Directory.Exists(mDirBase))
                Directory.CreateDirectory(mDirBase);

            String pathPDF = Path.Combine(mDirBase, "blt" + pendfin.pfin_operacao + ".pdf");

            mListaPathPDFS.Add(pathPDF);

            RegistrarLog($"PDF gerado em: {pathPDF}");

            File.WriteAllBytes(pathPDF, pdf);
        }

        public static IBanco GerarBoletoInfoCedente(IntegracaoBancaria integracaoBancaria, IntegracaoBancariaDet det)
        {
            Bancos banco = (Bancos)Convert.ToUInt16(integracaoBancaria.itbc_banco);
            IBanco ibanco = null;
            switch (banco)
            {
                case Bancos.Sicredi:
                    ibanco = GerarBoletoInfoCedenteSicredi(integracaoBancaria, det);
                    break;
                case Bancos.Itau:
                    ibanco = GerarBoetoInfoCedenteItau(integracaoBancaria, det);
                    break;
                case Bancos.Nbcbank:
                    ibanco = GerarBoletoInfoCedenteNBCBank(integracaoBancaria, det);
                    break;
                case Bancos.Bradesco:
                    ibanco = GerarBoletoInfoCedenteBradesco(integracaoBancaria, det);
                    break;
                case Bancos.Banrisul:
                    ibanco = GerarBoletoInfoCedenteBanrisul(integracaoBancaria, det);
                    break;
                default:
                    throw new NotImplementedException("Função não suportada");
            }

            return ibanco;
        }
        

        public static IBanco GerarBoletoInfoCedenteSicredi(IntegracaoBancaria integracaoBancaria, IntegracaoBancariaDet det)
        {
            var banco = Banco.Instancia(Bancos.Sicredi);

            banco.Cedente = new Cedente
            {
                CPFCNPJ = det["004"],
                Nome = det["005"],
                Observacoes = string.Empty,
                ContaBancaria = new ContaBancaria
                {
                    Posto = det["002"],
                    Agencia = det["001"],
                    DigitoAgencia = "0",
                    OperacaoConta = string.Empty,
                    Conta = det["003"].Substring(0, 4),
                    DigitoConta = det["003"].Substring(4, 1),

                    //DigitoConta = this.Conta.NumeroDigito,
                    CarteiraPadrao = "A", // this.Conta.CarteiraBoleto,
                    VariacaoCarteiraPadrao = String.Empty, //this.Conta.VariacaoCarteira,
                    TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
                    TipoFormaCadastramento = integracaoBancaria.itbc_tpproc == "001" ? TipoFormaCadastramento.SemRegistro : TipoFormaCadastramento.ComRegistro,
                    TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa,
                    TipoDocumento = TipoDocumento.Tradicional
                },
                Codigo = det["003"].Substring(0, 4),
                CodigoDV = det["003"].Substring(4, 1),
                CodigoTransmissao = string.Empty,
                
            };

            banco.FormataCedente();

            return banco;
        }

        public static IBanco GerarBoetoInfoCedenteItau(IntegracaoBancaria integracaoBancaria, IntegracaoBancariaDet det)
        {
            var banco = Banco.Instancia(Bancos.Itau);
            banco.Cedente = new Cedente
            {
                CPFCNPJ = det["019"],
                Nome = det["002"],
                Observacoes = string.Empty,
                ContaBancaria = new ContaBancaria
                {
                    //Posto = det["002"],
                    Agencia = det["001"].Substring(0, 4),
                    DigitoAgencia = "0",
                    OperacaoConta = string.Empty,
                    Conta = det["001"].Substring(4, 5),
                    DigitoConta = det["001"].Substring(9, 1),

                    //DigitoConta = this.Conta.NumeroDigito,
                    CarteiraPadrao = det["004"], // this.Conta.CarteiraBoleto,
                    VariacaoCarteiraPadrao = String.Empty, //this.Conta.VariacaoCarteira,
                    TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
                    //TipoFormaCadastramento = pendfin.itbc_tpproc == "001" ? TipoFormaCadastramento.SemRegistro : TipoFormaCadastramento.ComRegistro,
                    TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa,
                    TipoDocumento = TipoDocumento.Tradicional,
                    LocalPagamento = "ATÉ O VENCIMENTO, PAGUE EM QUALQUER BANCO OU CORRESPONDENTE NÃO BANCÁRIO. <br/> APÓS O VENCIMENTO, ACESSE ITAU.COM.BR / BOLETOS E PAGUE EM QUALQUER BANCO  <br/> OU CORRESPONDENTE NÃO BANCÁRIO."
                },
                Codigo = det["001"].Substring(4, 5),
                CodigoDV = det["001"].Substring(9, 1),
                CodigoTransmissao = string.Empty,
                CodigoFormatado = det["001"].Substring(0, 4) + "/" + det["001"].Substring(4, 5) + det["001"].Substring(9, 1)
                //Endereco = new Boleto2Net.Endereco
                //{
                //    LogradouroEndereco = empresa.empr_endereco,
                //    LogradouroNumero = empresa.empr_numero,
                //    LogradouroComplemento = empresa.empr_complemento,
                //    Bairro = empresa.empr_bairro,
                //    Cidade = empresa.empr_cidade,
                //    UF = empresa.empr_uf,
                //    CEP = empresa.empr_cep,
                //}
            };

            return banco;
        }

        public static IBanco GerarBoletoInfoCedenteNBCBank(IntegracaoBancaria integracaoBancaria, IntegracaoBancariaDet det)
        {
            var banco = Banco.Instancia(Bancos.Nbcbank);

            banco.Cedente = new Cedente
            {
                CPFCNPJ = "74828799000145",
                Nome = "NOVO BANCO CONTINENTAL SA - BANCO MULTIPLO ",
                Observacoes = string.Empty,
                ContaBancaria = new ContaBancaria
                {
                    //Posto = det["002"],
                    Agencia = "3708",
                    DigitoAgencia ="7",
                    OperacaoConta = string.Empty,
                    Conta = "525905",
                    DigitoConta = "3",

                    //DigitoConta = this.Conta.NumeroDigito,
                    CarteiraPadrao = det["003"], // this.Conta.CarteiraBoleto,
                    VariacaoCarteiraPadrao = String.Empty, //this.Conta.VariacaoCarteira,
                    TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
                    //TipoFormaCadastramento = pendfin.itbc_tpproc == "001" ? TipoFormaCadastramento.SemRegistro : TipoFormaCadastramento.ComRegistro,
                    TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa,
                    TipoDocumento = TipoDocumento.Tradicional,
                    LocalPagamento = "PAGAVEL EM QUALQUER BANCO ATÉ O VENCIMENTO"
                },
                Codigo = "525905",
                CodigoDV = "3",
                CodigoTransmissao = string.Empty,
                CodigoFormatado = "3708" + "/" + "5259053"
                //Endereco = new Boleto2Net.Endereco
                //{
                //    LogradouroEndereco = empresa.empr_endereco,
                //    LogradouroNumero = empresa.empr_numero,
                //    LogradouroComplemento = empresa.empr_complemento,
                //    Bairro = empresa.empr_bairro,
                //    Cidade = empresa.empr_cidade,
                //    UF = empresa.empr_uf,
                //    CEP = empresa.empr_cep,
                //}
            };



            return banco;
        }

        public static IBanco GerarBoletoInfoCedenteBradesco(IntegracaoBancaria integracaoBancaria, IntegracaoBancariaDet det)
        {
            var banco = Banco.Instancia(Bancos.Bradesco);

            banco.Cedente = new Cedente
            {
                CPFCNPJ = det["016"],
                Nome = det["002"],
                Observacoes = string.Empty,
                ContaBancaria = new ContaBancaria
                {
                    //Posto = det["002"],
                    Agencia = det["004"],
                    DigitoAgencia = det["018"],
                    OperacaoConta = string.Empty,
                    Conta = det["005"],
                    DigitoConta = det["006"],
                    //DigitoConta = this.Conta.NumeroDigito,
                    //CarteiraPadrao = "09", // this.Conta.CarteiraBoleto,
                    CarteiraPadrao = det["003"] /* Implementadas 02 e 09 */,
                    VariacaoCarteiraPadrao = String.Empty, //this.Conta.VariacaoCarteira,
                    TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
                    //TipoFormaCadastramento = pendfin.itbc_tpproc == "001" ? TipoFormaCadastramento.SemRegistro : TipoFormaCadastramento.ComRegistro,
                    TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa,
                    TipoDocumento = TipoDocumento.Tradicional,
                    LocalPagamento = $"{det["101"]}<br/>{det["102"]}<br/>{det["103"]}<br/>{det["104"]}"  //"ATÉ O VENCIMENTO, PAGUE EM QUALQUER BANCO OU CORRESPONDENTE NÃO BANCÁRIO. <br/> APÓS O VENCIMENTO, ACESSE ITAU.COM.BR / BOLETOS E PAGUE EM QUALQUER BANCO  <br/> OU CORRESPONDENTE NÃO BANCÁRIO."
                },
                Codigo = det["001"].Substring(0, 6).PadLeft(7, '0'),
                CodigoDV = det["001"].Substring(6, 1),
                CodigoTransmissao = string.Empty,
                CodigoFormatado = $"{det["001"].Substring(0, 4)}-{det["018"]}/{det["005"]}-{det["006"]}"
                //det["001"].Substring(0, 4) + "/" + det["001"].Substring(4, 5) + det["001"].Substring(9, 1)
                //Endereco = new Boleto2Net.Endereco
                //{
                //    LogradouroEndereco = empresa.empr_endereco,
                //    LogradouroNumero = empresa.empr_numero,
                //    LogradouroComplemento = empresa.empr_complemento,
                //    Bairro = empresa.empr_bairro,
                //    Cidade = empresa.empr_cidade,
                //    UF = empresa.empr_uf,
                //    CEP = empresa.empr_cep,
                //}
            };

            return banco;
        }


        public static IBanco GerarBoletoInfoCedenteBanrisul(IntegracaoBancaria integracaoBancaria, IntegracaoBancariaDet det)
        {
            var banco = Banco.Instancia(Bancos.Banrisul);

            banco.Cedente = new Cedente
            {
                CPFCNPJ = det["001"],
                Nome = det["027"],
                Observacoes = string.Empty,
                ContaBancaria = new ContaBancaria
                {
                    //Posto = det["002"],
                    Agencia = det["002"].Substring(0, 4),
                    DigitoAgencia = "",
                    OperacaoConta = string.Empty,
                    Conta = det["002"].Substring(4, 8), //7 ou 8
                    DigitoConta = det["002"].Substring(12, 1),
                    //DigitoConta = this.Conta.NumeroDigito,
                    //CarteiraPadrao = "09", // this.Conta.CarteiraBoleto,
                    CarteiraPadrao = det["007"] /* Implementadas 02 e 09 */,
                    VariacaoCarteiraPadrao = String.Empty, //this.Conta.VariacaoCarteira,
                    TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
                    //TipoFormaCadastramento = pendfin.itbc_tpproc == "001" ? TipoFormaCadastramento.SemRegistro : TipoFormaCadastramento.ComRegistro,
                    TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa,
                    TipoDocumento = TipoDocumento.Tradicional,
                    LocalPagamento = $"{det["101"]}<br/>{det["102"]}<br/>{det["103"]}<br/>{det["104"]}"  //"ATÉ O VENCIMENTO, PAGUE EM QUALQUER BANCO OU CORRESPONDENTE NÃO BANCÁRIO. <br/> APÓS O VENCIMENTO, ACESSE ITAU.COM.BR / BOLETOS E PAGUE EM QUALQUER BANCO  <br/> OU CORRESPONDENTE NÃO BANCÁRIO."
                },
                Codigo = det["002"],
                CodigoDV = "",
                CodigoTransmissao = string.Empty,
                CodigoFormatado = $"{det["002"].Substring(0, 4)}/{det["002"].Substring(4,7)}.{det["002"].Substring(11,2)}"    
                //det["001"].Substring(0, 4) + "/" + det["001"].Substring(4, 5) + det["001"].Substring(9, 1)
                //Endereco = new Boleto2Net.Endereco
                //{
                //    LogradouroEndereco = empresa.empr_endereco,
                //    LogradouroNumero = empresa.empr_numero,
                //    LogradouroComplemento = empresa.empr_complemento,
                //    Bairro = empresa.empr_bairro,
                //    Cidade = empresa.empr_cidade,
                //    UF = empresa.empr_uf,
                //    CEP = empresa.empr_cep,
                //}
            };

            return banco;
        }
        //public static IBanco GerarBoletoInfoCedenteCaixa(IntegracaoBancaria integracaoBancaria, IntegracaoBancariaDet det)
        //{

        //}



        public void MostarBoleto()
        {
            //MessageBox.Show("Mostrando boleto");
            var stream = CombinarPDFs();
            //frmPDFViewer f = new frmPDFViewer(@"C:\Users\Marco\Downloads\Boleto_4.pdf");
            //MessageBox.Show("StreamCriado");
            try
            {

                frmPDFViewer f = new frmPDFViewer(stream);
                //MessageBox.Show("frmPDFViewer criado");
                f.ShowDialog();
               //MessageBox.Show("exibido");

                stream.Close();
                stream.Dispose();

            }catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }


        }

        public Stream CombinarPDFs()
        {
            MemoryStream msPDFOut = new MemoryStream();
            PdfSharp.Pdf.PdfDocument outputDocument = new PdfSharp.Pdf.PdfDocument();
            for (int i = 0; i < mListaPathPDFS.Count; i++)
            {
                String file = mListaPathPDFS[i];
                PdfSharp.Pdf.PdfDocument inputDocument = PdfSharp.Pdf.IO.PdfReader.Open(file, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
                int count = inputDocument.PageCount;
                for (int idx = 0; idx < count; idx++)
                {
                    PdfSharp.Pdf.PdfPage page = inputDocument.Pages[idx];
                    outputDocument.AddPage(page);
                }

                inputDocument.Close();

            }
            outputDocument.Save(msPDFOut, false);
            //msPDFOut.Close();

            return msPDFOut;
        }

        public Empresa ObterEmpresa(String codEmpresa)
        {
            Empresa empresa = new Empresa();
            var conn = ObterConexaoBanco();

            String sql = "select * from empresas " +
            $"where empr_codigo = '{codEmpresa}'";

            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                empresa.empr_endereco = reader.GetSafeValue<String>("empr_endereco");
                empresa.empr_cnpj = reader.GetSafeValue<String>("empr_cnpj");
                empresa.empr_complemento = reader.GetSafeValue<String>("empr_complemento");
                empresa.empr_bairro = reader.GetSafeValue<String>("empr_bairro");
                empresa.empr_cep = reader.GetSafeValue<String>("empr_cep");
                empresa.empr_cidade = reader.GetSafeValue<String>("empr_cidade");
                empresa.empr_uf = reader.GetSafeValue<String>("empr_uf");
                empresa.empr_numero = reader.GetSafeValue<String>("empr_numero");
                empresa.empr_nome = reader.GetSafeValue<String>("empr_nome");
            }

            reader.Close();
            conn.Close();
            return empresa;
        }

        public Cliente ObterDestinatario(Pendfin pendfin)
        {
            Cliente clientes = null;
            if (pendfin.pfin_tpagente == "C")
            {
                clientes = ObterCliente(pendfin.pfin_codagente);
            }
            else if (pendfin.pfin_tpagente == "F")
            {
                clientes = ObterFornecedor(pendfin.pfin_codagente);
            }
            else
            {
                throw new NotImplementedException($"Tipo do agente ({pendfin.pfin_tpagente}) desconhecido");
            }

            return clientes;
        }

        public Cliente ObterCliente(Int64 codCliente)
        {
            Cliente cliente = new Cliente();
            var conn = ObterConexaoBanco();

            String sql = "select * from clientes " +
            $"where clie_codigo = {codCliente}";

            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                cliente.clie_cnpj = reader.GetSafeValue<String>("clie_cnpj");
                cliente.clie_razaosocial = reader.GetSafeValue<String>("clie_razaosocial");
                cliente.clie_endereco = reader.GetSafeValue<String>("clie_endereco");
                cliente.clie_numero = reader.GetSafeValue<String>("clie_numero");
                cliente.clie_complemento = reader.GetSafeValue<String>("clie_complemento");
                cliente.clie_bairro = reader.GetSafeValue<String>("clie_bairro");
                cliente.clie_cidade = reader.GetSafeValue<String>("clie_cidade");
                cliente.clie_uf = reader.GetSafeValue<String>("clie_uf");
                cliente.clie_cep = reader.GetSafeValue<String>("clie_cep");
                cliente.clie_email = reader.GetSafeValue<String>("clie_email");
                cliente.clie_cobremail = reader.GetSafeValue<String>("clie_cobremail");
            }

            reader.Close();
            conn.Close();
            return cliente;

        }

        public Cliente ObterFornecedor(Int64 codFornecedor)
        {
            Cliente cliente = new Cliente();
            var conn = ObterConexaoBanco();

            String sql = "select * from fornecedores " +
            $"where forn_codigo = {codFornecedor}";

            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                cliente.clie_cnpj = reader.GetSafeValue<String>("forn_cnpj");
                cliente.clie_razaosocial = reader.GetSafeValue<String>("forn_razaosocial");
                cliente.clie_endereco = reader.GetSafeValue<String>("forn_endereco");
                cliente.clie_numero = reader.GetSafeValue<String>("forn_numero");
                cliente.clie_complemento = reader.GetSafeValue<String>("forn_complemento");
                cliente.clie_bairro = reader.GetSafeValue<String>("forn_bairro");
                cliente.clie_cidade = reader.GetSafeValue<String>("forn_cidade");
                cliente.clie_uf = reader.GetSafeValue<String>("forn_uf");
                cliente.clie_cep = reader.GetSafeValue<String>("forn_cep");
                cliente.clie_email = reader.GetSafeValue<String>("forn_email");
                cliente.clie_cobremail = reader.GetSafeValue<String>("forn_emailnfe");
            }

            reader.Close();
            conn.Close();
            return cliente;
        }
        
        public static IntegracaoBancariaDet ObterConfigDet(String codigo)
        {
            IntegracaoBancariaDet retorno = new IntegracaoBancariaDet();
            var conn = ObterConexaoBanco();

            String sql = $"select * from intbcodet where itbd_itbc_codigo = '{codigo}' order by itbd_codigo";

            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                retorno.Add(reader.GetSafeValue<String>("itbd_codigo"),
                    reader.GetSafeValue<String>("itbd_instrucao"),
                    reader.GetSafeValue<String>("itbd_valor"));
            }

            conn.Close();
            return retorno;

        }

        public static IntegracaoBancaria ObterIntegracaoBancaria(String codIntbco)
        {
            IntegracaoBancaria integracaoBancaria = new IntegracaoBancaria();
            var conn = ObterConexaoBanco();

            String sql = "select * from intbco " +
            $"where itbc_codigo = '{codIntbco}'";

            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                integracaoBancaria.itbc_codigo = reader.GetSafeValue<String>("itbc_codigo");
                integracaoBancaria.itbc_descricao = reader.GetSafeValue<String>("itbc_descricao");
                integracaoBancaria.itbc_banco = reader.GetSafeValue<String>("itbc_banco");
                integracaoBancaria.itbc_tpproc = reader.GetSafeValue<String>("itbc_tpproc");
                integracaoBancaria.itbc_pger_conta = reader.GetSafeValue<String>("itbc_pger_conta");
                integracaoBancaria.itbc_database = reader.GetSafeValue<String>("itbc_database");
                integracaoBancaria.itbc_tppend = reader.GetSafeValue<String>("itbc_tppend");
                integracaoBancaria.itbc_usua_codigo = reader.GetSafeValue<String>("itbc_usua_codigo");
                integracaoBancaria.itbc_arqdest = reader.GetSafeValue<String>("itbc_arqdest");
                integracaoBancaria.itbc_cont_geral = reader.GetSafeValue<String>("itbc_cont_geral");
                integracaoBancaria.itbc_cont_individual = reader.GetSafeValue<String>("itbc_cont_individual");
                integracaoBancaria.itbc_protesto = reader.GetSafeValue<String>("itbc_protesto");
            }

            reader.Close();
            conn.Close();
            return integracaoBancaria;
        }

        public static Int64 ObterValorSequencia(String nomeSequencia)
        {
            var conn = ObterConexaoBanco();
            conn.Open();

            String sqlCreate = $"CREATE SEQUENCE IF NOT EXISTS {nomeSequencia}";
            String sql = $"SELECT nextval('{nomeSequencia}')";

            NpgsqlCommand cmd = new NpgsqlCommand(sqlCreate, conn);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch(Exception e)
            {

            }
            finally
            {
                cmd.Dispose();
            }
            
            

            cmd = new NpgsqlCommand(sql, conn);
            Int64 seq = (Int64)cmd.ExecuteScalar();
            
            conn.Close();

            return seq;
        }

        public List<Pendfin> LerDadosPendfin(List<String> operacoes)
        {
            List<Pendfin> retorno = new List<Pendfin>();
            var conn = ObterConexaoBanco();

            String sql = "select pfin_operacao, pfin_transacao, pfin_status, pfin_pagrec, " +
            "pfin_datamvto, pfin_dataref, pfin_datavcto, pfin_datalanc, pfin_valor, pfin_empr_codigo, " +
            "pfin_tpagente, pfin_codagente, pfin_documento, pfin_parcela, pfin_contager, pfin_juros, " +
            "pfin_desconto, pfin_multa, pfin_cpfcnpj, pfin_codbarras, pfin_linhadig, pfin_nossonumero, " +
            "pfin_observacao, pfin_protesto, " +
            "pger_impr_codigo, pger_itbc_codigo, " +
            "itbc_descricao, itbc_banco, itbc_tpproc, itbc_cont_geral, itbc_cont_individual, itbc_protesto " +
            "from pendfin " +
            "inner join planoger on pger_conta = pfin_contager " +
            "inner join intbco on itbc_codigo = pger_itbc_codigo " +
            $"where pfin_operacao in ({string.Join(",", operacoes.Select(p => "'" + p + "'"))})";

            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                Pendfin pendfin = new Pendfin();
                retorno.Add(pendfin);

                pendfin.pfin_operacao        = reader.GetSafeValue<String>("pfin_operacao");
                pendfin.pfin_transacao       = reader.GetSafeValue<String>("pfin_transacao");
                pendfin.pfin_status          = reader.GetSafeValue<String>("pfin_status");
                pendfin.pfin_pagrec          = reader.GetSafeValue<String>("pfin_pagrec");
                pendfin.pfin_datamvto        = reader.GetSafeValue<DateTime?>("pfin_datamvto");
                pendfin.pfin_dataref         = reader.GetSafeValue<DateTime?>("pfin_dataref");
                pendfin.pfin_datavcto        = reader.GetSafeValue<DateTime?>("pfin_datavcto");
                pendfin.pfin_datalanc        = reader.GetSafeValue<DateTime?>("pfin_datalanc");
                pendfin.pfin_valor           = reader.GetSafeValue<Decimal>("pfin_valor");
                pendfin.pfin_empr_codigo     = reader.GetSafeValue<String>("pfin_empr_codigo");
                pendfin.pfin_tpagente        = reader.GetSafeValue<String>("pfin_tpagente");
                pendfin.pfin_codagente       = reader.GetSafeValue<Int64>("pfin_codagente");
                pendfin.pfin_documento       = reader.GetSafeValue<Int64>("pfin_documento");
                pendfin.pfin_parcela         = reader.GetSafeValue<Int64>("pfin_parcela");
                pendfin.pfin_contager        = reader.GetSafeValue<String>("pfin_contager");
                pendfin.pfin_juros           = reader.GetSafeValue<Decimal>("pfin_juros");
                pendfin.pfin_desconto        = reader.GetSafeValue<Decimal>("pfin_desconto"); 
                pendfin.pfin_multa           = reader.GetSafeValue<Decimal>("pfin_multa");
                pendfin.pfin_cpfcnpj         = reader.GetSafeValue<String>("pfin_cpfcnpj");
                pendfin.pfin_codbarras       = reader.GetSafeValue<String>("pfin_codbarras");
                pendfin.pfin_linhadig        = reader.GetSafeValue<String>("pfin_linhadig");
                pendfin.pfin_nossonumero     = reader.GetSafeValue<String>("pfin_nossonumero");
                pendfin.pfin_observacao      = reader.GetSafeValue<String>("pfin_observacao");
                pendfin.pfin_protesto        = reader.GetSafeValue<String>("pfin_protesto");
                pendfin.pger_impr_codigo     = reader.GetSafeValue<String>("pger_impr_codigo");
                pendfin.pger_itbc_codigo     = reader.GetSafeValue<String>("pger_itbc_codigo");
                pendfin.itbc_descricao       = reader.GetSafeValue<String>("itbc_descricao");
                pendfin.itbc_banco           = reader.GetSafeValue<String>("itbc_banco");
                pendfin.itbc_tpproc          = reader.GetSafeValue<String>("itbc_tpproc");
                pendfin.itbc_cont_geral      = reader.GetSafeValue<String>("itbc_cont_geral");
                pendfin.itbc_cont_individual = reader.GetSafeValue<String>("itbc_cont_individual");
                pendfin.itbc_protesto        = reader.GetSafeValue<String>("itbc_protesto");
            }

            conn.Close();
            return retorno;
        }

        private static NpgsqlConnection ObterConexaoBanco()
        {
            Utils.ConnectionServer regedit = Utils.GetIpServer();
            //String strConn = $"Server = {regedit.Ip}; Port = 5432; Database = {regedit.DatabaseName}; User Id = 'postgres';Password = 123456;Encoding=LATIN1";
            String strConn = $"Server = {regedit.Ip}; Port = {regedit.PortaDB}; Database = {regedit.DatabaseName}; User Id = 'postgres';Password=63A4C19D3A7D90628C9F24AB1D7F233B28B0BAFB;";
            return new NpgsqlConnection(strConn);
        }

        public static void RegistrarLog(String mensagem)
        {
            if (HabilitaLogs)
            {
                File.AppendAllText("Boleto2.Net.Logs.txt", $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] - {mensagem}{Environment.NewLine}");
            }
        }

    }
}

