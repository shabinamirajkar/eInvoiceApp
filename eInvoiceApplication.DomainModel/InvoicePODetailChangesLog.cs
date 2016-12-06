using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace eInvoiceApplication.DomainModel
{
    public class InvoicePODetailChangesLog
    {
        [Key]
        public Int64 LogID { get; set; }
        [Required]
        public int InvoiceDetailChangesID { get; set; }
        [Required]
        public int InvoiceMasterID { get; set; }
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
        [Required]
        public String EditedByUserID { get; set; }
        [Required]
        public DateTime EditedByDate { get; set; }
        [Required]
        public string LogAction { get; set; }
        public DateTime LogDate { get; set; }

    }
}
