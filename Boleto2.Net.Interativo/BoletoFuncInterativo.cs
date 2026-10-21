using Boleto2Net;
using Boleto2Net.Extensions;
using Boleto2.Net.Interativo.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boleto2.Net.Interativo
{
    public class BoletoFuncInterativo
    {
        
        public static RetornoBoleto ObterDados(String codIntegracaoBancaria, String vcto, Decimal valor)
        {
            //System.Windows.Forms.MessageBox.Show("Método ObterDados");
            var integracaoBancaria = BoletoInterativo.ObterIntegracaoBancaria(codIntegracaoBancaria);
            //System.Windows.Forms.MessageBox.Show("Carregou dados Integracao bancarria");
            var det = BoletoInterativo.ObterConfigDet(codIntegracaoBancaria);
            return ObterDados(integracaoBancaria, det, vcto, valor, BoletoInterativo.ObterValorSequencia);
        }

        internal static RetornoBoleto ObterDados(IntegracaoBancaria integracaoBancaria,
            IntegracaoBancariaDet det, string vcto, decimal valor, Func<string, long> obterSequencia)
        {
            //System.Windows.Forms.MessageBox.Show("Carregou dados Integracao bancarria det");
            var enumBanco = (Bancos)Convert.ToUInt16(integracaoBancaria.itbc_banco);
            //enumBanco = Bancos.Itau;
            Boletos boletos = new Boletos();
            String nomeSeq = integracaoBancaria.itbc_cont_individual;
            if (enumBanco == Bancos.Sicredi)
                nomeSeq += DateTime.Now.ToString("yyyy");

            BoletoInterativo.RegistrarLog($"GerarInfoBoleto Seq: {nomeSeq}");

            // No Sicoob, valide o cadastro e o vencimento antes de consumir a sequência.
            if (enumBanco == Bancos.Sicoob)
            {
                boletos.Banco = BoletoInterativo.GerarBoletoInfoCedente(integracaoBancaria, det);
                DateTime.ParseExact(vcto, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
                    .FatorVencimento();
                if (valor <= 0 || valor > 99999999.99m)
                    throw new ArgumentException("Sicoob: valor do título deve ser positivo e caber nas dez posições do código de barras.");
                if (string.IsNullOrWhiteSpace(nomeSeq))
                    throw new ArgumentException("Sicoob: sequência do nosso número não configurada na integração bancária.");
            }

            //System.Windows.Forms.MessageBox.Show("Obtendo sequencia");
            Int64 seqNossoNumero = obterSequencia(nomeSeq);
            //Int64 seqNossoNumero = 19;



            //boletos.Banco = Banco.Instancia((Bancos)Convert.ToUInt16(integracaoBancaria.itbc_banco));
            //var boleto = new Boleto(boletos.Banco);

            //System.Windows.Forms.MessageBox.Show("Gerando informacoes cedente");
            if (enumBanco != Bancos.Sicoob)
                boletos.Banco = BoletoInterativo.GerarBoletoInfoCedente(integracaoBancaria, det);
            else
                SicoobIntegracao.ValidarSequencia(seqNossoNumero);

            //System.Windows.Forms.MessageBox.Show("Gerando info boleto");
            Boleto boleto = new Boleto(boletos.Banco);
            boleto.ValorTitulo = valor;
            boleto.DataVencimento = DateTime.ParseExact(vcto, "yyyy-MM-dd", null);
            boleto.NossoNumero = seqNossoNumero.ToString();
            //boleto.Carteira = "109";


            if (enumBanco == Bancos.Sicoob)
                SicoobIntegracao.AplicarConfiguracaoTitulo(boleto, det);
            else
                boletos.Banco.FormataCedente();

            //calcula tudo o que precisa
            boleto.ValidarDados();

            String linhaDigitavel = boleto.CodigoBarra.LinhaDigitavel;
            String codigoDeBarras = boleto.CodigoBarra.CodigoDeBarras;
            String nossoNumero    = boleto.NossoNumero;
            String nossoNumeroDV  = boleto.NossoNumeroDV;
            String nossoNumeroFormatado = boleto.NossoNumeroFormatado;

            //boleto.ValidarDadosChaves(String.Empty, "");
            //boleto.Banco.FormataCodigoBarraCampoLivre(boleto);

            return new RetornoBoleto()
            {
                CodigoDeBarras = codigoDeBarras,
                LinhaDigitavel = linhaDigitavel,
                NossoNumero = nossoNumero,
                NossoNumeroDV = nossoNumeroDV,
                NossoNumeroFormatado = nossoNumeroFormatado,
            };
            
        }

        public class RetornoBoleto
        {
            public String LinhaDigitavel { get; set; }
            public String CodigoDeBarras { get; set; }
            public String NossoNumero { get; set; }
            public String NossoNumeroDV { get; set; }
            public String NossoNumeroFormatado { get; set; }


        }


    }
}
