using Boleto2Net.Exceptions;
using Boleto2Net.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

[assembly: WebResource("BoletoNet.Imagens.748.jpg", "image/jpg")]

namespace Boleto2Net
{
    public class BancoSicredi : IBanco
    {
        internal static Lazy<IBanco> Instance { get; } = new Lazy<IBanco>(() => new BancoSicredi());

        public Cedente Cedente { get; set; }

        public int Codigo { get; } = 748;

        public string Nome { get; } = "Sicredi";

        public string Digito { get; } = "0";

        public List<string> IdsRetornoCnab400RegistroDetalhe { get; } = new List<string> { "1" };

        public bool RemoveAcentosArquivoRemessa { get; } = true;

        public void FormataCedente()
        {
            var contaBancaria = Cedente.ContaBancaria;

            //if (!CarteiraFactory<BancoSicredi>.CarteiraEstaImplementada(contaBancaria.CarteiraComVariacaoPadrao))
            //    throw Boleto2NetException.CarteiraNaoImplementada(contaBancaria.CarteiraComVariacaoPadrao);

            var codigoCedente = Cedente.Codigo;
            //if (String.IsNullOrEmpty(Cedente.CodigoDV))
            //    throw new Exception($"Dígito do código do cedente ({codigoCedente}) não foi informado.");
            if (String.IsNullOrEmpty(Cedente.ContaBancaria.Posto))
                throw new Exception($"Propriedade Posto não foi informado.");

            if (Cedente.ContaBancaria.Posto.Length != 2)
                throw new Exception($"Propriedade Posto {Cedente.ContaBancaria.Posto} deve conter 2 dígitos");

            contaBancaria.FormatarDados("PAGÁVEL EM QUALQUER BANCO ATÉ A DATA DE VENCIMENTO.", "", "", 8);

            Cedente.Codigo = codigoCedente.Length <= 4 ? codigoCedente.PadLeft(4, '0') : throw Boleto2NetException.CodigoCedenteInvalido(codigoCedente, 4);

            Cedente.CodigoFormatado = $"{Cedente.ContaBancaria.Agencia}.{Cedente.ContaBancaria.Posto}.{codigoCedente}{Cedente.CodigoDV}";
        }


        public string FormataCodigoBarraCampoLivre(Boleto boleto)
        {
            String codigoLivre = String.Empty;

            //nossonumero:= FormatDateTime('yy', Sistema.DataMvto) + '2' + StrZero(ns, 5) + DVNossoNumero;
            //codigo:= '7489';
            //Codigo2:= StrZero2(FatorVcto, 4) + _Valor;
            //CodLivre:= TipoCobrancaSicredi + '1' + nossonumero + agencia + posto + cedente + '10';
            //CodLivre:= CodLivre + DvCampoLivre(CodLivre);
            //Dvg:= DVGeral(Codigo + Codigo2 + CodLivre);
            //Result:= Codigo + DVG + Codigo2 + CodLivre;


            codigoLivre += Cedente.ContaBancaria.TipoFormaCadastramento == TipoFormaCadastramento.SemRegistro ? "1" : "3"; //com registro
            codigoLivre += "1"; //carteira simples
            codigoLivre += boleto.NossoNumero + boleto.NossoNumeroDV;
            codigoLivre += Cedente.ContaBancaria.Agencia;
            codigoLivre += Cedente.ContaBancaria.Posto;
            codigoLivre += Cedente.Codigo + Cedente.CodigoDV;
            codigoLivre += "1";
            codigoLivre += "0";
            var dv = codigoLivre.CalcularDVSicredi();
            //verificador
            return codigoLivre + dv ;

        }

        public void FormataNossoNumero(Boleto boleto)
        {
            var carteira = CarteiraFactory<BancoSicredi>.ObterCarteira(boleto.CarteiraComVariacao);
            carteira.FormataNossoNumero(boleto);
        }

        public string FormatarNomeArquivoRemessa(int numeroSequencial)
        {
            throw new NotImplementedException();
        }

        public string GerarDetalheRemessa(TipoArquivo tipoArquivo, Boleto boleto, ref int numeroRegistro)
        {
            throw new NotImplementedException();
        }

        public string GerarHeaderRemessa(TipoArquivo tipoArquivo, int numeroArquivoRemessa, ref int numeroRegistro)
        {
            throw new NotImplementedException();
        }

        public string GerarTrailerRemessa(TipoArquivo tipoArquivo, int numeroArquivoRemessa, ref int numeroRegistroGeral, decimal valorBoletoGeral, int numeroRegistroCobrancaSimples, decimal valorCobrancaSimples, int numeroRegistroCobrancaVinculada, decimal valorCobrancaVinculada, int numeroRegistroCobrancaCaucionada, decimal valorCobrancaCaucionada, int numeroRegistroCobrancaDescontada, decimal valorCobrancaDescontada)
        {
            throw new NotImplementedException();
        }

        public void LerDetalheRetornoCNAB240SegmentoT(ref Boleto boleto, string registro)
        {
            throw new NotImplementedException();
        }

        public void LerDetalheRetornoCNAB240SegmentoU(ref Boleto boleto, string registro)
        {
            throw new NotImplementedException();
        }

        public void LerDetalheRetornoCNAB400Segmento1(ref Boleto boleto, string registro)
        {
            throw new NotImplementedException();
        }

        public void LerDetalheRetornoCNAB400Segmento7(ref Boleto boleto, string registro)
        {
            throw new NotImplementedException();
        }

        public void LerHeaderRetornoCNAB240(ArquivoRetorno arquivoRetorno, string registro)
        {
            throw new NotImplementedException();
        }

        public void LerHeaderRetornoCNAB400(string registro)
        {
            throw new NotImplementedException();
        }

        public void LerTrailerRetornoCNAB400(string registro)
        {
            throw new NotImplementedException();
        }

        public void ValidaBoleto(Boleto boleto)
        {
            //throw new NotImplementedException();
        }
    }
}
