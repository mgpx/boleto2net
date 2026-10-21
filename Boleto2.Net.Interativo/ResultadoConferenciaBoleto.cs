using System;
using System.Collections.Generic;
using System.Linq;
using Boleto2.Net.Interativo.Entidades;

namespace Boleto2.Net.Interativo
{
    public enum TipoConsultaBoleto { Transacao, Operacao }

    public sealed class TituloConferencia
    {
        internal Pendfin Dados { get; set; }
        public string Operacao => Dados.pfin_operacao;
        public string Transacao => Dados.pfin_transacao;
        public string Banco => Dados.itbc_banco;
        public string Integracao => Dados.itbc_descricao;
        public long Documento => Dados.pfin_documento;
        public long Parcela => Dados.pfin_parcela;
        public DateTime? Vencimento => Dados.pfin_datavcto;
        public decimal Valor => Dados.pfin_valor;
        public string NossoNumero => Dados.pfin_nossonumero;
        public string CodigoDeBarras => Dados.pfin_codbarras;
        public string LinhaDigitavel => Dados.pfin_linhadig;
    }

    public sealed class CampoConferencia
    {
        public string Campo { get; internal set; }
        public string ERP { get; internal set; }
        public string Calculado { get; internal set; }
        public string Resultado { get; internal set; }

        internal static CampoConferencia Comparar(string campo, string erp, string calculado, bool formatado)
        {
            Func<string, string> normalizar = texto => formatado
                ? new string((texto ?? "").Where(c => c != '.' && !char.IsWhiteSpace(c)).ToArray())
                : (texto ?? "").Trim();
            return new CampoConferencia
            {
                Campo = campo, ERP = erp, Calculado = calculado,
                Resultado = string.IsNullOrWhiteSpace(erp) ? "Ausente"
                    : normalizar(erp) == normalizar(calculado) ? "Confere" : "Divergente"
            };
        }
    }

    public sealed class ResultadoConferenciaBoleto
    {
        public IReadOnlyList<CampoConferencia> Campos { get; internal set; }
        public string Mensagem { get; internal set; }
        public bool Confere => Campos != null && Campos.All(c => c.Resultado == "Confere");
    }
}
