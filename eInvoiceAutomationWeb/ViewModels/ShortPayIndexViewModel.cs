using eInvoiceApplication.DomainModel;
using SAPSourceMasterApplication.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eInvoiceAutomationWeb.ViewModels
{
    public class ShortPayIndexViewModel
    {
        public string DocumentNo { get; set; }
        public string SN { get; set; }
        public bool ShowPOWarning { get; set; }
        public RoutingDetailsViewModel RoutingDetails { get; set; }
        public InvoiceShortPayLetter ShortPay { get; set; }
        public IEnumerable<ExchangeEmployeeProfile> AddressedTo { get; set; }
        public IEnumerable<ExchangeEmployeeProfile> SentFrom { get; set; }
        public string FromCA { get; set; }
        public string ShortPayNotesDefault { get; set; }
        public List<InvoiceCAFindingsEmp> InvoiceCAFindingsEmp { get; set; }
        public InvoiceGridTotalsforPDF InvoiceGridTotalsforPDF { get; set; }
    }
}