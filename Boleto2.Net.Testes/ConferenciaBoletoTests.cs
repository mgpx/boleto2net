using System;
using Boleto2.Net.Interativo;
using Boleto2.Net.Interativo.Entidades;
using Boleto2Net;
using NUnit.Framework;

namespace Boleto2Net.Testes
{
    [TestFixture]
    public class ConferenciaBoletoTests
    {
        [TestCase(756, "00000204", "0000020", "4")]
        [TestCase(748, "26/200020-4", "26200020", "4")]
        [TestCase(341, "109/12345678-9", "12345678", "9")]
        [TestCase(237, "0912345678901P", "12345678901", "P")]
        [TestCase(753, "019/12345678901-0", "12345678901", "0")]
        [TestCase(41, "12345678-90", "12345678", "90")]
        [TestCase(1, "12345671234567890", "12345671234567890", "")]
        public void SeparaNumeroEDigitoSemPerderZeros(int banco, string original, string numero, string dv)
        {
            var entrada = NumeroBoletoConferencia.Ler(banco, original);
            Assert.That(entrada.Numero, Is.EqualTo(numero));
            Assert.That(entrada.Digito, Is.EqualTo(dv));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("0000020X")]
        public void NaoOcultaNossoNumeroInvalido(string numero)
        {
            Assert.Throws<ArgumentException>(() => NumeroBoletoConferencia.Ler(756, numero));
        }

        [Test]
        public void ComparaLinhaIgnorandoSomenteFormatacao()
        {
            Assert.That(CampoConferencia.Comparar("Linha digitável", "123.45 678", "12345678", true).Resultado,
                Is.EqualTo("Confere"));
            Assert.That(CampoConferencia.Comparar("Linha digitável", "123X45678", "12345678", true).Resultado,
                Is.EqualTo("Divergente"));
            Assert.That(CampoConferencia.Comparar("Código de barras", null, "123", false).Resultado,
                Is.EqualTo("Ausente"));
            Assert.That(CampoConferencia.Comparar("Código de barras", "00123", "123", false).Resultado,
                Is.EqualTo("Divergente"));
        }

        [Test]
        public void SicoobMostraCalculoMesmoComDigitoERPIncorreto()
        {
            var titulo = new Pendfin
            {
                itbc_banco = "756", pfin_nossonumero = "00000205", pfin_parcela = 1,
                pfin_datavcto = new DateTime(2026, 10, 12), pfin_datalanc = new DateTime(2026, 10, 1),
                pfin_valor = 1515.80m,
                pfin_codbarras = "75699159700001515801308101081571300000204001",
                pfin_linhadig = "75691308130108157130700002040012915970000151580"
            };
            var cliente = new Cliente { clie_cnpj = "44331610128" };
            var boleto = BoletoInterativo.MontarBoleto(titulo, new Empresa(), cliente,
                SicoobInterativoTests.Configuracao(), SicoobInterativoTests.Integracao(), false);
            var resultado = ConferenciaBoletoService.Comparar(titulo, boleto);
            Assert.That(resultado.Campos[0].ERP, Is.EqualTo("00000205"));
            Assert.That(resultado.Campos[0].Calculado, Is.EqualTo("0000020-4"));
            Assert.That(resultado.Campos[0].Resultado, Is.EqualTo("Divergente"));
            Assert.That(resultado.Campos[1].Resultado, Is.EqualTo("Confere"));
            Assert.That(resultado.Campos[2].Resultado, Is.EqualTo("Confere"));
            Assert.That(resultado.Confere, Is.False);
            Assert.Throws<ArgumentException>(() => BoletoInterativo.MontarBoleto(titulo, new Empresa(), cliente,
                SicoobInterativoTests.Configuracao(), SicoobInterativoTests.Integracao(), true));
        }

        [TestCase(756, "1", "01", "0000020", "00000209")]
        [TestCase(748, "A", "", "26200020", "262000209")]
        [TestCase(341, "109", "", "12345678", "109/12345678-9")]
        [TestCase(237, "09", "", "12345678901", "09123456789019")]
        [TestCase(753, "019", "", "12345678901", "019/12345678901-0")]
        [TestCase(41, "1", "", "12345678", "12345678-99")]
        [TestCase(1, "17", "019", "12345671234567890", "12345671234567890")]
        public void ConfereCalculoParaOsSeteBancos(int codigo, string carteira, string variacao, string numero, string numeroERP)
        {
            var banco = Banco.Instancia(codigo);
            banco.Cedente = new Cedente
            {
                Codigo = codigo == 1 ? "1234567" : codigo == 756 ? "081571" : "1234",
                CodigoDV = "3",
                ContaBancaria = new ContaBancaria
                {
                    Agencia = "3081", Posto = "01", Conta = codigo == 341 ? "12345" : codigo == 237 || codigo == 753 ? "1234567" : "12345678", DigitoConta = "0",
                    CarteiraPadrao = carteira, VariacaoCarteiraPadrao = variacao
                }
            };
            var boleto = new Boleto(banco)
            {
                NossoNumero = NumeroBoletoConferencia.Ler(codigo, numeroERP).Numero,
                ValorTitulo = 100m, DataVencimento = DateTime.Today.AddDays(30), DataEmissao = DateTime.Today
            };
            boleto.ValidarDadosChaves("", "");
            ConferenciaBoletoService.RecalcularSicredi(boleto);
            Assert.That(boleto.CodigoBarra.CodigoDeBarras.Length, Is.EqualTo(44));
            var titulo = new Pendfin
            {
                pfin_nossonumero = numeroERP, pfin_codbarras = boleto.CodigoBarra.CodigoDeBarras,
                pfin_linhadig = boleto.CodigoBarra.LinhaDigitavel
            };
            var resultado = ConferenciaBoletoService.Comparar(titulo, boleto);
            Assert.That(resultado.Campos[1].Resultado, Is.EqualTo("Confere"));
            Assert.That(resultado.Campos[2].Resultado, Is.EqualTo("Confere"));
            if (codigo != 1)
                Assert.That(resultado.Campos[0].Resultado, Is.EqualTo("Divergente"), "O dígito fictício do ERP não pode ser aceito como cálculo.");
            else
                Assert.That(resultado.Campos[0].Resultado, Is.EqualTo("Confere"));
        }

        [TestCase("")]
        [TestCase("123'; drop table pendfin;--")]
        public void RejeitaConsultaInvalidaSemConectar(string identificador)
        {
            Assert.Throws<ArgumentException>(() => new ConferenciaBoletoService()
                .ConsultarTitulos(TipoConsultaBoleto.Operacao, identificador));
        }

        [Test]
        public void NaoDeclaraDigitoNaoArmazenadoComoConferido()
        {
            var banco = Banco.Instancia(341);
            banco.Cedente = new Cedente
            {
                ContaBancaria = new ContaBancaria { Agencia = "1234", Conta = "12345", DigitoConta = "0", CarteiraPadrao = "109" }
            };
            var boleto = new Boleto(banco)
            {
                NossoNumero = "12345678", ValorTitulo = 100m, DataVencimento = DateTime.Today.AddDays(30)
            };
            boleto.ValidarDadosChaves("", "");
            var titulo = new Pendfin
            {
                pfin_nossonumero = "12345678", pfin_codbarras = boleto.CodigoBarra.CodigoDeBarras,
                pfin_linhadig = boleto.CodigoBarra.LinhaDigitavel
            };
            Assert.That(ConferenciaBoletoService.Comparar(titulo, boleto).Campos[0].Resultado, Is.EqualTo("Ausente"));
        }
    }
}
