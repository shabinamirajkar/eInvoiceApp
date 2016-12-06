using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eInvoiceApplication.DomainModel;
using SAPSourceMasterApplication.DomainModel;
using System.ComponentModel.DataAnnotations;

namespace eInvoiceAutomationWeb.ViewModels
{
    public class CATTFindingsViewModel
    {
        public RoutingDetailsViewModel RoutingDetails { get; set; }
        public InvoiceCATTFindings InvoiceCATTFindings { get; set; }
        public List<InvoiceCATTFindingsEmp> InvoiceCATTFindingsEmp { get; set; }
        public List<InvoiceCAFindingsEmp> InvoiceCAFindingsEmp { get; set; }
        public IEnumerable<ExchangeEmployeeProfile> ToCA { get; set; }
        public IEnumerable<ExchangeEmployeeProfile> FromCATT { get; set; }
        public string ToCACSV { get; set; }
        public string  FromCATTCSV { get; set; }
        public DateTime? DateSubmit { get; set; }
        public string DocumentNo { get; set; }
        [DataType(DataType.Currency)]
        public decimal? AssetPayment { get; set; }
        [DataType(DataType.Currency)]
        public decimal? ApprovedPayment { get; set; }
        public string LoggedinUserType { get; set; }
        public bool CATTRecommendedAdjustmentReadOnly { get; set; }
        public bool CARecommendedAdjustmentReadOnly { get; set; }
        public bool CATTNotesReadOnly { get; set; }
        public bool CANotesReadOnly { get; set; }
        //public decimal TotalAdjustment { get; set; }
        //public decimal TotalAdjustmentPayment { get; set; }
        //public decimal ApprovedAdjustment { get; set; }
        //public decimal ApprovedAdjustmentPayment { get; set; }
        //public string CATTNotes { get; set; }
        //public string CANotes { get; set; }

    }

}