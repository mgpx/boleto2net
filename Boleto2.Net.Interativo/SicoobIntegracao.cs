using System;
using System.Globalization;
using System.Linq;
using Boleto2.Net.Interativo.Entidades;
using Boleto2Net;

namespace Boleto2.Net.Interativo
{
    internal static class SicoobIntegracao
    {
        internal static IBanco GerarCedente(IntegracaoBancaria integracao, IntegracaoBancariaDet det)
        {
            if (integracao == null)
                throw new ArgumentNullException(nameof(integracao));
            if (det == null)
                throw new ArgumentNullException(nameof(det));

            var documento = ParametroNumerico(det, "001", 14);
            if (documento.Length != 11 && documento.Length != 14)
                throw new ArgumentException("Sicoob: parâmetro 001 deve conter CPF ou CNPJ sem pontuação.");
            var agencia = ParametroNumerico(det, "002", 4);
            var digitoAgencia = ParametroNumerico(det, "003", 1);
            var convenio = ParametroNumerico(det, "004", 7);
            if (convenio.Length != 7)
                throw new ArgumentException("Sicoob: parâmetro 004 deve conter os seis dígitos do beneficiário e seu dígito verificador.");
            var nome = ParametroObrigatorio(det, "005");
            var carteira = ParametroObrigatorio(det, "009");
            var modalidade = ParametroObrigatorio(det, "010");
            if (carteira != "1" || modalidade != "01")
                throw new ArgumentException("Sicoob: carteira/modalidade não implementada. Utilize a carteira 1/01 (parâmetros 009 e 010).");
            var impressao = (TipoImpressaoBoleto)ParametroEnum(det, "012", typeof(TipoImpressaoBoleto));
            var distribuicao = (TipoDistribuicaoBoleto)ParametroEnum(det, "013", typeof(TipoDistribuicaoBoleto));
            var conta = ParametroNumerico(det, "025", 8);
            var digitoConta = ParametroNumerico(det, "026", 1);
            ValidarConfiguracaoTitulo(det);

            var banco = Banco.Instancia(Bancos.Sicoob);
            banco.Cedente = new Cedente
            {
                CPFCNPJ = documento,
                Nome = nome,
                Codigo = convenio.Substring(0, 6),
                CodigoDV = convenio.Substring(6, 1),
                ContaBancaria = new ContaBancaria
                {
                    Agencia = agencia,
                    DigitoAgencia = digitoAgencia,
                    Conta = conta,
                    DigitoConta = digitoConta,
                    CarteiraPadrao = carteira,
                    VariacaoCarteiraPadrao = modalidade,
                    TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
                    // O parâmetro 011 do ERP usa códigos diferentes do enum da biblioteca.
                    TipoFormaCadastramento = integracao.itbc_tpproc == "001"
                        ? TipoFormaCadastramento.SemRegistro : TipoFormaCadastramento.ComRegistro,
                    TipoImpressaoBoleto = impressao,
                    TipoDistribuicao = distribuicao,
                    TipoDocumento = TipoDocumento.Tradicional
                }
            };
            banco.FormataCedente();
            if (!string.IsNullOrWhiteSpace(det["024"]))
                banco.Cedente.ContaBancaria.LocalPagamento = det["024"].Trim();
            return banco;
        }

        internal static void AplicarConfiguracaoTitulo(Boleto boleto, IntegracaoBancariaDet det)
        {
            ValidarConfiguracaoTitulo(det);
            boleto.EspecieDocumento = (TipoEspecieDocumento)ParametroEnum(det, "016", typeof(TipoEspecieDocumento));
            boleto.Aceite = ParametroObrigatorio(det, "017");
            boleto.MensagemInstrucoesCaixa = string.Join("<br/>", Enumerable.Range(101, 6)
                .Select(codigo => det[codigo.ToString(CultureInfo.InvariantCulture)])
                .Where(texto => !string.IsNullOrWhiteSpace(texto)));
        }

