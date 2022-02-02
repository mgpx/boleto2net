using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boleto2.Net.Interativo.Entidades
{
    public class IntegracaoBancaria
    {
        public String itbc_codigo { get; set; }
        public String itbc_descricao { get; set; }
        public String itbc_banco { get; set; }
        public String itbc_tpproc { get; set; }
        public String itbc_pger_conta { get; set; }
        public String itbc_database { get; set; }
        public String itbc_tppend { get; set; }
        public String itbc_usua_codigo { get; set; }
        public String itbc_arqdest { get; set; }
        public String itbc_cont_geral { get; set; }
        public String itbc_cont_individual { get; set; }
        public String itbc_protesto { get; set; }
    }
}
