using Boleto2Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Boleto2.Net.Interativo.BoletoFuncInterativo;

namespace Boleto2.Net.Interativo
{
    public class BoletoAvulso
    {
        public static RetornoBoleto GerarBoletoSicoob(String cooperativa, String codCliente, String dvCodCliente, Decimal valorTitulo, DateTime vcto, String nossoNumero, Int32 parcela)
        {
            var bancoSicoob = Banco.Instancia(Bancos.Sicoob);
            bancoSicoob.Cedente = new Cedente();
            bancoSicoob.Cedente.Codigo = codCliente;
            bancoSicoob.Cedente.CodigoDV = dvCodCliente;
            bancoSicoob.Cedente.ContaBancaria = new ContaBancaria
            {
                Posto = "16",
                Agencia = cooperativa,
                DigitoAgencia = "0",
                OperacaoConta = string.Empty,
                Conta = "6405",
                DigitoConta = "2",
                
                //DigitoConta = this.Conta.NumeroDigito,
                CarteiraPadrao = "1",
                VariacaoCarteiraPadrao = "01",
                TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
                TipoFormaCadastramento = TipoFormaCadastramento.ComRegistro,
                TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa,
                TipoDocumento = TipoDocumento.Tradicional
            };
            Boleto boleto = new Boleto(bancoSicoob);
            boleto.Parcela = parcela;
            boleto.ValorTitulo = valorTitulo;
            boleto.DataVencimento = vcto;
            boleto.NossoNumero = nossoNumero;
            boleto.Carteira = "1";
            boleto.VariacaoCarteira = "01";
            
            
            //boleto.VariacaoCarteira

            // boleto.ValorTitulo = valor;
            //boleto.DataVencimento = DateTime.ParseExact(vcto, "yyyy-MM-dd", null);
            //boleto.NossoNumero = seqNossoNumero.ToString();
            //boleto.Carteira = "109";


            boleto.Banco.FormataCedente();

            //calcula tudo o que precisa
            boleto.ValidarDados();

            return new RetornoBoleto()
            {
                CodigoDeBarras = boleto.CodigoBarra.LinhaDigitavel,
                LinhaDigitavel = boleto.CodigoBarra.CodigoDeBarras,
                NossoNumero = boleto.NossoNumero,
                NossoNumeroDV = boleto.NossoNumeroDV,
                NossoNumeroFormatado = boleto.NossoNumeroFormatado,
            };
        }

        public static RetornoBoleto GerarBoletoBanrisul(String codAgencia, String codBenificiario,  Decimal valorTitulo, DateTime vcto, String nossoNumero)
        {
            var bancoSicoob = Banco.Instancia(Bancos.Banrisul);
            bancoSicoob.Cedente = new Cedente();
            bancoSicoob.Cedente.Codigo = codBenificiario;
            bancoSicoob.Cedente.CodigoDV = "0";
            bancoSicoob.Cedente.ContaBancaria = new ContaBancaria
            {
                Posto = "16",
                Agencia = codAgencia,
                DigitoAgencia = "0",
                OperacaoConta = string.Empty,
                Conta = codBenificiario.PadLeft(8, '0').Substring(0, 8),
                DigitoConta = "",

                //DigitoConta = this.Conta.NumeroDigito,
                CarteiraPadrao = "1",
                //VariacaoCarteiraPadrao = "01",
                TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
                TipoFormaCadastramento = TipoFormaCadastramento.ComRegistro,
                TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa,
                TipoDocumento = TipoDocumento.Tradicional
            };
            Boleto boleto = new Boleto(bancoSicoob);
            boleto.Parcela = 1;
            boleto.ValorTitulo = valorTitulo;
            boleto.DataVencimento = vcto;
            boleto.NossoNumero = nossoNumero;
            boleto.Carteira = "1";
            //boleto.VariacaoCarteira = "01";


            //boleto.VariacaoCarteira

            // boleto.ValorTitulo = valor;
            //boleto.DataVencimento = DateTime.ParseExact(vcto, "yyyy-MM-dd", null);
            //boleto.NossoNumero = seqNossoNumero.ToString();
            //boleto.Carteira = "109";


            boleto.Banco.FormataCedente();

            //calcula tudo o que precisa
            boleto.ValidarDados();

            return new RetornoBoleto()
            {
                LinhaDigitavel = boleto.CodigoBarra.LinhaDigitavel,
                CodigoDeBarras = boleto.CodigoBarra.CodigoDeBarras,
                NossoNumero = boleto.NossoNumero,
                NossoNumeroDV = boleto.NossoNumeroDV,
                NossoNumeroFormatado = boleto.NossoNumeroFormatado,
            };
        }

        public static RetornoBoleto GerarBoletoSicredi(String codAgencia, String codBenificiario, Decimal valorTitulo, DateTime vcto, String nossoNumero, String byteGeracao = "2")
        {
            var bancoSicredi = Banco.Instancia(Bancos.Sicredi);
            bancoSicredi.Cedente = new Cedente();
            bancoSicredi.Cedente.Codigo = codBenificiario;
            bancoSicredi.Cedente.CodigoDV = "0";
            

            if (codBenificiario.Length == 5)
            {
                bancoSicredi.Cedente.Codigo = codBenificiario.Substring(0, 4);
                bancoSicredi.Cedente.CodigoDV = codBenificiario.Substring(4, 1);
            }
            
            bancoSicredi.Cedente.ContaBancaria = new ContaBancaria
            {
                Posto = "31",
                Agencia = codAgencia,
                DigitoAgencia = "0",
                OperacaoConta = string.Empty,
                Conta = codBenificiario.PadLeft(8, '0').Substring(0, 8),
                DigitoConta = "",

                //DigitoConta = this.Conta.NumeroDigito,
                CarteiraPadrao = "A",
                //VariacaoCarteiraPadrao = "01",
                TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,//TipoCarteira.CarteiraCobrancaSimples,
                //TipoFormaCadastramento = TipoFormaCadastramento.ComRegistro,
                TipoFormaCadastramento = TipoFormaCadastramento.SemRegistro,
                TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa,
                TipoDocumento = TipoDocumento.Tradicional
            };
            Boleto boleto = new Boleto(bancoSicredi);
            boleto.ByteNossoNumero = byteGeracao;
            boleto.Parcela = 1;
            boleto.ValorTitulo = valorTitulo;
            boleto.DataVencimento = vcto;
            boleto.NossoNumero = nossoNumero;
            boleto.Carteira = "01";
            //boleto.VariacaoCarteira = "01";


            //boleto.VariacaoCarteira

            // boleto.ValorTitulo = valor;
            //boleto.DataVencimento = DateTime.ParseExact(vcto, "yyyy-MM-dd", null);
            //boleto.NossoNumero = seqNossoNumero.ToString();
            //boleto.Carteira = "109";


            boleto.Banco.FormataCedente();

            //calcula tudo o que precisa
            boleto.ValidarDados();

            return new RetornoBoleto()
            {
                LinhaDigitavel = boleto.CodigoBarra.LinhaDigitavel,
                CodigoDeBarras = boleto.CodigoBarra.CodigoDeBarras,
                NossoNumero = boleto.NossoNumero,
                NossoNumeroDV = boleto.NossoNumeroDV,
                NossoNumeroFormatado = boleto.NossoNumeroFormatado,
            };
        }
    }
}
