using Boleto2Net;
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
            //System.Windows.Forms.MessageBox.Show("Carregou dados Integracao bancarria det");
            var enumBanco = (Bancos)Convert.ToUInt16(integracaoBancaria.itbc_banco);
            //enumBanco = Bancos.Itau;
            Boletos boletos = new Boletos();
            String nomeSeq = integracaoBancaria.itbc_cont_individual;
            if (enumBanco == Bancos.Sicredi)
                nomeSeq += DateTime.Now.ToString("yyyy");

            //System.Windows.Forms.MessageBox.Show("Obtendo sequencia");
            Int64 seqNossoNumero = BoletoInterativo.ObterValorSequencia(nomeSeq);
            //boletos.Banco = Banco.Instancia((Bancos)Convert.ToUInt16(integracaoBancaria.itbc_banco));
            //var boleto = new Boleto(boletos.Banco);

            //System.Windows.Forms.MessageBox.Show("Gerando informacoes cedente");
            boletos.Banco = BoletoInterativo.GerarBoletoInfoCedente(integracaoBancaria, det);

            //System.Windows.Forms.MessageBox.Show("Gerando info boleto");
            Boleto boleto = new Boleto(boletos.Banco);
            boleto.ValorTitulo = valor;
            boleto.DataVencimento = DateTime.ParseExact(vcto, "yyyy-MM-dd", null);
            boleto.NossoNumero = seqNossoNumero.ToString();
            //boleto.Carteira = "109";


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
