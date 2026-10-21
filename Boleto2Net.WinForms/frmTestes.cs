using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Boleto2.Net.Interativo;

namespace Boleto2.Net.WinForms
{
    public partial class frmTestes : Form
    {
        private readonly ConferenciaBoletoService servico = new ConferenciaBoletoService();
        private bool ocupado;

        public frmTestes()
        {
            InitializeComponent();
            tipoConsulta.Items.AddRange(new object[] { "Transação", "Operação" });
            tipoConsulta.SelectedIndex = 0;
            var conexao = Boleto2.Net.Interativo.Utils.GetIpServer();
            ambiente.Text = $"Servidor: {conexao.Ip}:{conexao.PortaDB}     Base: {conexao.DatabaseName}";
            buscar.Click += async (s, e) => await Buscar();
            conferir.Click += async (s, e) => await Conferir();
            imprimir.Click += async (s, e) => await Imprimir();
            antigos.Click += (s, e) => { using (var form = new Form1()) form.ShowDialog(this); };
            titulos.SelectionChanged += (s, e) => MostrarSelecionado();
            identificador.KeyDown += async (s, e) =>
            {
                if (e.KeyCode != Keys.Enter || ocupado) return;
                e.SuppressKeyPress = true;
                await Buscar();
            };
            FormClosing += (s, e) =>
            {
                if (!ocupado) return;
                e.Cancel = true;
                mensagem.Text = "Aguarde a conclusão da operação antes de fechar a tela.";
            };
            AtualizarBotoes();
        }

        private TituloConferencia Selecionado => titulos.CurrentRow?.DataBoundItem as TituloConferencia;

        private async Task Buscar()
        {
            var tipo = tipoConsulta.SelectedIndex == 0 ? TipoConsultaBoleto.Transacao : TipoConsultaBoleto.Operacao;
            var chave = identificador.Text;
            await Executar(async () =>
            {
                titulos.DataSource = null;
                comparacao.Rows.Clear();
                var encontrados = await Task.Run(() => servico.ConsultarTitulos(tipo, chave));
                titulos.DataSource = encontrados;
                MostrarSelecionado();
                mensagem.Text = encontrados.Count == 0 ? "Nenhum título com integração bancária foi encontrado."
                    : $"{encontrados.Count} título(s) encontrado(s). Selecione um título para conferir ou imprimir.";
            }, "Consultando títulos no ERP...");
        }

        private async Task Conferir()
        {
            var titulo = Selecionado;
            if (titulo == null) return;
            await Executar(async () =>
            {
                MostrarSelecionado();
                var resultado = await Task.Run(() => servico.Conferir(titulo.Operacao));
                comparacao.Rows.Clear();
                foreach (var campo in resultado.Campos)
                {
                    var indice = comparacao.Rows.Add(campo.Campo, campo.ERP, campo.Calculado, campo.Resultado);
                    var linha = comparacao.Rows[indice];
                    linha.DefaultCellStyle.BackColor = campo.Resultado == "Confere" ? Color.Honeydew
                        : campo.Resultado == "Divergente" ? Color.MistyRose : Color.LemonChiffon;
                }
                mensagem.Text = resultado.Mensagem;
            }, "Recalculando o título pela DLL...");
        }

        private async Task Imprimir()
        {
            var titulo = Selecionado;
            if (titulo == null) return;
            await Executar(async () =>
            {
                var pdf = await Task.Run(() => servico.GerarPDF(titulo.Operacao));
                mensagem.Text = "Prévia aberta. Utilize o botão de impressão do visualizador para escolher a impressora.";
                using (var memoria = new MemoryStream(pdf))
                using (var visualizador = new frmPDFViewer(memoria))
                    visualizador.ShowDialog(this);
                mensagem.Text = "Prévia encerrada. A impressão é comandada pelo visualizador.";
            }, "Gerando o PDF pela DLL...");
        }

        private async Task Executar(Func<Task> acao, string andamento)
        {
            if (ocupado) return;
            ocupado = true;
            AtualizarBotoes();
            mensagem.Text = andamento;
            try { await acao(); }
            catch (Exception erro) { mensagem.Text = "Não foi possível concluir a operação: " + erro.Message; }
            finally
            {
                ocupado = false;
                AtualizarBotoes();
            }
        }

        private void MostrarSelecionado()
        {
            comparacao.Rows.Clear();
            var titulo = Selecionado;
            if (titulo != null)
            {
                comparacao.Rows.Add("Nosso número e dígito", titulo.NossoNumero, "", "Não conferido");
                comparacao.Rows.Add("Código de barras", titulo.CodigoDeBarras, "", "Não conferido");
                comparacao.Rows.Add("Linha digitável", titulo.LinhaDigitavel, "", "Não conferido");
                mensagem.Text = $"Operação {titulo.Operacao} — {titulo.Integracao}. Dados gravados no ERP; cálculo ainda não conferido.";
            }
            AtualizarBotoes();
        }

        private void AtualizarBotoes()
        {
            buscar.Enabled = tipoConsulta.Enabled = identificador.Enabled = antigos.Enabled = titulos.Enabled = !ocupado;
            conferir.Enabled = imprimir.Enabled = !ocupado && Selecionado != null;
            UseWaitCursor = ocupado;
        }
    }
}
