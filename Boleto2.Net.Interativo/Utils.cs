using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boleto2.Net.Interativo
{
    public class Utils
    {
        public class ConnectionServer
        {
            public String Ip { get; set; } = "127.0.0.1";
            public String DatabaseName { get; set; } = "interativo";

        }

        public class ConnectionPrinter
        {
            public String PortaComMiniImpr { get; set; } = "USB";
            public String ModeloMiniImpr { get; set; } = "GENERIC";
        }

        public static ConnectionServer GetIpServer()
        {
            ConnectionServer conn = new Utils.ConnectionServer();
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Interativo\\Config"))
                {
                    if (key != null)
                    {
                        Object ipServer = key.GetValue("IpServer");
                        Object databaseName = key.GetValue("DatabaseName");

                        if (ipServer != null)
                            conn.Ip = ipServer.ToString();

                        if (databaseName != null)
                            conn.DatabaseName = databaseName.ToString();
                    }
                }



            }
            catch (Exception ex)
            {

            }
            return conn;
        }

        public static ConnectionPrinter GetPrinter()
        {
            ConnectionPrinter conn = new Utils.ConnectionPrinter();
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Interativo\\Vendas"))
            {
                if (key != null)
                {
                    Object portaComMiniImpr = key.GetValue("PortaComMiniImpr");
                    Object modeloMiniImpr = key.GetValue("ModeloMiniImpr");

                    if (portaComMiniImpr != null)
                        conn.PortaComMiniImpr = portaComMiniImpr.ToString();

                    if (modeloMiniImpr != null)
                        conn.ModeloMiniImpr = modeloMiniImpr.ToString();
                }
            }

            return conn;
        }
    }
}
