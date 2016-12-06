using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace eInvoiceApplication.DomainModel
{
    public class InvoicePODetail
    {
        [Key]
        public int InvoiceDetailID { get; set; }

        public int InvoiceMasterID { get; set; }
        [ForeignKey("InvoiceMasterID")]
        public InvoiceMaster InvoiceMaster { get; set; }

        public string PONumber { get; set; }
        public Nullable<int> POLine { get; set; }
        public Nullable<int> GLAccount { get; set; }
        public Nullable<int> CostCenter { get; set; }
        public string WBS { get; set; }
        public Nullable<int> Fund { get; set; }
        public string FunctionalArea { get; set; }
        public string GrantNumber { get; set; }
        public string InternalOrder { get; set; }
        public Nullable<decimal> InvoiceAmount { get; set; }

        public String FormattedPONumber
        {
           get 
           {
               if (PONumber != null)
                   return PONumber.TrimStart('0');
               else
                   return ""; 
            }
        }
   }
}
