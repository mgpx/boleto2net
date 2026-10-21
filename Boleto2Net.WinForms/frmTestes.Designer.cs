using System.Drawing;
using System.Windows.Forms;

namespace Boleto2.Net.WinForms
{
    partial class frmTestes
    {
        private System.ComponentModel.IContainer components;
        private ComboBox tipoConsulta;
        private TextBox identificador;
        private Button buscar, conferir, imprimir, antigos;
        private Label ambiente;
        private TextBox mensagem;
        private DataGridView titulos, comparacao;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Text = "Conferência de boletos — ERP × DLL";
            Name = "frmTestes";
            Font = new Font("Segoe UI", 10F);
            ClientSize = new Size(1180, 760);
            MinimumSize = new Size(940, 680);
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Font;
            var estrutura = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 7 };
            estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
            ambiente = new Label { AutoSize = true, ForeColor = Color.DimGray, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            estrutura.Controls.Add(ambiente, 0, 0);
            var busca = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            tipoConsulta = new ComboBox { Name = "tipoConsulta", DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
            identificador = new TextBox { Name = "identificador", Width = 230, MaxLength = 30 };
            buscar = new Button { Name = "buscar", Text = "Buscar", Width = 100, Height = 32 };
            busca.Controls.Add(new Label { Text = "Consultar por", AutoSize = true, Padding = new Padding(0, 6, 8, 0) });
            busca.Controls.Add(tipoConsulta);
            busca.Controls.Add(identificador);
            busca.Controls.Add(buscar);
            estrutura.Controls.Add(busca, 0, 1);
            titulos = CriarGrade();
            titulos.Name = "titulos";
            titulos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Coluna(titulos, "Operação", "Operacao", 130);
            Coluna(titulos, "Transação", "Transacao", 130);
            Coluna(titulos, "Banco", "Banco", 65);
            Coluna(titulos, "Documento", "Documento", 100);
            Coluna(titulos, "Parcela", "Parcela", 65);
            Coluna(titulos, "Vencimento", "Vencimento", 115);
            titulos.Columns[5].DefaultCellStyle.Format = "dd/MM/yyyy";
            Coluna(titulos, "Valor", "Valor", 110);
            titulos.Columns[6].DefaultCellStyle.Format = "N2";
            Coluna(titulos, "Integração bancária", "Integracao", 220);
            estrutura.Controls.Add(titulos, 0, 2);
            estrutura.Controls.Add(new Label { Text = "Dados do título selecionado — selecione as células para copiar os valores", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
            comparacao = CriarGrade();
            comparacao.Name = "comparacao";
            comparacao.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            comparacao.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            comparacao.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            comparacao.RowTemplate.MinimumHeight = 65;
            comparacao.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Coluna(comparacao, "Informação", "", 150);
            Coluna(comparacao, "Gravado no ERP", "", 350);
            Coluna(comparacao, "Calculado pela DLL", "", 350);
            Coluna(comparacao, "Resultado", "", 130);
            comparacao.Columns[0].FillWeight = 17;
            comparacao.Columns[1].FillWeight = comparacao.Columns[2].FillWeight = 34;
            comparacao.Columns[3].FillWeight = 15;
            estrutura.Controls.Add(comparacao, 0, 4);
            var acoes = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
            conferir = new Button { Name = "conferir", Text = "Conferir pela DLL", Width = 165, Height = 32 };
            imprimir = new Button { Name = "imprimir", Text = "Imprimir / Prévia", Width = 165, Height = 32 };
            antigos = new Button { Name = "antigos", Text = "Testes antigos", Width = 145, Height = 32 };
            acoes.Controls.Add(conferir);
            acoes.Controls.Add(imprimir);
            acoes.Controls.Add(antigos);
            estrutura.Controls.Add(acoes, 0, 5);
            mensagem = new TextBox { Name = "mensagem", ReadOnly = true, Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, Text = "Informe uma transação ou operação para consultar os títulos do ERP." };
            estrutura.Controls.Add(mensagem, 0, 6);
            Controls.Add(estrutura);
            AcceptButton = buscar;
        }

        private static DataGridView CriarGrade()
            => new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                MultiSelect = false, RowHeadersVisible = false, BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            };

        private static void Coluna(DataGridView grade, string titulo, string propriedade, int largura)
            => grade.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = titulo, DataPropertyName = propriedade, Width = largura,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
    }
}
