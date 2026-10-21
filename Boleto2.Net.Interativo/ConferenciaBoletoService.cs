using System;
using System.Collections.Generic;
using System.Linq;
using Boleto2.Net.Interativo.Entidades;
using Boleto2Net;
using Boleto2Net.Extensions;

namespace Boleto2.Net.Interativo
{
    public sealed class ConferenciaBoletoService
    {
        private static readonly object Sincronizacao = new object();

        public IReadOnlyList<TituloConferencia> ConsultarTitulos(TipoConsultaBoleto tipo, string identificador)
        {
            var chave = (identificador ?? "").Trim();
            if (chave.Length == 0 || chave.Length > 30 || chave.Any(c => c < '0' || c > '9'))
                throw new ArgumentException("Informe um identificador numérico, preservando os zeros à esquerda.");
            if (!Enum.IsDefined(typeof(TipoConsultaBoleto), tipo))
                throw new ArgumentException("Tipo de consulta inválido.");
            var gerador = new BoletoInterativo(new List<string>());
            var operacoes = tipo == TipoConsultaBoleto.Transacao
                ? BoletoInterativo.ObterOperacoes(chave) : new List<string> { chave };
            if (operacoes.Count == 0)
                return new List<TituloConferencia>();
            return gerador.LerDadosPendfin(operacoes).OrderBy(p => p.pfin_parcela)
                .ThenBy(p => p.pfin_operacao).Select(p => new TituloConferencia { Dados = p }).ToList();
        }

        public ResultadoConferenciaBoleto Conferir(string operacao)
        {
            var titulo = ObterTitulo(operacao);
            try
            {
                lock (Sincronizacao)
                {
                    var boleto = Montar(titulo.Dados, false);
                    return Comparar(titulo.Dados, boleto);
                }
            }
            catch (Exception erro)
            {
                return new ResultadoConferenciaBoleto
                {
                    Mensagem = erro.Message,
                    Campos = new[]
                    {
                        ErroCampo("Nosso número e dígito", titulo.NossoNumero),
                        ErroCampo("Código de barras", titulo.CodigoDeBarras),
                        ErroCampo("Linha digitável", titulo.LinhaDigitavel)
                    }
                };
            }
        }

        public byte[] GerarPDF(string operacao)
        {
            var titulo = ObterTitulo(operacao);
            lock (Sincronizacao)
            {
                var boleto = Montar(titulo.Dados, true);
                using (var impressao = new BoletoBancario { Boleto = boleto })
                    return impressao.MontaBytesPDF(false);
            }
        }

        private TituloConferencia ObterTitulo(string operacao)
        {
            var titulos = ConsultarTitulos(TipoConsultaBoleto.Operacao, operacao);
            if (titulos.Count != 1)
                throw new ArgumentException("A operação não foi encontrada ou não identifica um único título com integração bancária.");
            return titulos[0];
        }

        private static Boleto Montar(Pendfin titulo, bool validarChaves)
        {
            var gerador = new BoletoInterativo(new List<string>());
            var empresa = gerador.ObterEmpresa(titulo.pfin_empr_codigo);
            var cliente = gerador.ObterDestinatario(titulo);
            var cadastro = BoletoInterativo.ObterIntegracaoBancaria(titulo.pger_itbc_codigo);
            var detalhes = BoletoInterativo.ObterConfigDet(titulo.pger_itbc_codigo);
            return BoletoInterativo.MontarBoleto(titulo, empresa, cliente, detalhes, cadastro, validarChaves);
        }

        internal static ResultadoConferenciaBoleto Comparar(Pendfin titulo, Boleto boleto)
        {
            var entrada = NumeroBoletoConferencia.Ler(boleto.Banco.Codigo, titulo.pfin_nossonumero);
            var numero = entrada.Numero;
            if (boleto.Banco.Codigo == 1 && numero.Length <= 10)
                numero = boleto.Banco.Cedente.Codigo + numero.PadLeft(10, '0');
            var chaveERP = numero + entrada.Digito;
            var chaveDLL = boleto.NossoNumero + (entrada.Digito.Length > 0 ? boleto.NossoNumeroDV : "");
            var campoNumero = CampoConferencia.Comparar("Nosso número e dígito", chaveERP, chaveDLL, false);
            if (entrada.Digito.Length == 0 && !string.IsNullOrEmpty(boleto.NossoNumeroDV))
                campoNumero.Resultado = "Ausente";
            campoNumero.ERP = titulo.pfin_nossonumero;
            campoNumero.Calculado = boleto.NossoNumeroFormatado;
            var campos = new[]
            {
                campoNumero,
                CampoConferencia.Comparar("Código de barras", titulo.pfin_codbarras, boleto.CodigoBarra.CodigoDeBarras, false),
                CampoConferencia.Comparar("Linha digitável", titulo.pfin_linhadig, boleto.CodigoBarra.LinhaDigitavel, true)
            };
            return new ResultadoConferenciaBoleto
            {
                Campos = campos,
                Mensagem = campos.All(c => c.Resultado == "Confere")
                    ? "Os dados calculados pela DLL conferem com o ERP."
                    : "Há divergências ou informações ausentes. Consulte os campos destacados."
            };
        }

        internal static void RecalcularSicredi(Boleto boleto)
        {
            if (boleto.Banco.Codigo != 748) return;
            var conta = boleto.Banco.Cedente.ContaBancaria;
            var numero = boleto.NossoNumero;
            boleto.NossoNumeroDV = StringExtensions.CalcularDVSicredi(conta.Agencia, conta.Posto,
                boleto.Banco.Cedente.Codigo + boleto.Banco.Cedente.CodigoDV,
                numero.Substring(0, 2), numero.Substring(3, 5), numero.Substring(2, 1));
            boleto.NossoNumeroFormatado = numero.Substring(0, 2) + "/" + numero.Substring(2) + "-" + boleto.NossoNumeroDV;
            Banco.FormataCodigoBarra(boleto);
            Banco.FormataLinhaDigitavel(boleto);
        }

        private static CampoConferencia ErroCampo(string campo, string erp)
            => new CampoConferencia { Campo = campo, ERP = erp, Calculado = "", Resultado = "Erro no cálculo" };
    }
}
