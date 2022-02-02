using Boleto2Net.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.String;

namespace Boleto2Net
{
    [CarteiraCodigo("019")]
    public class BancoNcbbankCarteira19 : ICarteira<BancoNbcbank>
    {

        internal static Lazy<ICarteira<BancoNbcbank>> Instance { get; } = new Lazy<ICarteira<BancoNbcbank>>(() => new BancoNcbbankCarteira19());

        private BancoNcbbankCarteira19()
        {

        }

        public void FormataNossoNumero(Boleto boleto)
        {

            // Nosso número não pode ter mais de 11 dígitos

            if (IsNullOrWhiteSpace(boleto.NossoNumero) || boleto.NossoNumero == "00000000000")
            {
                // Banco irá gerar Nosso Número
                boleto.NossoNumero = new String('0', 11);
                boleto.NossoNumeroDV = "0";
                boleto.NossoNumeroFormatado = "000/00000000000-0";
            }
            else
            {
                //se tiver 17 entao já está formatado
                if (boleto.NossoNumero.Length == 17)
                {
                    boleto.NossoNumeroFormatado = boleto.NossoNumero;
                    boleto.NossoNumero = boleto.NossoNumeroFormatado.Substring(4, 11);
                }
                // Nosso Número informado pela empresa
                if (boleto.NossoNumero.Length > 11)
                    throw new Exception($"Nosso Número ({boleto.NossoNumero}) deve conter 11 dígitos.");
                boleto.NossoNumero = boleto.NossoNumero.PadLeft(11, '0');
                boleto.NossoNumeroDV = (boleto.Carteira + boleto.NossoNumero).CalcularDVBradesco();
                boleto.NossoNumeroFormatado = $"{boleto.Carteira.PadLeft(3, '0')}/{boleto.NossoNumero}-{boleto.NossoNumeroDV}";
            }



        }

        public string FormataCodigoBarraCampoLivre(Boleto boleto)
        {
            //23799789800000400203708|19|14025702920|05259050
            //                   3708|19|00000000016|5259050
            var contaBancaria = boleto.Banco.Cedente.ContaBancaria;
            return $"{contaBancaria.Agencia}{Convert.ToInt32(boleto.Carteira)}{boleto.NossoNumero}{Convert.ToInt64(contaBancaria.Conta).ToString().PadLeft(7,'0')}{"0"}";
        }
    }
}

