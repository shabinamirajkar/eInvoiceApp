using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class InvoiceRoutingRecord
    {
        [Key]
        public int InvoiceRoutingID { get; set; }

        public int InvoiceMasterID { get; set; }
        [ForeignKey("InvoiceMasterID")]
        public InvoiceMaster InvoiceMaster { get; set; }

        public int RoleID { get; set; }
        [ForeignKey("RoleID")]
        public ConfigRole ConfigRole { get; set; }

        public String ApproverUserID { get; set; }
        public DateTime DateTime { get; set; }
        public String Action { get; set; }
    }

    public class eInvoiceApprovers
    {
        public string Role { get; set; }
        public string ApproverUserID { get; set; }
        public string ApproverFullName { get; set; }
        public DateTime DateTime { get; set; }
    }
}
