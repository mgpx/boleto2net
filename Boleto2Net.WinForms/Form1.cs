using Boleto2.Net.Interativo;
using Boleto2Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boleto2.Net.WinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSicredi_Click(object sender, EventArgs e)
        {
            Boletos boletos = new Boletos();

            //Cabeçalho
            boletos.Banco = Banco.Instancia(Bancos.Sicredi);
            boletos.Banco.Cedente = new Cedente
            {
                CPFCNPJ = "10245481000159",
                Nome = "LORIDANI PATRICIA SOARES - ME",
                Observacoes = string.Empty,
                ContaBancaria = new ContaBancaria
                {
                    Posto = "16",
                    Agencia = "0167",
                    DigitoAgencia = "0",
                    OperacaoConta = string.Empty,
                    Conta = "6405",
                    DigitoConta = "2",

                    //DigitoConta = this.Conta.NumeroDigito,
                    CarteiraPadrao = String.Empty, // this.Conta.CarteiraBoleto,
                    VariacaoCarteiraPadrao = String.Empty, //this.Conta.VariacaoCarteira,
                    TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
                    TipoFormaCadastramento = TipoFormaCadastramento.ComRegistro,
                    TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa,
                    TipoDocumento = TipoDocumento.Tradicional
                },
                Codigo = "6405",
                CodigoDV = "2",
                CodigoTransmissao = string.Empty,
                Endereco = new Boleto2Net.Endereco
                {
                    LogradouroEndereco = "Rua Olinto Carlos Toigo",
                    LogradouroNumero = "2075",
                    LogradouroComplemento = String.Empty,
                    Bairro = "Parque Dos Pinheiros",
                    Cidade = "Flores da Cunha",
                    UF = "RS",
                    CEP = "95270000"
                }
            };

            boletos.Banco.FormataCedente();
            //Títulos

            var boleto = new Boleto(boletos.Banco);
            boleto.Sacado = new Sacado
            {
                CPFCNPJ = "89963102000116",
                Nome = "Supermercado Vermelhao Ltda",
                Observacoes = string.Empty,
                Endereco = new Boleto2Net.Endereco
                {
                    LogradouroEndereco = "Av 25 De Julho",
                    LogradouroNumero = "1333",
                    LogradouroComplemento = String.Empty,
                    Bairro = "Centro",
                    Cidade = "Flores da Cunha",
                    UF = "RS",
                    CEP = "95270000",
                }
            };

            boleto.CodigoOcorrencia = "01"; //Registrar remessa
            boleto.DescricaoOcorrencia = "Remessa Registrar";

            boleto.NumeroDocumento = "0";
            boleto.NumeroControleParticipante = String.Empty;


            boleto.DataEmissao = new DateTime(2018, 08, 10);
            boleto.DataVencimento = new DateTime(2018, 09, 09);
            //ano/bytegeracao/sequencial
            int seq = 0;
            boleto.NossoNumero = (boleto.DataEmissao.Year % 100).ToString() + "2" + seq.ToString().PadLeft(5, '0');

            boleto.ValorTitulo = 1m;
            boleto.Aceite = "N";
            boleto.EspecieDocumento = TipoEspecieDocumento.DM;
            boleto.Carteira = "A";
            boleto.VariacaoCarteira = String.Empty;

            //boleto.DataDesconto = DateTime.Today;
            //boleto.ValorDesconto = 0;
            //if (this.Conta.PercentualMulta > 0)
            //{
            //    boleto.DataMulta = cr.Vencimento.AddDays(1);
            //    boleto.PercentualMulta = this.Conta.PercentualMulta;
            //    boleto.ValorMulta = boleto.ValorTitulo * boleto.PercentualMulta / 100;

            //    boleto.MensagemInstrucoesCaixa = $"Cobrar Multa de {boleto.ValorMulta.FormatoMoeda()} após o vencimento.";
            //}

            //if (this.Conta.PercentualMora > 0)
            //{
            //    boleto.DataJuros = cr.Vencimento.AddDays(1);
            //    boleto.PercentualJurosDia = (this.Conta.PercentualMora / 30);
            //    boleto.ValorJurosDia = boleto.ValorTitulo * boleto.PercentualJurosDia / 100;

            //    string instrucao = $"Cobrar juros de {boleto.PercentualJurosDia.FormatoPorcentagem()} por dia de atraso";
            //    if (string.IsNullOrEmpty(boleto.MensagemInstrucoesCaixa))
            //        boleto.MensagemInstrucoesCaixa = instrucao;
            //    else
            //        boleto.MensagemInstrucoesCaixa += Environment.NewLine + instrucao;
            //}

            /*
            boleto.CodigoInstrucao1 = string.Empty;
            boleto.ComplementoInstrucao1 = string.Empty;

            boleto.CodigoInstrucao2 = string.Empty;
            boleto.ComplementoInstrucao2 = string.Empty;

            boleto.CodigoInstrucao3 = string.Empty;
            boleto.ComplementoInstrucao3 = string.Empty;                
            */

            boleto.CodigoProtesto = TipoCodigoProtesto.NaoProtestar;
            //boleto.CodigoProtesto = this.Conta.DiasProtesto == 0 ? TipoCodigoProtesto.NaoProtestar : TipoCodigoProtesto.ProtestarDiasuteis;
            //boleto.DiasProtesto = this.Conta.DiasProtesto;

            boleto.CodigoBaixaDevolucao = TipoCodigoBaixaDevolucao.NaoBaixarNaoDevolver;
            boleto.DiasBaixaDevolucao = 0;

            boleto.ValidarDados();
            boletos.Add(boleto);

            var boletoBancario = new BoletoBancario() { Boleto = boleto };
            var pdf = boletoBancario.MontaBytesPDF(false);
            var pathPDF = @"C:\Temp17\teste.pdf";
            File.WriteAllBytes(pathPDF, pdf);
            //Boleto2.Net.
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String aux = String.Empty;
            //Exported.GerarPDFOperacao("00100510151", true, @"C\Temp\", false, ref aux);
            Exported.GerarInfoBoleto("001", "2019-05-08", "60010", out aux);
        }

        private void btnNbcBank_Click(object sender, EventArgs e)
        {
            String aux = String.Empty;
            //Exported.GerarInfoBoleto("001", "2019-05-30", "1000", out aux);
            //Exported.GerarPDFTransacao("0010000151", true, @"C:\Temp\", false, ref aux);
            //Exported.GerarPDFTransacao("0010129472", true, @"C:\Temp\", false, ref aux);
            //Exported.GerarPDFTransacao("0010129480", true, @"C:\Temp\", false, ref aux);
            Exported.GerarPDFTransacao("0010145486", false, @"C:\Temp\", false, ref aux);
        }

        private void btnGerarSicoob_Click(object sender, EventArgs e)
        {
            var respsta = BoletoAvulso.GerarBoletoSicoob("3226", "9206", "1", 450, new DateTime(2018, 05, 06), "0000002", 1);
        }



        private void button2_Click(object sender, EventArgs e)
        {
            String retorno = "";
            //Exported.GerarPDFOperacao(this.textBox1.Text, true, @"C:\Temp\", false, ref retorno);
            //Exported.GerarInfoBoleto("003", "2021-01-16", "1000", out retorno);
            //Exported.GerarPDFTransacao(this.textBox1.Text, false, @"C:\Temp\", true, ref retorno);
            Exported.GerarPDFTransacao(this.textBox1.Text, true, @"C:\Temp", this.cbEnviarEmail.Checked, ref retorno);
            
            if (!String.IsNullOrEmpty(retorno))
            MessageBox.Show("Retorno: " + retorno);

            //var contaBancaria = new ContaBancaria
            //{
            //    Agencia = "3037",
            //    DigitoAgencia = "6",
            //    Conta = "108785",
            //    DigitoConta = "1",
            //    CarteiraPadrao = "1",
            //    VariacaoCarteiraPadrao = "01",
            //    TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
            //    TipoFormaCadastramento = TipoFormaCadastramento.ComRegistro,
            //    TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa
            //};
            //var _banco = Banco.Instancia(Bancos.Sicoob);
            //_banco.Cedente = GerarCedente("108785", "1", "", contaBancaria);
            //_banco.FormataCedente();

            //var boleto = new Boleto(_banco)
            //{
            //    DataVencimento = new DateTime(2022,08,24),
            //    ValorTitulo = 457.30m,
            //    NossoNumero = "0000509",
            //    NumeroDocumento = "16",
            //    EspecieDocumento = TipoEspecieDocumento.DM,
            //    Sacado = new Sacado()
            //};

            ////Ação
            //boleto.ValidarDados();

            ////Assertivas
            //Assert.That(boleto.CodigoBarra.DigitoVerificador, Is.EqualTo(digitoVerificador), $"Dígito Verificador diferente de {digitoVerificador}");
            //Assert.That(boleto.NossoNumeroFormatado, Is.EqualTo(nossoNumeroFormatado), "Nosso número inválido");
            //Assert.That(boleto.CodigoBarra.CodigoDeBarras, Is.EqualTo(codigoDeBarras), "Código de Barra inválido");
            //Assert.That(boleto.CodigoBarra.LinhaDigitavel, Is.EqualTo(linhaDigitavel), "Linha digitável inválida");

        }

        internal static Cedente GerarCedente(string codigoCedente, string digitoCodigoCedente, string codigoTransmissao, ContaBancaria contaBancaria)
        {
            return new Cedente
            {
                CPFCNPJ = "86.875.666/0001-09",
                Nome = "Cedente Teste",
                Codigo = codigoCedente,
                CodigoDV = digitoCodigoCedente,
                Endereco = new Endereco
                {
                    LogradouroEndereco = "Rua Teste do Cedente",
                    LogradouroNumero = "789",
                    LogradouroComplemento = "Cj 333",
                    Bairro = "Bairro",
                    Cidade = "Cidade",
                    UF = "SP",
                    CEP = "65432987"
                },
                ContaBancaria = contaBancaria
            };
        }

        internal static Sacado GerarSacado()
        {
            return new Sacado
            {
                CPFCNPJ = "443.316.101-28",
                Nome = "Sacado Teste PF",
                Observacoes = "Matricula 678/9",
                Endereco = new Endereco
                {
                    LogradouroEndereco = "Rua Testando",
                    LogradouroNumero = "456",
                    Bairro = "Bairro",
                    Cidade = "Cidade",
                    UF = "SP",
                    CEP = "56789012"
                }
            };
           
        }

        private void btnTeste_Click(object sender, EventArgs e)
        {
            String ret = "";
            //Exported.GerarInfoBoleto("001", "2020-10-21", "4535", out ret);
            Exported.GerarPDFOperacao("00100002676", true, @"C:\Interativo\BltExp\", false, ref ret);
        }

        private void btnTesteGerar_Click(object sender, EventArgs e)
        {
            String ret = String.Empty;
            Exported.GerarInfoBoleto("001", "2024-01-01", "1000", out ret);

            MessageBox.Show(ret);
        }
    }
}
