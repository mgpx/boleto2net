using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boleto2.Net.Interativo.Entidades
{
    public class Cliente
    {
        public String clie_cnpj           { get; set; }
        public String clie_razaosocial    { get; set; }
        public String clie_endereco       { get; set; }
        public String clie_numero         { get; set; }
        public String clie_complemento    { get; set; }
        public String clie_bairro         { get; set; }
        public String clie_cidade         { get; set; }
        public String clie_uf             { get; set; }
        public String clie_cep            { get; set; }
        public String clie_email          { get; set; }
        public String clie_cobremail      { get; set; }
    }
}
