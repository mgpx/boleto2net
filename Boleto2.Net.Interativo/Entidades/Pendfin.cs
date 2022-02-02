using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boleto2.Net.Interativo.Entidades
{
    public class Pendfin
    {
        public String pfin_operacao        { get; set; }
        public String pfin_transacao       { get; set; }
        public String pfin_status          { get; set; }
        public String pfin_pagrec          { get; set; }
        public DateTime? pfin_datamvto        { get; set; }
        public DateTime? pfin_dataref         { get; set; }
        public DateTime? pfin_datavcto        { get; set; }
        public DateTime? pfin_datalanc        { get; set; }
        public Decimal pfin_valor           { get; set; }
        public String pfin_empr_codigo     { get; set; }
        public String pfin_tpagente        { get; set; }
        public Int64 pfin_codagente       { get; set; }
        public Int64 pfin_documento       { get; set; }
        public Int64 pfin_parcela         { get; set; }
        public String pfin_contager        { get; set; }
        public Decimal pfin_juros           { get; set; }
        public Decimal pfin_desconto        { get; set; }
        public Decimal pfin_multa           { get; set; }
        public String pfin_cpfcnpj         { get; set; }
        public String pfin_codbarras       { get; set; }
        public String pfin_linhadig        { get; set; }
        public String pfin_nossonumero     { get; set; }
        public String pfin_observacao      { get; set; }
        public String pfin_protesto        { get; set; }
        public String pger_impr_codigo     { get; set; }
        public String pger_itbc_codigo     { get; set; }
        public String itbc_descricao       { get; set; }
        public String itbc_banco           { get; set; }
        public String itbc_tpproc          { get; set; }
        public String itbc_cont_geral      { get; set; }
        public String itbc_cont_individual { get; set; }
        public String itbc_protesto        { get; set; }

    }
}
