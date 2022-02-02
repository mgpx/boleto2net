using System;

namespace Boleto2Net.Extensions
{
    public static class StringExtensions
    {
        public static string Right(this string value, int length)
        {
            if (String.IsNullOrEmpty(value))
                return string.Empty;
            return value.Length <= length ? value : value.Substring(value.Length - length);
        }
        public static string Left(this string value, int length)
        {
            if (String.IsNullOrEmpty(value))
                return string.Empty;
            return value.Length <= length ? value : value.Substring(0, length);
        }

        public static string MidVB(this string str, int start, int length)
        {
            return str.Mid(--start,length);
        }

        public static string Mid(this string str, int startIndex, int length)
        {
            if (str.Length <= 0 || startIndex >= str.Length) return string.Empty;
            if (startIndex + length > str.Length)
            {
                length = str.Length - startIndex;
            }
            return str.Substring(startIndex, length);
        }

        public static string CalcularDVCaixa(this string texto)
        {
            string digito;
            int pesoMaximo = 9, soma = 0, peso = 2;
            for (var i = texto.Length - 1; i >= 0; i--)
            {
                soma = soma + Convert.ToInt32(texto.Substring(i, 1)) * peso;
                if (peso == pesoMaximo)
                    peso = 2;
                else
                    peso = peso + 1;
            }
            var resto = soma % 11;
            if (resto <= 1)
                digito = "0";
            else
                digito = (11 - resto).ToString();
            return digito;
        }

        public static string CalcularDVSantander(this string texto)
        {
            string digito;
            int pesoMaximo = 9, soma = 0, peso = 2;
            for (var i = texto.Length - 1; i >= 0; i--)
            {
                soma = soma + Convert.ToInt32(texto.Substring(i, 1)) * peso;
                if (peso == pesoMaximo)
                    peso = 2;
                else
                    peso = peso + 1;
            }
            var resto = soma % 11;
            if (resto <= 1)
                digito = "0";
            else
                digito = (11 - resto).ToString();
            return digito;
        }

        public static string CalcularDVSicoob(this string texto)
        {
            string digito, fatorMultiplicacao = "319731973197319731973";
            int soma = 0;
            for (int i = 0; i < 21; i++)
            {
                soma += Convert.ToInt16(texto.Substring(i, 1)) * Convert.ToInt16(fatorMultiplicacao.Substring(i, 1));
            }
            int resto = (soma % 11);
            if (resto <= 1)
                digito = "0";
            else
                digito = (11 - resto).ToString();
            return digito;
        }

        public static string CalcularDVBradesco(this string texto)
        {
            string digito;
            int pesoMaximo = 7, soma = 0, peso = 2;
            for (var i = texto.Length - 1; i >= 0; i--)
            {
                soma = soma + (int)char.GetNumericValue(texto[i]) * peso;
                if (peso == pesoMaximo)
                    peso = 2;
                else
                    peso = peso + 1;
            }
            var resto = soma % 11;
            switch (resto)
            {
                case 0:
                    digito = "0";
                    break;
                case 1:
                    digito = "P";
                    break;
                default:
                    digito = (11 - resto).ToString();
                    break;
            }
            return digito;
        }

        public static string CalcularDVItau(this string texto)
        {
            string digito;
            int soma = 0, peso = 2, digTmp = 0;
            for (var i = texto.Length - 1; i >= 0; i--)
            {
                digTmp = (int)char.GetNumericValue(texto[i]) * peso;
                if (digTmp > 9)
                    digTmp = (digTmp / 10) + (digTmp % 10);

                soma = soma + digTmp;

                if (peso == 2)
                    peso = 1;
                else
                    peso = peso + 1;
            }
            var resto = (soma % 10);
            if (resto == 0)
                digito = "0";
            else
                digito = (10 - resto).ToString();
            return digito;
        }

        public static string CalcularDVSicredi(this string texto)
        {
            /* Variáveis
             * -------------
             * d - Dígito
             * s - Soma
             * p - Peso
             * b - Base
             * r - Resto
             */

            int d, s = 0, p = 2, b = 9;
            //Atribui os pesos de {2..9}
            for (int i = texto.Length - 1; i >= 0; i--)
            {
                s = s + (Convert.ToInt32(texto.Substring(i, 1)) * p);
                if (p < b)
                    p = p + 1;
                else
                    p = 2;
            }
            d = 11 - (s % 11);//Calcula o Módulo 11;
            if (d > 9)
                d = 0;
            return d.ToString();
        }

        public static string CalcularDVSicredi(String agencia, String posto, String cedente, String ano, String seq, String byteGer = "2")
        {
            if (byteGer == "1")
                throw new ArgumentOutOfRangeException("Não pode ser 1");

            String dados = agencia.PadLeft(4, '0') + posto.PadLeft(2, '0') + cedente.PadLeft(5, '0') + ano + byteGer + seq.PadLeft(5, '0');
            int soma = 0;
            for (int controle = 0; controle < dados.Length; controle++)
            {
                int n = int.Parse(dados.Substring(controle, 1));

                if (controle == 0 || controle == 8 || controle == 16)
                {
                    soma += n * 4;
                }
                else if (controle == 1 || controle == 9 || controle == 17)
                {
                    soma += n * 3;
                }
                else if (controle == 2 || controle == 10 || controle == 18)
                {
                    soma += n * 2;
                }
                else if (controle == 3 || controle == 11)
                {
                    soma += n * 9;
                }
                else if (controle == 4 || controle == 12)
                {
                    soma += n * 8;
                }
                else if (controle == 5 || controle == 13)
                {
                    soma += n * 7;
                }
                else if (controle == 6 || controle == 14)
                {
                    soma += n * 6;
                }
                else if (controle == 7 || controle == 15)
                {
                    soma += n * 5;
                }
            }

            // se for maqior que 9 será 0
            string digitoAutoConferencia = 11 - (soma % 11) > 9 ? "0" : (11 - (soma % 11)).ToString();

            //return dados.Substring(11) + digitoAutoConferencia; // Número no Banco já Montado
            return  digitoAutoConferencia; // Número no Banco já Montado
        }
    
    }
}
