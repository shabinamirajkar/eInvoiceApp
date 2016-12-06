using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class InvoicePODetailChanges
    {
       
        [Key]
        public int InvoiceDetailChangesID { get; set; }

        public int InvoiceMasterID { get; set; }
        [ForeignKey("InvoiceMasterID")]
        public InvoiceMaster InvoiceMaster { get; set; }

        public string PONumber { get; set; }
        public int? POLine { get; set; }
        public int? GLAccount { get; set; }
        public int? CostCenter { get; set; }
        public string WBS { get; set; }
        public int? Fund { get; set; }
        public string FunctionalArea { get; set; }
        public string GrantNumber { get; set; }
        public string InternalOrder { get; set; }
        public Decimal? InvoiceAmount { get; set; }
        public String Notes { get; set; }
        public String EditedByUserID { get; set; }
        public DateTime EditedByDate { get; set; }
        public string EditedFlag { get; set; }

    }
}
