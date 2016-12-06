using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class DashboardReport
    {
        [Key]
        public int InvoiceMasterID { get; set; }

        public string ContractNo { get; set; }
        public string DocumentNo { get; set; }
        public string VendorName { get; set; }
        public string VendorNo { get; set; }
        public string InvoiceNo { get; set; }
        public decimal InvoiceAmount { get; set; }
        public DateTime PaymentDueBy { get; set; }
        public int DaysPending { get; set; }
        public string InvoiceStatus { get; set; }
        public string DestinationUser { get; set; }
        public DateTime? CompletedDate { get; set; }
    }

    public class DashboardReportSearch
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Status { get; set; }
        public string Access { get; set; }
        public string SubmittedBy { get; set; }
        public string  LoggedinUserType { get; set; }
    }


}
