using eInvoiceApplication.DomainModel;
using eInvoiceAutomationWeb.ViewModels;
using InfoMajesty.CustomWorklist;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using SAPSourceMasterApplication.DomainModel;
using System.Configuration;
using eInvoiceAutomationWeb.PDF;

namespace eInvoiceAutomationWeb.Controllers
{
    public class ShortPayPDFController : PdfViewController
    {
        private static readonly log4net.ILog LogManager = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        int invoiceMasterID;
        public ActionResult GenerateShortPayPDF(string documentNo)
        {
            if (string.IsNullOrEmpty(documentNo)) { throw new Exception("Document No. cannot be empty"); }

            ShortPayIndexViewModel shortPayIndexViewModel; //= new ShortPayIndexViewModel();
            try
            {
            using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                if (Session["InvoiceMasterID"] != null)
                    invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                else
                    invoiceMasterID = eInvoiceModelContext.GetInvoiceMasterIDFilterOnStatus(documentNo);

                };
                PDFHelper.GenerateShortPayPDF(invoiceMasterID, out documentNo, out shortPayIndexViewModel);

                //LogManager.Debug("GenerateShortPayPDF: START");
                //using (var eInvoiceModelContext = new eInvoiceModelContext())
                //{
                //    if (Session["InvoiceMasterID"] != null)
                //        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                //    else
                //        invoiceMasterID = eInvoiceModelContext.GetInvoiceMasterIDFilterOnStatus(documentNo);

                //    if (invoiceMasterID == 0) { throw new Exception("Document No. is not valid"); }

                //    shortPayIndexViewModel.ShortPay = eInvoiceModelContext.GetShortPayDetails(invoiceMasterID);

                //    //in case CA is not avilable during PDF Generation..
                //    if (shortPayIndexViewModel.SentFrom != null)
                //    {
                //        //Get CA for InvoiceMasterID, RoleID of CA = 3
                //        shortPayIndexViewModel.SentFrom = eInvoiceModelContext.GetCATTFindingsApprover(shortPayIndexViewModel.ShortPay.SentFrom);
                //        //  shortPayIndexViewModel.ShortPay.SentFrom = BuildEmployeeCSV(shortPayIndexViewModel.SentFrom);
                //        shortPayIndexViewModel.ShortPay.SentFrom = shortPayIndexViewModel.SentFrom.FirstOrDefault().ApproverName;
                //    }

                //    shortPayIndexViewModel.RoutingDetails = new RoutingDetailsViewModel();
                //    shortPayIndexViewModel.RoutingDetails.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);

                //    shortPayIndexViewModel.ShortPayNotesDefault = eInvoiceModelContext.GetConfigMiscData().Where(p => p.ConfiguredCol == "ShortPayNotesDefault").FirstOrDefault().ConfiguredColText;

                //    //in case CA is not avilable during PDF Generation..
                //    if (shortPayIndexViewModel.SentFrom != null)
                //    {
                //        shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("@CAFullName", shortPayIndexViewModel.ShortPay.SentFrom);
                //        shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("@emailaddress", shortPayIndexViewModel.SentFrom.FirstOrDefault().WorkEmail);
                //    }
                //    else
                //    {
                //        shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("@CAFullName", string.Empty);
                //        shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("@emailaddress", string.Empty);
                //    }
                //    shortPayIndexViewModel.InvoiceGridTotalsforPDF = new InvoiceGridTotalsforPDF();
                //    shortPayIndexViewModel.InvoiceGridTotalsforPDF = eInvoiceModelContext.GetInvoiceGridTotalsforPDF(invoiceMasterID);

                //    string ApprovedAmt = String.Format("{0:C}", shortPayIndexViewModel.InvoiceGridTotalsforPDF.CATotal.Value);
                //    shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("$CAApprovedAdjustment", ApprovedAmt);

                //    shortPayIndexViewModel.InvoiceCAFindingsEmp = new List<InvoiceCAFindingsEmp>();
                //    //CA Line Items Grid..
                //    shortPayIndexViewModel.InvoiceCAFindingsEmp = eInvoiceModelContext.GetInvoiceCAFindingsEmp(invoiceMasterID);

                //    if (shortPayIndexViewModel.InvoiceCAFindingsEmp != null)
                //    {
                //        //Manually Adding Totals in Grid Last row.
                //        shortPayIndexViewModel.InvoiceCAFindingsEmp.Add(
                //            new InvoiceCAFindingsEmp
                //            {
                //                EmployeeName = "Total:",
                //                RateVariance = shortPayIndexViewModel.InvoiceGridTotalsforPDF.CARateVariance.Value,
                //                Total = shortPayIndexViewModel.InvoiceGridTotalsforPDF.CATotal.Value,
                //            });
                //    }
                //}
                //LogManager.Debug("GenerateShortPayPDF: START");
                return this.ViewPdf("", "_GeneratePDF", shortPayIndexViewModel);
            }

            catch (Exception ex)
            {
                LogManager.Error("GenerateShortPayPDF: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        //public ActionResult CAFindingsEmp_Read([DataSourceRequest] DataSourceRequest request)
        //{
        //    try
        //    {
        //        LogManager.Debug("CAFindingsEmp_Read: START");
        //        using (var eInvoiceModelContext = new eInvoiceModelContext())
        //        {
        //            invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
        //            List<InvoiceCAFindingsEmp> cafindingsEmp = eInvoiceModelContext.GetInvoiceCAFindingsEmp(invoiceMasterID);
        //            LogManager.Debug("CAFindingsEmp_Read: END");
        //            return Json(cafindingsEmp.ToDataSourceResult(request));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error("CAFindingsEmp_Read: ERROR " + ex.Message, ex);
        //        return PartialView("Error");
        //    }
        //}

        //private string BuildEmployeeCSV(IEnumerable<ExchangeEmployeeProfile> employees)
        //{
        //    try
        //    {
        //        LogManager.Debug("BuildEmployeeCSV: START");
        //        string empcsv = string.Empty;
        //        foreach (ExchangeEmployeeProfile sepCATT in employees)
        //        {
        //            empcsv = empcsv + (string.IsNullOrEmpty(empcsv) ? sepCATT.ApproverName : ", " + sepCATT.ApproverName);
        //        }
        //        LogManager.Debug("BuildEmployeeCSV: END");
        //        return empcsv;
        //    }

        //    catch (Exception ex)
        //    {
        //        LogManager.Error("BuildEmployeeCSV: ERROR " + ex.Message, ex);
        //        return null;
        //    }
        //}

    }
}