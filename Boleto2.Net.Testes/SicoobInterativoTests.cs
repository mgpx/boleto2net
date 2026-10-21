using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Boleto2.Net.Interativo;
using Boleto2.Net.Interativo.Entidades;
using NUnit.Framework;

namespace Boleto2Net.Testes
{
    [TestFixture]
    [NonParallelizable]
    public class SicoobInterativoTests
    {
        internal static IntegracaoBancaria Integracao()
            => new IntegracaoBancaria { itbc_banco = "756", itbc_tpproc = "011" };

        internal static IntegracaoBancariaDet Configuracao(string substituir = null, string valor = null)
        {
            var det = new IntegracaoBancariaDet();
            var valores = new Dictionary<string, string>
            {
                ["001"] = "34645626000177", ["002"] = "3081", ["003"] = "3",
                ["004"] = "0815713", ["005"] = "Beneficiário de teste",
                ["009"] = "1", ["010"] = "01", ["011"] = "0",
                ["012"] = "2", ["013"] = "2", ["016"] = "02", ["017"] = "N",
                ["024"] = "PAGÁVEL PREFERENCIALMENTE NO SICOOB",
                ["025"] = "44871", ["026"] = "0", ["027"] = "99999", ["028"] = "9",
                ["101"] = "APÓS O VENCIMENTO COBRAR MORA DE 2% AO MÊS",
                ["102"] = "APÓS O VENCIMENTO COBRAR MULTA DE 0.30% SOBRE O VALOR",
                ["103"] = "", ["104"] = "", ["105"] = "Instrução adicional",
                ["106"] = "Última instrução"
            };
            if (substituir != null)
                valores[substituir] = valor;
            foreach (var item in valores)
                det.Add(item.Key, item.Key, item.Value);
            return det;
        }

        [Test]
        public void MontaBeneficiarioComParametrosDoERP()
        {
            var banco = BoletoInterativo.GerarBoletoInfoCedente(Integracao(), Configuracao());
            Assert.That(banco.Codigo, Is.EqualTo(756));
            Assert.That(banco.Cedente.Codigo, Is.EqualTo("081571"));
            Assert.That(banco.Cedente.CodigoDV, Is.EqualTo("3"));
            Assert.That(banco.Cedente.ContaBancaria.Agencia, Is.EqualTo("3081"));
            Assert.That(banco.Cedente.ContaBancaria.DigitoAgencia, Is.EqualTo("3"));
            Assert.That(banco.Cedente.ContaBancaria.Conta, Is.EqualTo("00044871"));
            Assert.That(banco.Cedente.ContaBancaria.DigitoConta, Is.EqualTo("0"));
            Assert.That(banco.Cedente.ContaBancaria.CarteiraComVariacaoPadrao, Is.EqualTo("1/01"));
            Assert.That(banco.Cedente.ContaBancaria.LocalPagamento, Is.EqualTo("PAGÁVEL PREFERENCIALMENTE NO SICOOB"));
            Assert.That(banco.Cedente.ContaBancaria.TipoFormaCadastramento, Is.EqualTo(TipoFormaCadastramento.ComRegistro));
        }

        [TestCase("004", "", "004")]
        [TestCase("004", "081571X", "004")]
        [TestCase("004", "12345678", "004")]
        [TestCase("009", "2", "carteira")]
        [TestCase("010", "02", "carteira")]
        [TestCase("002", "12345", "002")]
        [TestCase("026", "", "026")]
        [TestCase("012", "0", "012")]
        public void RejeitaConfiguracaoInvalida(string codigo, string valor, string mensagem)
        {
            var erro = Assert.Throws<ArgumentException>(() =>
                BoletoInterativo.GerarBoletoInfoCedente(Integracao(), Configuracao(codigo, valor)));
            Assert.That(erro.Message.ToLowerInvariant(), Does.Contain(mensagem));
        }

        [TestCase("0000020", "4", 1515.80, 2026, 10, 12, "75699159700001515801308101081571300000204001")]
        [TestCase("0000019", "6", 994.30, 2026, 10, 8, "75696159300000994301308101081571300000196001")]
        [TestCase("0000018", "9", 1460.50, 2026, 9, 9, "75691156400001460501308101081571300000189001")]
        [TestCase("0000017", "1", 916.00, 2026, 8, 5, "75696152900000916001308101081571300000171001")]
        [TestCase("0000016", "4", 1414.70, 2026, 7, 28, "75692152100001414701308101081571300000164001")]
        [TestCase("0000015", "7", 1388.50, 2026, 7, 7, "75697150000001388501308101081571300000157001")]
        public void ReproduzCodigoDeBarrasDoERP(string numero, string dv, decimal valor, int ano, int mes, int dia, string barras)
        {
            var banco = BoletoInterativo.GerarBoletoInfoCedente(Integracao(), Configuracao());
            var boleto = new Boleto(banco)
            {
                NossoNumero = numero, ValorTitulo = valor,
                DataVencimento = new DateTime(ano, mes, dia), Sacado = Utils.GerarSacado()
            };
            boleto.ValidarDados();
            Assert.That(boleto.NossoNumeroDV, Is.EqualTo(dv));
            Assert.That(boleto.CodigoBarra.CodigoDeBarras, Is.EqualTo(barras));
        }

