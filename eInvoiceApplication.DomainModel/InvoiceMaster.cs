using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class InvoiceMaster
    {
        //EInvoiceModelContext eInvoiceModelContext;
        
        [Key]
        public int InvoiceMasterID { get; set; }
        public int? IAProcID { get; set; }
        [Required]
        public string DocumentNo { get; set; }
        [Required]
        public string InvoiceNo { get; set; }

        [Range(0, 99999999999999999.99)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0,0.00}")]
        public decimal? InvoiceAmount { get; set; }

        public string VendorNo { get; set;}
        public string VendorName {get; set;}
        public string ContractNo { get; set;}
        public string Period {get; set;}
        public string Project {get; set;}
        public DateTime PaymentDueBy { get; set; }
        public string PostedParkedBy { get; set; }
        public int? CATTThreshold { get; set;}

        [Display(Name="Require CATT Approval")]
        public Nullable<bool> CATTApprovalRequired { get; set;}

        public string SESNumber { get; set;}
        public DateTime? APSubmittedDate { get; set;}
        public string APSubmittedByUserID { get; set;}
        public string Status { get; set;}
        public Nullable<bool> NonContractingStatus { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceType { get; set; }
    }

}
