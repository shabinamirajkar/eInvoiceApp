using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class InvoicePOApproverMemoryProfile
    {
        public InvoicePOApproverMemoryProfile()
        {
        }

        [Key]
        public int POApproverMemoryID { get; set;}
        public int RoleID { get; set;}
        [ForeignKey("RoleID")]
        public ConfigRole ConfigRole { get; set; }
        public String PONumber { get; set;}
        public int POLine { get; set;}
        [Required]
        public String ApproverUserID { get; set;}
        public String ApproverSuggestedbySAP { get; set;}
       
    }
}
