using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class InvoiceComment
    {
        eInvoiceModelContext eInvoiceModelContext = new eInvoiceModelContext();

        [Key]
        public int InvoiceCommentID { get; set; }

        public int InvoiceMasterID { get; set; }
        [ForeignKey("InvoiceMasterID")]
        public InvoiceMaster InvoiceMaster { get; set; }

        public string Comment { get; set; }
        public string CommentBy { get; set; }
        public DateTime CommentDate { get;set; }
    }
}
