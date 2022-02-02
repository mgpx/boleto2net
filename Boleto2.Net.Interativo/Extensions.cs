using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boleto2.Net.Interativo
{
    public static class Extensions
    {

        public static T GetSafeValue<T>(this NpgsqlDataReader reader, String fieldName)
        {
            int ordinal = reader.GetOrdinal(fieldName);
            Boolean isNull = reader.IsDBNull(ordinal);

            if (isNull)
                return default(T);

            if (typeof(T) == typeof(String))
            {
                return (T)Convert.ChangeType(reader.GetString(ordinal), typeof(String));
            }
            else if (typeof(T) == typeof(DateTime?) || typeof(T) == typeof(DateTime))
            {
                return (T)Convert.ChangeType(reader.GetDateTime(ordinal), typeof(DateTime));
            }
            else if (typeof(T) == typeof(Decimal?) || typeof(T) == typeof(Decimal))
            {
                return (T)Convert.ChangeType(reader.GetDecimal(ordinal), typeof(Decimal));
            }
            else if (typeof(T) == typeof(Int32?))
            {
                return (T)Convert.ChangeType(reader.GetDecimal(ordinal), typeof(Int32));
            }
            else if (typeof(T) == typeof(Int64) || typeof(T) == typeof(Int64?))
            {
                return (T)Convert.ChangeType(reader.GetDecimal(ordinal), typeof(Int64));
            }
            else if (typeof(T) == typeof(Object))
            {
                return (T)reader.GetValue(ordinal);
            }


            throw new NotImplementedException();

        }

        public static string RemoveAccents(this string text)
        {
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }

        public static string Reduzido(this string text, Int32 lenght)
        {
            String retorno = "";
            if (text == null)
                return null;

            retorno = text;
            if (text.Length > lenght)
                retorno = text.Substring(0, lenght);

            return retorno;
        }

        public static string GetPart(this string text, int startIndex, int endIndex)
        {
            //start index comeca em 1
            var index = startIndex - 1;
            return text.Substring(index, (endIndex - startIndex) + 1);
        }

        public static void AjeitaDataGridView(this DataGridView dataGridView)
        {
            //para deixar o tamanho "certo e editavel" o tamanho da coluna // all cells bloqueia o usuario a nao editar
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            for (int i = 0; i < dataGridView.Columns.Count; i++)
            {
                int colw = dataGridView.Columns[i].Width;
                if (dataGridView.Columns[i].ValueType == typeof(Decimal))
                {
                    dataGridView.Columns[i].DefaultCellStyle.Format = "N2";
                }
                //
                dataGridView.Columns[i].Width = colw;
            }

            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        }

        public static void EnableDoubleBuffered(this Control control, bool setting)
        {
            Type dgvType = control.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                  BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(control, setting, null);
        }
    }
}
