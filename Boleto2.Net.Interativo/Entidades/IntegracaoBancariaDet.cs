using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boleto2.Net.Interativo.Entidades
{
    public class IntegracaoBancariaDet
    {
        public class ItemIntegracaoBancariaDet
        {
            public String Codigo { get; set; }
            public String Instrucao { get; set; }
            public String Valor { get; set; }
        }

        private List<ItemIntegracaoBancariaDet> mDet = new List<ItemIntegracaoBancariaDet>();

        public List<ItemIntegracaoBancariaDet> Det { get => mDet; }

        public String this[String codigo]
        {
            get
            {
                return mDet.Where(t => t.Codigo == codigo).FirstOrDefault()?.Valor;
            }
        }

        public void Add(String codigo, String instrucao, String valor)
        {
            mDet.Add(new ItemIntegracaoBancariaDet() { Codigo = codigo, Instrucao = instrucao, Valor = valor });
        } 
    }
}