        [Test]
        public void GeraPDFDeTituloExistenteSemConsultarBancoDeDados()
        {
            var pasta = Path.Combine(TestContext.CurrentContext.WorkDirectory, "sicoob-pdf");
            var gerador = new BoletoInterativo(new List<string>());
            gerador.SetDirBase(pasta);
            var pendfin = new Pendfin
            {
                pfin_operacao = "sicoob-teste", itbc_banco = "756",
                pfin_nossonumero = "00000204", pfin_parcela = 1, pfin_valor = 1515.80m,
                pfin_datavcto = new DateTime(2026, 10, 12), pfin_datalanc = new DateTime(2026, 10, 1),
                pfin_codbarras = "75699159700001515801308101081571300000204001",
                pfin_linhadig = "75691308130108157130700002040012915970000151580"
            };
            var empresa = new Empresa
            {
                empr_nome = "Beneficiário de teste", empr_cnpj = "34645626000177",
                empr_endereco = "Rua de teste", empr_numero = "123", empr_bairro = "Centro",
                empr_cidade = "Caxias do Sul", empr_uf = "RS", empr_cep = "95010000"
            };
            var cliente = new Cliente
            {
                clie_razaosocial = "Pagador de teste", clie_cnpj = "44331610128",
                clie_endereco = "Rua do pagador", clie_numero = "456", clie_bairro = "Centro",
                clie_cidade = "Caxias do Sul", clie_uf = "RS", clie_cep = "95010000"
            };
            gerador.GerarBoletoPDF(pendfin, empresa, cliente, Configuracao(), Integracao());
            var caminho = Path.Combine(pasta, "bltsicoob-teste.pdf");
            Assert.That(File.Exists(caminho), Is.True, "O PDF do Sicoob deve ser gerado.");
            Assert.That(System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(caminho), 0, 5), Is.EqualTo("%PDF-"));
        }

        private static Pendfin Titulo()
            => new Pendfin
            {
                pfin_nossonumero = "00000204", pfin_parcela = 1,
                pfin_codbarras = "75699159700001515801308101081571300000204001",
                pfin_linhadig = "75691308130108157130700002040012915970000151580"
            };

        private static Boleto BoletoExistente()
            => new Boleto(BoletoInterativo.GerarBoletoInfoCedente(Integracao(), Configuracao()))
            {
                ValorTitulo = 1515.80m, DataVencimento = new DateTime(2026, 10, 12)
            };