        internal static void PrepararTituloExistente(Boleto boleto, Pendfin pendfin, IntegracaoBancariaDet det, bool validarChaves = true)
        {
            var numero = (pendfin.pfin_nossonumero ?? string.Empty).Trim();
            if (numero.Length != 8 || !SomenteDigitos(numero))
                throw new ArgumentException("Sicoob: nosso número do ERP deve conter sete dígitos e o dígito verificador (oito posições).");
            if (pendfin.pfin_parcela < 1 || pendfin.pfin_parcela > 999)
                throw new ArgumentException("Sicoob: parcela do título deve estar entre 1 e 999.");

            boleto.NossoNumero = numero.Substring(0, 7);
            boleto.Parcela = (int)pendfin.pfin_parcela;
            AplicarConfiguracaoTitulo(boleto, det);
            boleto.ValidarDadosChaves(pendfin.pfin_codbarras, pendfin.pfin_linhadig);

            if (!validarChaves) return;

            if (boleto.NossoNumeroDV != numero.Substring(7, 1))
                throw new ArgumentException("Sicoob: dígito do nosso número gravado no ERP diverge do calculado.");
            if (string.IsNullOrWhiteSpace(pendfin.pfin_codbarras)
                || pendfin.pfin_codbarras.Trim() != boleto.CodigoBarra.CodigoDeBarras)
                throw new ArgumentException("Sicoob: código de barras gravado no ERP está ausente ou diverge do calculado.");

            var linha = (pendfin.pfin_linhadig ?? string.Empty).Trim();
            if (linha.Any(c => (c < '0' || c > '9') && c != '.' && !char.IsWhiteSpace(c))
                || ApenasDigitos(linha) != ApenasDigitos(boleto.CodigoBarra.LinhaDigitavel))
                throw new ArgumentException("Sicoob: linha digitável gravada no ERP está ausente ou diverge da calculada.");
        }

        internal static void ValidarSequencia(long numero)
        {
            if (numero < 1 || numero > 9999999)
                throw new ArgumentException("Sicoob: sequência do nosso número deve estar entre 1 e 9999999.");
        }

        private static void ValidarConfiguracaoTitulo(IntegracaoBancariaDet det)
        {
            ParametroEnum(det, "016", typeof(TipoEspecieDocumento));
            var aceite = ParametroObrigatorio(det, "017");
            if (aceite != "A" && aceite != "N")
                throw new ArgumentException("Sicoob: parâmetro 017 (aceite) deve ser A ou N.");
        }

        private static int ParametroEnum(IntegracaoBancariaDet det, string codigo, Type tipo)
        {
            int valor;
            if (!int.TryParse(ParametroObrigatorio(det, codigo), NumberStyles.None, CultureInfo.InvariantCulture, out valor)
                || !Enum.IsDefined(tipo, valor))
                throw new ArgumentException($"Sicoob: parâmetro {codigo} possui código inválido.");
            return valor;
        }

        private static string ParametroNumerico(IntegracaoBancariaDet det, string codigo, int tamanhoMaximo)
        {
            var valor = ParametroObrigatorio(det, codigo);
            if (valor.Length > tamanhoMaximo || !SomenteDigitos(valor))
                throw new ArgumentException($"Sicoob: parâmetro {codigo} deve conter somente números, com até {tamanhoMaximo} dígitos.");
            return valor;
        }

        private static string ParametroObrigatorio(IntegracaoBancariaDet det, string codigo)
        {
            var valor = det[codigo];
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException($"Sicoob: parâmetro obrigatório {codigo} não informado na integração bancária.");
            return valor.Trim();
        }

        private static bool SomenteDigitos(string valor) => valor.All(c => c >= '0' && c <= '9');
        private static string ApenasDigitos(string valor) => new string(valor.Where(c => c >= '0' && c <= '9').ToArray());
    }
}
