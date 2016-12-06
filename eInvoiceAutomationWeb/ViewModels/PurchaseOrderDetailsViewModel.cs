using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eInvoiceApplication.DomainModel;

namespace eInvoiceAutomationWeb.ViewModels
{
    public class PurchaseOrderDetailsViewModel
    {
        public RoutingDetailsViewModel RoutingDetails { get; set; }
        public IEnumerable<InvoicePODetail> InvoicePODetails { get; set; }
        public IEnumerable<InvoicePODetailChanges> InvoicePODetailChanges { get; set; }
        public IEnumerable<InvoicePODetailChangesLog> InvoicePODetailChangesLog { get; set; }
        public string DocumentNo { get; set; }
        public bool ShowPOWarning { get; set; }
        public bool ShowModifyFlag { get; set; }
    }

    public class AccountingCostCodesViewModel
    {
        public int InvoiceDetailID { get; set; }
        public int InvoiceMasterID { get; set; }
        public string PONumber { get; set; }
        public Nullable<int> POLine { get; set; }
        public Nullable<int> GLAccount { get; set; }
        public Nullable<int> CostCenter { get; set; }
        public string WBS { get; set; }
        public Nullable<int> Fund { get; set; }
        public string FunctionalArea { get; set; }
        public string GrantNumber { get; set; }
        public string InternalOrder { get; set; }
        public Nullable<decimal> InvoiceAmount { get; set; }

        public string FormattedPONumber
        {
            get
            {
                if (string.IsNullOrEmpty(PONumber))
                    return string.Empty;
                else
                    return PONumber.TrimStart('0');
            }
        }
        public string FormattedGrantNumber
        {
            get
            {
                if (string.IsNullOrEmpty(GrantNumber))
                    return string.Empty;
                else
                    return GrantNumber.TrimStart('0');
            }
        }
    }

    public class ModifyAccountingCostCodesViewModel
    {
        public int InvoiceDetailChangesID { get; set; }
        public int InvoiceMasterID { get; set; }
        public string SAPPONumber { get; set; }
        public Nullable<int> SAPPOLine { get; set; }
        public int? GLAccount { get; set; }
        public int? CostCenter { get; set; }
        public string WBSNo { get; set; }
        public int? Fund { get; set; }
        public string FunctionalArea { get; set; }
        public string GrantNumber { get; set; }
        public string InternalOrder { get; set; }
        public Decimal? InvoiceAmount { get; set; }
        public String Notes { get; set; }
        public String EditedFlag { get; set; }

        public string FormattedPONumber
        {
            get
            {
                if (string.IsNullOrEmpty(SAPPONumber))
                    return string.Empty;
                else
                    return SAPPONumber.TrimStart('0');
            }
        }
        public string FormattedGrantNumber
        {
            get
            {
                if (string.IsNullOrEmpty(GrantNumber))
                    return string.Empty;
                else
                    return GrantNumber.TrimStart('0');
            }
        }
        public bool wbsChanged { get; set; }
        public bool costCenterChanged { get; set; }
    }

    public class AccountingCostCodesChangeHistoryViewModel
    {
        public Int64 LogID { get; set; }
        public int InvoiceDetailChangesID { get; set; }
        public int InvoiceMasterID { get; set; }
        public string PONumber { get; set; }
        public Nullable<int> POLine { get; set; }
        public Nullable<int> GLAccount { get; set; }
        public Nullable<int> CostCenter { get; set; }
        public string WBS { get; set; }
        public Nullable<int> Fund { get; set; }
        public string FunctionalArea { get; set; }
        public string GrantNumber { get; set; }
        public string InternalOrder { get; set; }
        public Nullable<decimal> InvoiceAmount { get; set; }
        public String Notes { get; set; }
        public String EditedByUserID { get; set; }
        public String EditedByDate { get; set; }
        public string LogAction { get; set; }
    }


   
}