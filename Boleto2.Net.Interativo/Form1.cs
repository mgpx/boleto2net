using Boleto2Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boleto2.Net.Interativo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            //if (String.IsNullOrEmpty(this.txtOperacao.Text))
            //{
            //    MessageBox.Show("Operacao inválida", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}
            //var list = new List<String>() { "00100006407" };
            //var list = new List<String>() { "00100336885" };
            //var list = new List<String>() { "00100108741", "00100059196" };
            String aux = String.Empty;

            Exported.GerarPDFOperacao("00100509901", true, @"C\Temp\", true, ref aux);

            //BoletoFuncInterativo.RetornoBoleto retorno = BoletoFuncInterativo.ObterDados("002", "2019-03-19", 1.00m);

            //BoletoInterativo boletoInterativo = new BoletoInterativo(list);
            //boletoInterativo.Gerar();
            ////boletoInterativo.EnviarEmail();
            //boletoInterativo.MostarBoleto();

            //Boletos boletos = new Boletos();
            //boletos.Banco = Banco.Instancia(Bancos.Itau);
            //var boleto = new Boleto(boletos.Banco);
            //boleto.Banco.FormataCodigoBarraCampoLivre(boleto);

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
