using Boleto2Net.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boleto2Net
{
    [CarteiraCodigo("A")]
    internal class BancoSicrediCarteiraA : ICarteira<BancoSicredi>
    {
        internal static Lazy<ICarteira<BancoSicredi>> Instance { get; } = new Lazy<ICarteira<BancoSicredi>>(() => new BancoSicrediCarteiraA());

        private BancoSicrediCarteiraA()
        {

        }

        public void FormataNossoNumero(Boleto boleto)
        {
            if (boleto.NossoNumero.Length <= 5)
            {
                String ano = (boleto.DataEmissao.Year % 100).ToString();

                String verificador = StringExtensions.CalcularDVSicredi(boleto.Banco.Cedente.ContaBancaria.Agencia, 
                    boleto.Banco.Cedente.ContaBancaria.Posto,
                    boleto.Banco.Cedente.Codigo + boleto.Banco.Cedente.CodigoDV,
                    ano, 
                    boleto.NossoNumero,
                    boleto.ByteNossoNumero);

                boleto.NossoNumero = ano + boleto.ByteNossoNumero + boleto.NossoNumero.PadLeft(5, '0') + verificador;
            }
            if (boleto.NossoNumero.Length == 9)
            {
                boleto.NossoNumeroDV = boleto.NossoNumero.Substring(8, 1);
                boleto.NossoNumero = boleto.NossoNumero.Substring(0, 8);
            }
            else if (boleto.NossoNumero.Length == 8)
            {
                boleto.NossoNumero = boleto.NossoNumero.PadLeft(8, '0');
                boleto.NossoNumeroDV = boleto.NossoNumero.CalcularDVSicredi();
                String ano = (boleto.DataEmissao.Year % 100).ToString();
            }

            //192005001
            boleto.NossoNumeroFormatado = boleto.NossoNumero.Substring(0, 2) + "/" + boleto.NossoNumero.Substring(2, 6) + "-" + boleto.NossoNumeroDV;
        }

        public string FormataCodigoBarraCampoLivre(Boleto boleto)
        {
            var cedente = boleto.Banco.Cedente;
            var contaBancaria = cedente.ContaBancaria;
            var formataCampoLivre = $"{cedente.Codigo}{cedente.CodigoDV}{boleto.NossoNumero.Substring(2, 3)}1{boleto.NossoNumero.Substring(5, 3)}4{boleto.NossoNumero.Substring(8, 9)}";
            formataCampoLivre += formataCampoLivre.CalcularDVCaixa();
            return formataCampoLivre;
        }
    }

}
