using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class InvoiceCATTFindings
    {
       // EInvoiceModelContext eInvoiceModelContext;

        [Key]
        public int InvoiceCATTFindingsID { get; set; }
      
        public int InvoiceMasterID { get; set; }
        [ForeignKey("InvoiceMasterID")]
        public InvoiceMaster InvoiceMaster { get; set; }

        public string AddressedTo { get; set; }
        public string SentFrom { get; set; }
        public DateTime? Date { get; set; }
        public Decimal? CATTRecommendedAdjustment { get; set; }
        public Decimal? CARecommendedAdjustment { get; set; }
        public String CATTNotes { get; set; }
        public String CANotes { get; set; }
       
    }
}
