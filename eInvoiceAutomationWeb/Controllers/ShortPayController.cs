using eInvoiceApplication.DomainModel;
using eInvoiceAutomationWeb.ViewModels;
using Kendo.Mvc.UI;
using SAPSourceMasterApplication.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Data;
using System.ComponentModel;
using System.Data.Entity;
using Kendo.Mvc.Extensions;
using System.IO;
using System.Web.UI;

namespace eInvoiceAutomationWeb.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None)]
    [SessionTimeOutFilter]
    public class ShortPayController : Controller
    {
        private static readonly log4net.ILog LogManager = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        int invoiceMasterID;
        ShortPayIndexViewModel shortPayIndexViewModel;

        
        public ActionResult ShortPay(string documentNo, string status, string SN, bool ReadOnly, int defaultInvoiceMasterID = 0)
        {
            try
            {
                LogManager.Debug("ShortPay: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {

                    shortPayIndexViewModel = new ShortPayIndexViewModel();

                    //if InvoiceMasterID is passed use that, and store in session..
                    if (defaultInvoiceMasterID > 0)
                    {
                        Session["InvoiceMasterID"] = defaultInvoiceMasterID;
                    }

                    if (!String.IsNullOrEmpty(SN))
                    {
                        int SNIndex = SN.IndexOf('_');
                        string newProcId = SN.Substring(0, SNIndex);
                        invoiceMasterID = eInvoiceModelContext.GetInvoiceMasterIDFromProcId(Convert.ToInt32(newProcId));
                        Session["InvoiceMasterID"] = invoiceMasterID;
                        shortPayIndexViewModel.DocumentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(invoiceMasterID);
                        shortPayIndexViewModel.SN = SN;
                    }
                    else
                    {
                        if (Session["InvoiceMasterID"] != null)
                        {
                            invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                            shortPayIndexViewModel.DocumentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(invoiceMasterID);
                        }
                    }
                    if (Session["InvoiceMasterID"] != null)
                    {
                        TempData["DocumentNo"] = shortPayIndexViewModel.DocumentNo;
                        shortPayIndexViewModel.RoutingDetails = new RoutingDetailsViewModel();
                        shortPayIndexViewModel.RoutingDetails.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);
                        shortPayIndexViewModel.ShortPay = eInvoiceModelContext.GetShortPayDetails(invoiceMasterID);
                        //Get CATT for InvoiceMasterID, RoleID of CATT = 2
                        shortPayIndexViewModel.AddressedTo = eInvoiceModelContext.GetCATTFindingsApprover(shortPayIndexViewModel.ShortPay.AddressedTo);
                        //Get CA for InvoiceMasterID, RoleID of CA = 3
                        shortPayIndexViewModel.SentFrom = eInvoiceModelContext.GetCATTFindingsApprover(shortPayIndexViewModel.ShortPay.SentFrom);
                        shortPayIndexViewModel.FromCA = BuildApproversList(shortPayIndexViewModel.SentFrom);

                        if (ReadOnly == false)
                        {
                            switch (status)
                            {
                                case "CA Review":
                                    LogManager.Debug("ShortPay: END");
                                    return PartialView("_ShortPay", shortPayIndexViewModel);
                                default:
                                    LogManager.Debug("ShortPay: END");
                                    return PartialView("_ShortPayReadOnly", shortPayIndexViewModel);
                            }
                        }
                        else
                        {
                            LogManager.Debug("ShortPay: END");
                            return PartialView("_ShortPayReadOnly", shortPayIndexViewModel);
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Either Document Number is not valid or a process may have already been started for this Document Number;Check your Dashboard for Saved Invoices.";
                        return PartialView("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("ShortPay: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        public JsonResult SaveShortPayDetails(InvoiceShortPayLetter invoiceShortPayLetter)
        {
            try
            {
                LogManager.Debug("SaveShortPayDetails: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    if (Session["InvoiceMasterID"] != null)
                      invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    InvoiceShortPayLetter shortPay = new InvoiceShortPayLetter();
                    shortPay.InvoiceMasterID = invoiceMasterID;
                    shortPay.AddressedTo = invoiceShortPayLetter.AddressedTo;
                    shortPay.SentFrom = invoiceShortPayLetter.SentFrom;
                    shortPay.Subject = invoiceShortPayLetter.Subject;
                    shortPay.Date = invoiceShortPayLetter.Date;
                    shortPay.ApprovedPaymentAmount = invoiceShortPayLetter.ApprovedPaymentAmount;
                    shortPay.CAContactNo = invoiceShortPayLetter.CAContactNo;
                    shortPay.CANotes = invoiceShortPayLetter.CANotes;
                    eInvoiceModelContext.SaveShortPayDetails(invoiceMasterID, shortPay);
                    LogManager.Debug("SaveShortPayDetails: END");
                    return Json(invoiceShortPayLetter, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveShortPayDetails: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public ActionResult CAFindingsEmp_Read([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                LogManager.Debug("CAFindingsEmp_Read: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    List<InvoiceCAFindingsEmp> cafindingsEmp = eInvoiceModelContext.GetInvoiceCAFindingsEmp(invoiceMasterID);
                    LogManager.Debug("CAFindingsEmp_Read: END");
                    return Json(cafindingsEmp.ToDataSourceResult(request));
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("CAFindingsEmp_Read: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        public JsonResult FetchApprovers()
        {
            try
            {
                LogManager.Debug("FetchApprovers: START");
                SAPSourceModelContext SAPmodel = new SAPSourceModelContext();
                IEnumerable<ExchangeEmployeeProfile> selectedSAPEmp = null;
                selectedSAPEmp = SAPmodel.FetchExchangeEmployeesList();

                LogManager.Debug("FetchApprovers: END");
                return Json(selectedSAPEmp, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                LogManager.Error("FetchApprovers: ERROR " + ex.Message, ex);
                return null;
            }
        }

        #region Private Methods

        // IM -- 
        // Method - This is invoked from  Approvers grid
        //Method - populates ApproverUserID in Approvers grid in Edit mode
        private List<ExchangeEmployeeProfile> GetApprovers()
        {
            try
            {
                LogManager.Debug("GetApprovers: START");
                using (var SAPSourceModelContext = new SAPSourceModelContext())
                {
                    List<ExchangeEmployeeProfile> approversList = SAPSourceModelContext.FetchExchangeEmployeesList();
                    LogManager.Debug("GetApprovers: END");
                    return approversList;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetApprovers: ERROR " + ex.Message, ex);
                return null;
            }
        }

        private string BuildApproversList(IEnumerable<ExchangeEmployeeProfile> employees)
        {
            try
            {
                LogManager.Debug("BuildApproversList: START");
                string empList = string.Empty;
                foreach (ExchangeEmployeeProfile empl in employees)
                {
                    string empName = empl.FirstName + " " + empl.LastName;
                    empList = empList + (string.IsNullOrEmpty(empList) ? empName : ", " + empName);
                }
                LogManager.Debug("BuildApproversList: END");
                return empList;
            }

            catch (Exception ex)
            {
                LogManager.Error("BuildApproversList: ERROR " + ex.Message, ex);
                return null;
            }
        }
    }
        #endregion
}
