using System;
using Boleto2Net.Extensions;
using static System.String;

namespace Boleto2Net
{
    [CarteiraCodigo("109")]
    internal class BancoItauCarteira109 : ICarteira<BancoItau>
    {
        internal static Lazy<ICarteira<BancoItau>> Instance { get; } = new Lazy<ICarteira<BancoItau>>(() => new BancoItauCarteira109());

        private BancoItauCarteira109()
        {

        }

        public void FormataNossoNumero(Boleto boleto)
        {
            if (IsNullOrWhiteSpace(boleto.NossoNumero))
                throw new Exception("Nosso Número não informado.");

            // Nosso número não pode ter mais de 8 dígitos
            if (boleto.NossoNumero.Length <= 8)
            {
                boleto.NossoNumero = boleto.NossoNumero.PadLeft(8, '0');
                boleto.NossoNumeroDV = (boleto.Banco.Cedente.ContaBancaria.Agencia + boleto.Banco.Cedente.ContaBancaria.Conta + boleto.Carteira + boleto.NossoNumero).CalcularDVItau();
                boleto.NossoNumeroFormatado = $"{boleto.Carteira}/{boleto.NossoNumero}-{boleto.NossoNumeroDV}";
            }
            else if (boleto.NossoNumero.Length == 14)
            {
                boleto.NossoNumeroFormatado = boleto.NossoNumero;
                boleto.NossoNumero = boleto.NossoNumero.Substring(4, 8);
                boleto.NossoNumeroDV = boleto.NossoNumeroFormatado.Substring(13, 1);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public string FormataCodigoBarraCampoLivre(Boleto boleto)
        {
            //return $"{boleto.Carteira}{boleto.NossoNumero}{boleto.NossoNumeroDV}{boleto.Banco.Cedente.ContaBancaria.Agencia}{boleto.Banco.Cedente.ContaBancaria.Conta}{boleto.Banco.Cedente.ContaBancaria.DigitoConta}000";
            return $"{boleto.Carteira}{boleto.NossoNumero}{boleto.NossoNumeroDV}{boleto.Banco.Cedente.ContaBancaria.Agencia}{boleto.Banco.Cedente.ContaBancaria.Conta}{boleto.Banco.Cedente.ContaBancaria.DigitoConta}000";
        }
    }
}