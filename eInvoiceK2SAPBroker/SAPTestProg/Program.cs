using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace eInvoiceK2SAPBroker
{
    class Program
    {
        static void Main(string[] args)
        {

            eInvoiceK2SAPBroker.eInvoiceLoadDocNoFromSAP.LoadDocNoFromSAP("5100076480", " ");
            //eInvoiceK2SAPBroker.eInvoiceLoadDocNoFromSAP.LoadDocNoFromSAP("5100076480", "Y");
            
        }
    }
}
