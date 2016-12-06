using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class InvoiceShortPayLetter
    {
        [Key]
        public int ShortPayLetterID { get; set; }

        public int InvoiceMasterID { get; set; }
        [ForeignKey("InvoiceMasterID")]
        public InvoiceMaster InvoiceMaster { get; set; }

        public String AddressedTo { get; set; }
        public String SentFrom { get; set; }
        public String Subject { get; set; }
        public DateTime? Date { get; set; }
        public String CAContactNo { get; set; }
        public Decimal? ApprovedPaymentAmount { get; set; }
        public String CANotes { get; set; }
    }
}
