using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eInvoiceApplication.DomainModel;
using SAPSourceMasterApplication.DomainModel;
using System.ComponentModel.DataAnnotations;

namespace eInvoiceAutomationWeb.ViewModels
{
    public class PDFDocumentViewModel
    {
        public CATTFindingsViewModel CATTFindingsViewModel { get; set; }
        public PurchaseOrderDetailsViewModel PurchaseOrderDetailsViewModel { get; set; }
        public RoutingDetailsViewModel RoutingDetailsViewModel { get; set; }
        public List<ApproversViewModel> ApproversViewModel { get; set; }
        public List<AttachmentsViewModel> AttachmentsViewModel { get; set; }
        public List<CommentsViewModel> CommentsViewModel { get; set; }
        public ShortPayIndexViewModel ShortPayIndexViewModel { get; set; }
        public List<ModifyAccountingCostCodesViewModel> ModifyAccountingCostCodesViewModel { get; set; }
        public List<AccountingCostCodesViewModel> AccountingCostCodesViewModel { get; set; }
        public List<eInvoiceApprovers> eInvoiceApprovers { get; set; }
        public InvoiceGridTotalsforPDF InvoiceGridTotalsforPDF { get; set; }
    }
}