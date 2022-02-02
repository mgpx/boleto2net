using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boleto2.Net.Interativo
{
    public partial class frmPDFViewer : Form
    {
        //https://github.com/pvginkel/PdfiumViewer/blob/2593329b382c599724ff49d41020aa71b6f6a16b/PdfiumViewer.Demo/MainForm.cs
        //https://github.com/pvginkel/PdfiumBuild

        public String mFileName = String.Empty;
        public Stream mStream = Stream.Null;
        
        private frmPDFViewer()
        {

            //MessageBox.Show("frmPDFViewer");
            InitializeComponent();
        }

        public frmPDFViewer(String fileName)
            :this()
        {
            if (File.Exists(fileName) == false)
                throw new FileNotFoundException("Não foi possível carregar o arquivo " + fileName);

            mFileName = fileName;
        }

        public frmPDFViewer(Stream stream)
            :this()
        {
            this.mStream = stream;
        }

        protected override void OnShown(EventArgs e)
        {
            //MessageBox.Show("onShown");
            base.OnShown(e);
            PdfiumViewer.PdfDocument pdf = null;
            if (String.IsNullOrEmpty(mFileName) == false)
            {
                pdf = PdfiumViewer.PdfDocument.Load(this, this.mFileName);
            }
            else if (this.mStream != Stream.Null)
            {
                pdf = PdfiumViewer.PdfDocument.Load(this, this.mStream);
            }
            pdfViewer1.Document = pdf;
            //MessageBox.Show("documento setado");
            pdfViewer1.Renderer.Zoom = 1.5;
            //MessageBox.Show("zoom setado");
            pdfViewer1.DefaultDocumentName = DateTime.Now.ToString("dd-MM-yyyy HHmmss" + ".pdf");
            //MessageBox.Show("nome documento setado");

            this.Text = "Pré Visualização [Boletos]";
        }
    }
}
