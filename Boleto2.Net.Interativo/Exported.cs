using RGiesecke.DllExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boleto2.Net.Interativo
{
    [Guid("57F0E489-9810-414B-988B-197860EED2FD")]
    [ComVisible(true)]
    public class Exported
    {
        public Exported()
        {

        }


        [DllExport("GerarPDF", CallingConvention.StdCall)]
        public static void GerarPDF(String operacoes, Boolean enviarEmail, String diretorio, [MarshalAs(UnmanagedType.BStr)] ref String retorno)
        {
            retorno = String.Empty;
            //System.Windows.Forms.MessageBox.Show($"Operações:{operacoes}\nEmail:{enviarEmail}\nDir:{diretorio}" );
            try
            {
                if (operacoes == null || operacoes.Length == 0)
                {
                    retorno = "Nenhuma operação informada";
                    return;
                }


                BoletoInterativo boletoInterativo = new BoletoInterativo(operacoes.Split(new char[] { ';' }).ToList());
                boletoInterativo.SetDirBase(diretorio);
                boletoInterativo.Gerar();

                //System.Windows.Forms.MessageBox.Show($"Gerado");

                if (enviarEmail)
                    boletoInterativo.EnviarEmail();
            }
            catch(Exception e)
            {
                retorno = e.ToString();
                System.Windows.Forms.MessageBox.Show(retorno);
            }
        }


        [DllExport("GerarPDFTransacao", CallingConvention.StdCall)]
        public static void GerarPDFTransacao(String transacao, Boolean mostrarTela, String diretorio, Boolean enviarEmail, [MarshalAs(UnmanagedType.BStr)] ref String retorno)
        {
            BoletoInterativo.RegistrarLog($"GerarPDFTransacao: {transacao}");
            if (String.IsNullOrEmpty(transacao))
            {
                return;
            }

            try
            {
                List<String> operacoes = new List<string>();
                if (transacao.Length == 10)
                    operacoes = BoletoInterativo.ObterOperacoes(transacao);
                else if (transacao.Length == 11)
                    operacoes.Add(transacao);

                InternalGerarPDFOperacao(operacoes, mostrarTela, diretorio, enviarEmail, ref retorno);
            }catch(Exception e)
            {
                BoletoInterativo.RegistrarLog($"Erro ao gerar PDF: {e.Message.ToString()}");
                MessageBox.Show(e.ToString(), "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        [DllExport("GerarPDFOperacao", CallingConvention.StdCall)]
        public static void GerarPDFOperacao(String operacoes, Boolean mostrarTela, String diretorio, Boolean enviarEmail, [MarshalAs(UnmanagedType.BStr)] ref String retorno)
        {
            try
            {
                BoletoInterativo.RegistrarLog($"GerarPDFOperacao: {operacoes}");

                Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
                //Application.EnableVisualStyles();
                List<String> listOperacoes = operacoes.Split(new char[] { ';' }).ToList();
                
                InternalGerarPDFOperacao(listOperacoes, mostrarTela, diretorio, enviarEmail, ref retorno);
            }
            catch (Exception e)
            {
                BoletoInterativo.RegistrarLog($"Erro ao gerar PDF: {e.Message.ToString()}");
                MessageBox.Show(e.ToString(), "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void InternalGerarPDFOperacao(List<String> operacoes, Boolean mostrarTela, String diretorio, Boolean enviarEmail, [MarshalAs(UnmanagedType.BStr)] ref String retorno)
        {
            retorno = String.Empty;
            try
            {
                if (operacoes == null || operacoes.Count == 0)
                    return;

                BoletoInterativo boletoInterativo = new BoletoInterativo(operacoes);
                boletoInterativo.SetDirBase(diretorio);
                boletoInterativo.Gerar();

                if (mostrarTela)
                    boletoInterativo.MostarBoleto();

                if (enviarEmail)
                {
                    boletoInterativo.EnviarEmail();
                }
                    
                
                //if (enviarEmail)
                //    boletoInterativo.EnviarEmail();
            }
            catch (Exception e)
            {
                retorno = e.ToString();
            }
        }

        [DllExport("GerarInfoBoleto", CallingConvention.StdCall)]
        public static void GerarInfoBoleto(String codIntegracaoBancaria, String vcto, String valor, [MarshalAs(UnmanagedType.BStr)]out String retorno)
        {
            //StringBuilder sb = new StringBuilder();
            //sb.Append("CodIntegracaoBancaria:" + codIntegracaoBancaria + "\n");
            //sb.Append("Vcto:" + vcto + "\n");
            //sb.Append("Valor:" + valor + "\n");

            //System.Windows.Forms.MessageBox.Show(sb.ToString());
            Decimal valorDecimal = Convert.ToDecimal(Convert.ToInt32(valor)) / 100.0m;
            //System.Windows.Forms.MessageBox.Show("Converteu Valor");
            retorno = String.Empty;
            try
            {
                BoletoFuncInterativo.RetornoBoleto retornoBoleto = BoletoFuncInterativo.ObterDados(codIntegracaoBancaria, vcto, valorDecimal);
                //System.Windows.Forms.MessageBox.Show("Info Boleto Concluido");
                String aux = $"{retornoBoleto.CodigoDeBarras};{retornoBoleto.LinhaDigitavel};{retornoBoleto.NossoNumero};{retornoBoleto.NossoNumeroDV};{retornoBoleto.NossoNumeroFormatado}";
                //System.Windows.Forms.MessageBox.Show(aux);
                //System.Windows.Forms.MessageBox.Show("Passando retorno a referencia");
                retorno = aux;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            
        }

        [DllExport("Teste", CallingConvention.StdCall)]
        public static void Teste()
        {
            System.Windows.Forms.MessageBox.Show("OK");
        }

        [DllExport("Teste2", CallingConvention.StdCall)]
        public static void Teste2(String texto)
        {
            System.Windows.Forms.MessageBox.Show("OK " + texto);
        }



        [DllExport("GerarPDF2", CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static void GerarPDF2(
            [MarshalAs(UnmanagedType.BStr)]
            String operacoes,
            [MarshalAs(UnmanagedType.Bool)]
            Boolean enviarEmail,
            [MarshalAs(UnmanagedType.BStr)]
            String diretorio)
        {
            //Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //return "";
        }

        //[DllExport("Teste", CallingConvention.StdCall)]
        public static void Teste5()
        {
        }
    }
}
