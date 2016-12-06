using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace eInvoice.K2Access
{
    public static class K2Configuration
    {
        public static string ServerName
        {
            get
            {              
                string k2ServerName = ConfigurationManager.AppSettings["K2ServerName"];
                return k2ServerName;
                //return "rok2dev01";

            }
        }

        public static string Domain
        {
            get
            {
                string domain = ConfigurationManager.AppSettings["Domain"];
                return domain;
                //return "sccta";
            }
        }


        public static uint ManagementServerPort
        {

            get
            {
                //string port = "5555";
                string port = ConfigurationManager.AppSettings["K2ManagementServerPort"];
                uint p = 0;
                uint.TryParse(port, out p);

                return p;
            }
        }

        public static uint HostClientPort
        {
            get
            {
                //string port = "5252";
                string port = ConfigurationManager.AppSettings["K2HostClientPort"];
                uint p = 0;
                uint.TryParse(port, out p);
                return p;
            }
        }
    }
}
