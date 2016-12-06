using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class InvoiceAttachment
    {
        eInvoiceModelContext eInvoiceModelContext = new eInvoiceModelContext();

        [Key]
        public int InvoiceAttachmentID { get; set; }

        public int InvoiceMasterID { get; set; }
        [ForeignKey("InvoiceMasterID")]
        public InvoiceMaster InvoiceMaster { get; set; }

        public string FileName { get; set; }
        [Required]
        public Byte[] FileAttachment { get; set; }
        [Required]
        public string UploadedUserID { get; set; }
    }
}