        [Test]
        public void PreservaParcelaEDadosDoTituloExistente()
        {
            var boleto = BoletoExistente();
            var titulo = Titulo();
            titulo.pfin_parcela = 2;
            titulo.pfin_codbarras = "75697159700001515801308101081571300000204002";
            titulo.pfin_linhadig = "75691.30813 01081.571307 00002.040020 7 15970000151580";
            SicoobIntegracao.PrepararTituloExistente(boleto, titulo, Configuracao());
            Assert.That(boleto.Parcela, Is.EqualTo(2));
            Assert.That(boleto.NossoNumero, Is.EqualTo("0000020"));
            Assert.That(boleto.NossoNumeroDV, Is.EqualTo("4"));
            Assert.That(boleto.EspecieDocumento, Is.EqualTo(TipoEspecieDocumento.DM));
            Assert.That(boleto.Aceite, Is.EqualTo("N"));
            Assert.That(boleto.MensagemInstrucoesCaixa, Is.EqualTo(
                "APÓS O VENCIMENTO COBRAR MORA DE 2% AO MÊS<br/>APÓS O VENCIMENTO COBRAR MULTA DE 0.30% SOBRE O VALOR<br/>Instrução adicional<br/>Última instrução"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("0000020")]
        [TestCase("00000204X")]
        [TestCase("0000020X")]
        [TestCase("00000205")]
        public void RejeitaNossoNumeroArmazenadoInvalido(string numero)
        {
            var boleto = BoletoExistente();
            var titulo = Titulo();
            titulo.pfin_nossonumero = numero;
            Assert.Throws<ArgumentException>(() => SicoobIntegracao.PrepararTituloExistente(boleto, titulo, Configuracao()));
        }

        [TestCase(0)]
        [TestCase(1000)]
        public void RejeitaParcelaInvalida(int parcela)
        {
            var boleto = BoletoExistente();
            var titulo = Titulo();
            titulo.pfin_parcela = parcela;
            Assert.Throws<ArgumentException>(() => SicoobIntegracao.PrepararTituloExistente(boleto, titulo, Configuracao()));
        }

        [TestCase(null, "75691308130108157130700002040012915970000151580")]
        [TestCase("75699159700001515801308101081571300000204002", "75691308130108157130700002040012915970000151580")]
        [TestCase("75699159700001515801308101081571300000204001", null)]
        [TestCase("75699159700001515801308101081571300000204001", "75691308130108157130700002040012915970000151581")]
        [TestCase("75699159700001515801308101081571300000204001", "75691308130108157130700002040012915970000151580X")]
        public void RejeitaChavesAusentesOuDivergentes(string barras, string linha)
        {
            var boleto = BoletoExistente();
            var titulo = Titulo();
            titulo.pfin_codbarras = barras;
            titulo.pfin_linhadig = linha;
            Assert.Throws<ArgumentException>(() => SicoobIntegracao.PrepararTituloExistente(boleto, titulo, Configuracao()));
        }

        [Test]
        public void GeraDadosNovosComSequenciaControlada()
        {
            var integracao = Integracao();
            integracao.itbc_cont_individual = "sequencia_teste";
            var chamadas = 0;
            var retorno = BoletoFuncInterativo.ObterDados(integracao, Configuracao(), "2026-10-12", 1515.80m, nome =>
            {
                Assert.That(nome, Is.EqualTo("sequencia_teste"));
                chamadas++;
                return 20;
            });
            Assert.That(chamadas, Is.EqualTo(1));
            Assert.That(retorno.NossoNumero, Is.EqualTo("0000020"));
            Assert.That(retorno.NossoNumeroDV, Is.EqualTo("4"));
            Assert.That(retorno.NossoNumeroFormatado, Is.EqualTo("0000020-4"));
            Assert.That(retorno.CodigoDeBarras, Is.EqualTo(Titulo().pfin_codbarras));
            Assert.That(new string(retorno.LinhaDigitavel.Where(char.IsDigit).ToArray()),
                Is.EqualTo(Titulo().pfin_linhadig));
        }

        [TestCase("004", "", "2026-10-12", 10)]
        [TestCase("017", "X", "2026-10-12", 10)]
        [TestCase("016", "999", "2026-10-12", 10)]
        [TestCase("004", "0815713", "inválida", 10)]
        [TestCase("004", "0815713", "2026-10-12", 0)]
        [TestCase("004", "0815713", "2026-10-12", 100000000)]
        public void ValidaEntradaAntesDeConsumirSequencia(string campo, string valor, string vencimento, decimal titulo)
        {
            var integracao = Integracao();
            integracao.itbc_cont_individual = "sequencia_teste";
            var chamadas = 0;
            Assert.Catch(() => BoletoFuncInterativo.ObterDados(integracao, Configuracao(campo, valor), vencimento, titulo, nome =>
            {
                chamadas++;
                return 20;
            }));
            Assert.That(chamadas, Is.Zero);
        }

        [TestCase(0L)]
        [TestCase(10000000L)]
        public void RejeitaSequenciaForaDoLimite(long sequencia)
        {
            var integracao = Integracao();
            integracao.itbc_cont_individual = "sequencia_teste";
            Assert.Throws<ArgumentException>(() => BoletoFuncInterativo.ObterDados(
                integracao, Configuracao(), "2026-10-12", 10m, nome => sequencia));
        }

        [Test]
        public void MantemFluxoSicrediComSufixoAnualNaSequencia()
        {
            var integracao = new IntegracaoBancaria
            {
                itbc_banco = "748", itbc_tpproc = "001", itbc_cont_individual = "sicredi_teste"
            };
            var det = new IntegracaoBancariaDet();
            det.Add("001", "Agência", "1234");
            det.Add("002", "Posto", "01");
            det.Add("003", "Beneficiário", "12345");
            det.Add("004", "CNPJ", "34645626000177");
            det.Add("005", "Nome", "Beneficiário de teste");
            var retorno = BoletoFuncInterativo.ObterDados(integracao, det,
                DateTime.Today.AddDays(30).ToString("yyyy-MM-dd"), 100m, nome =>
                {
                    Assert.That(nome, Is.EqualTo("sicredi_teste" + DateTime.Now.ToString("yyyy")));
                    return 20;
                });
            Assert.That(retorno.CodigoDeBarras, Does.StartWith("748"));
            Assert.That(retorno.CodigoDeBarras.Length, Is.EqualTo(44));
            Assert.That(retorno.LinhaDigitavel.Count(char.IsDigit), Is.EqualTo(47));
        }

        [Test]
        public void RejeitaSequenciaNaoConfiguradaAntesDeConsulta()
        {
            var chamadas = 0;
            Assert.Throws<ArgumentException>(() => BoletoFuncInterativo.ObterDados(
                Integracao(), Configuracao(), "2026-10-12", 100m, nome =>
                {
                    chamadas++;
                    return 20;
                }));
            Assert.That(chamadas, Is.Zero);
        }
    }
}
