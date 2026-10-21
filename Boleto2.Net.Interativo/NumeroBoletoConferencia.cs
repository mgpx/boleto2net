using System;
using System.Text.RegularExpressions;

namespace Boleto2.Net.Interativo
{
    internal sealed class NumeroBoletoConferencia
    {
        internal string Numero { get; private set; }
        internal string Digito { get; private set; }

        internal static NumeroBoletoConferencia Ler(int banco, string original)
        {
            var texto = (original ?? string.Empty).Trim().ToUpperInvariant();
            string expressao;
            switch (banco)
            {
                case 756: expressao = @"^(\d{7})[-.]?(\d)$"; break;
                case 748: expressao = @"^(\d{2})/?(\d{6})[-.]?(\d)?$"; break;
                case 341: expressao = @"^(?:\d{3}/)?(\d{8})(?:[-.](\d))?$"; break;
                case 237:
                case 753:
                    expressao = @"^(?:(?:\d{3}/)|(?:\d{2}(?=\d{11}[0-9P]$)))?(\d{11})[-.]?([0-9P])?$";
                    break;
                case 41: expressao = @"^(\d{8})(?:[-.]?(\d{2}))?$"; break;
                case 1: expressao = @"^(\d{1,17})$"; break;
                default: throw new ArgumentException("Banco não integrado ao fluxo de conferência.");
            }
            var partes = Regex.Match(texto, expressao, RegexOptions.CultureInvariant);
            if (!partes.Success)
                throw new ArgumentException("Nosso número ausente ou em formato não reconhecido para este banco: " + texto);
            return new NumeroBoletoConferencia
            {
                Numero = banco == 748 ? partes.Groups[1].Value + partes.Groups[2].Value : partes.Groups[1].Value,
                Digito = banco == 748 ? partes.Groups[3].Value : partes.Groups.Count > 2 ? partes.Groups[2].Value : string.Empty
            };
        }
    }
}
