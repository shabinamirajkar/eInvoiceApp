using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class InvoicePOApprover
    {
        public InvoicePOApprover()
        {
        }

        [Key]
        public int InvoicePOApproverID { get; set;}
        public int InvoiceMasterID { get; set; }
        public String Role { get; set; } 
        public String PONumber { get; set;}
        public Nullable<int> POLine { get; set;}
        [Required]
        public String ApproverUserID { get; set;}
        public String ApproverSuggestedbySAP { get; set;}

        //public String FormattedPONumber
        //{
        //    get { return PONumber.TrimStart('0'); } 
        //}
    }
}
