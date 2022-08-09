namespace Boleto2.Net.WinForms
{
    partial class Form1
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSicredi = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnNbcBank = new System.Windows.Forms.Button();
            this.btnGerarSicoob = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.btnTeste = new System.Windows.Forms.Button();
            this.cbEnviarEmail = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnSicredi
            // 
            this.btnSicredi.Location = new System.Drawing.Point(493, 46);
            this.btnSicredi.Name = "btnSicredi";
            this.btnSicredi.Size = new System.Drawing.Size(118, 48);
            this.btnSicredi.TabIndex = 0;
            this.btnSicredi.Text = "Gerar Boleto Sicredi";
            this.btnSicredi.UseVisualStyleBackColor = true;
            this.btnSicredi.Click += new System.EventHandler(this.btnSicredi_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(672, 46);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnNbcBank
            // 
            this.btnNbcBank.Location = new System.Drawing.Point(493, 100);
            this.btnNbcBank.Name = "btnNbcBank";
            this.btnNbcBank.Size = new System.Drawing.Size(118, 48);
            this.btnNbcBank.TabIndex = 0;
            this.btnNbcBank.Text = "Gerar NBC Bank";
            this.btnNbcBank.UseVisualStyleBackColor = true;
            this.btnNbcBank.Click += new System.EventHandler(this.btnNbcBank_Click);
            // 
            // btnGerarSicoob
            // 
            this.btnGerarSicoob.Location = new System.Drawing.Point(493, 154);
            this.btnGerarSicoob.Name = "btnGerarSicoob";
            this.btnGerarSicoob.Size = new System.Drawing.Size(118, 48);
            this.btnGerarSicoob.TabIndex = 2;
            this.btnGerarSicoob.TabStop = false;
            this.btnGerarSicoob.Text = "Gerar Sicoob";
            this.btnGerarSicoob.UseVisualStyleBackColor = true;
            this.btnGerarSicoob.Click += new System.EventHandler(this.btnGerarSicoob_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(99, 28);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(146, 20);
            this.textBox1.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Operacao";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(99, 59);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(146, 35);
            this.button2.TabIndex = 5;
            this.button2.Text = "Exibir Boleto";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnTeste
            // 
            this.btnTeste.Location = new System.Drawing.Point(493, 265);
            this.btnTeste.Name = "btnTeste";
            this.btnTeste.Size = new System.Drawing.Size(139, 38);
            this.btnTeste.TabIndex = 6;
            this.btnTeste.Text = "Teste";
            this.btnTeste.UseVisualStyleBackColor = true;
            this.btnTeste.Click += new System.EventHandler(this.btnTeste_Click);
            // 
            // cbEnviarEmail
            // 
            this.cbEnviarEmail.AutoSize = true;
            this.cbEnviarEmail.Location = new System.Drawing.Point(266, 69);
            this.cbEnviarEmail.Name = "cbEnviarEmail";
            this.cbEnviarEmail.Size = new System.Drawing.Size(84, 17);
            this.cbEnviarEmail.TabIndex = 7;
            this.cbEnviarEmail.Text = "Enviar Email";
            this.cbEnviarEmail.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(395, 145);
            this.Controls.Add(this.cbEnviarEmail);
            this.Controls.Add(this.btnTeste);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnGerarSicoob);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnNbcBank);
            this.Controls.Add(this.btnSicredi);
            this.Name = "Form1";
            this.Text = "Teste Boleto";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSicredi;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnNbcBank;
        private System.Windows.Forms.Button btnGerarSicoob;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnTeste;
        private System.Windows.Forms.CheckBox cbEnviarEmail;
    }
}

