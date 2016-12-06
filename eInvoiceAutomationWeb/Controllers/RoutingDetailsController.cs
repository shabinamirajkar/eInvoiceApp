using eInvoiceApplication.DomainModel;
using eInvoiceAutomationWeb.ViewModels;
using Kendo.Mvc.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using System.IO;
using SAPSourceMasterApplication.DomainModel;
using Newtonsoft.Json;
using System.Text;
using System.Data;
using System.ComponentModel;
using eInvoiceK2SAPBroker;
using eInvoice.K2Access;
using System.Web.UI;
using eInvoiceAutomationWeb.PDF;
using System.Security;
using System.Threading.Tasks;
using System.Net;


namespace eInvoiceAutomationWeb.Controllers
    {
    [OutputCache(Location = OutputCacheLocation.None)]
    [SessionTimeOutFilter]
    public class RoutingDetailsController : Controller
        {
        private static readonly log4net.ILog LogManager = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        RoutingDetailsViewModel routingDetailsViewModel = new RoutingDetailsViewModel();
        int invoiceMasterID;

        // IM --  
        // Method - First Method to be invoked when clicked on eInvoice menu
        // Method - returns the appropriate RoutingDetails partial view depending on the role
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult InvoiceDetails(string documentNo, string status, string SN, bool ReadOnly, bool ShowPODetails, int InvoiceMasterID = 0, string SharedUser = "")
            {
            try
                {
                LogManager.Debug("InvoiceDetails: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                    {
                    Session["LoadFromSAPClicked"] = false;
                    // For initial AP Review view
                    if (documentNo == "0")
                        {
                        if (ShowPODetails == false && Session["InvoiceMasterID"] == null)
                            {
                            routingDetailsViewModel.DocumentNo = "";
                            routingDetailsViewModel.SN = "";
                            routingDetailsViewModel.DocumentType = "";
                            LogManager.Debug("InvoiceDetails: END");
                            return PartialView("_RoutingDetailsMain", routingDetailsViewModel);
                            }
                        // AP Review switching tabs between Routing Details and PO Details
                        else
                            {
                            invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                            routingDetailsViewModel.DocumentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(invoiceMasterID);
                            LogManager.Debug("InvoiceDetails: END");
                            return RedirectToAction("RoutingDetailsAP", new { documentNo = routingDetailsViewModel.DocumentNo, InvoiceMasterID = invoiceMasterID });
                            }
                        }
                    else
                        {
                        if (TempData["DocumentNo"] != null)
                            routingDetailsViewModel.DocumentNo = TempData["DocumentNo"].ToString();
                        else
                            routingDetailsViewModel.DocumentNo = documentNo;

                        routingDetailsViewModel.SN = SN;
                        //For opening eInvoice from Dashboard
                        if (!String.IsNullOrEmpty(SN))
                            {
                            int SNIndex = SN.IndexOf('_');
                            string newProcId = SN.Substring(0, SNIndex);
                            invoiceMasterID = eInvoiceModelContext.GetInvoiceMasterIDFromProcId(Convert.ToInt32(newProcId));
                            Session["InvoiceMasterID"] = invoiceMasterID;
                            routingDetailsViewModel.DocumentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(invoiceMasterID);
                            routingDetailsViewModel.DocumentType = eInvoiceModelContext.GetDocumentTypeFromInvoiceMasterID(invoiceMasterID);
                            routingDetailsViewModel.NonContractingStatus = eInvoiceModelContext.GetNonContractingStatusFromInvoiceMasterID(invoiceMasterID);
                            routingDetailsViewModel.InvoiceType = eInvoiceModelContext.GetInvoiceTypeFromInvoiceMasterID(invoiceMasterID);
                            //List<string> checkValues = new List<string> { "KG", "KA", "KR", "RC", "KN", "KC", "RN", "RG", "RP" };
                            //if (checkValues.Contains(routingDetailsViewModel.DocumentType))
                            //    routingDetailsViewModel.CheckNonContractingInvoice = true;
                            }
                        // For opening Saved eInvoice from Dashboard and also while switching between 2 tabs in AP Review
                        if (String.IsNullOrEmpty(SN) && status == "AP Review" && ReadOnly == false)
                            {
                            // Session["InvoiceMasterID"] = eInvoiceModelContext.GetInvoiceMasterID(routingDetailsViewModel.DocumentNo);
                            Session["InvoiceMasterID"] = InvoiceMasterID;
                            invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);

                            routingDetailsViewModel.DocumentType = eInvoiceModelContext.GetDocumentTypeFromInvoiceMasterID(invoiceMasterID);
                            routingDetailsViewModel.NonContractingStatus = eInvoiceModelContext.GetNonContractingStatusFromInvoiceMasterID(invoiceMasterID);
                            routingDetailsViewModel.InvoiceType = eInvoiceModelContext.GetInvoiceTypeFromInvoiceMasterID(invoiceMasterID);
                            //List<string> checkValues = new List<string> { "KG", "KA", "KR", "RC", "KN", "KC", "RN", "RG", "RP" };
                            //if (checkValues.Contains(routingDetailsViewModel.DocumentType))
                            //    routingDetailsViewModel.CheckNonContractingInvoice = true;

                            LogManager.Debug("InvoiceDetails: END");
                            return RedirectToAction("RoutingDetailsAP", new { documentNo = routingDetailsViewModel.DocumentNo, InvoiceMasterID = invoiceMasterID });
                            }
                        // For opening Report View from Dashboard
                        else if (ReadOnly == true && String.IsNullOrEmpty(SN))
                            {
                            Session["InvoiceMasterID"] = null;
                            LogManager.Debug("InvoiceDetails: END");
                            return RedirectToAction("RoutingDetailsReportView", new { documentNo = routingDetailsViewModel.DocumentNo, InvoiceMasterID = InvoiceMasterID });
                            }
                        // For all other Read Only views and FAP Review when opened from Dashboard
                        else
                            {
                            Session["InvoiceMasterID"] = null;
                            switch (status)
                                {
                                case "FAP Review":
                                    LogManager.Debug("InvoiceDetails: END");
                                    return RedirectToAction("RoutingDetailsFAP", new { documentNo = routingDetailsViewModel.DocumentNo, SN = SN, SharedUser = SharedUser });
                                default:
                                    LogManager.Debug("InvoiceDetails: END");
                                    return RedirectToAction("RoutingDetailsReadOnlyViews", new { documentNo = routingDetailsViewModel.DocumentNo, status = status, SN = SN, SharedUser = SharedUser });
                                }
                            }
                        }
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("InvoiceDetails: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        // IM --  
        // Method - This is invoked after AP/FAP enters DocumentNo and clicks on 'Load From SAP'
        // Method - returns data corresponding to the entered DocumentNo with all the grids - InvoiceDetails, Approvers, Comments and Attachments
        [HttpPost]
        public ActionResult InvoiceDetails(RoutingDetailsViewModel routingDetailsViewModel, string DocumentNo, string SN, string SharedUser = "")
            {
            try
                {
                LogManager.Debug("InvoiceDetails: START");
                if (String.IsNullOrEmpty(DocumentNo))
                    return Content("Document No cannot be empty");
                else
                    {
                    using (var eInvoiceModelContext = new eInvoiceModelContext())
                        {
                        string documentNo = DocumentNo.Trim();
                        TempData["DocumentNo"] = documentNo;
                        string userId = GetCurrentUserId();
                        int ID = 0;
                        Session["LoadFromSAPClicked"] = true;

                        if (Session["InvoiceMasterID"] != null)
                            {
                            ID = Convert.ToInt32(Session["InvoiceMasterID"]);
                            }
                        else if (!String.IsNullOrEmpty(SN))
                            {
                            int SNIndex = SN.IndexOf('_');
                            string newProcId = SN.Substring(0, SNIndex);
                            ID = eInvoiceModelContext.GetInvoiceMasterIDFromProcId(Convert.ToInt32(newProcId));
                            Session["InvoiceMasterID"] = ID.ToString();
                            }
                        else
                            {
                            ID = eInvoiceModelContext.GetInvoiceMasterIDFilterOnStatus(documentNo);
                            Session["InvoiceMasterID"] = ID.ToString();
                            }

                        if (ID == 0) // Doc No doesn't exist in InvoiceMaster table
                            {
                            eInvoiceLoadDocNoFromSAP.LoadDocNoFromSAP(documentNo, userId, " ", ID, "AP Review");
                            ID = eInvoiceModelContext.GetInvoiceMasterIDFilterOnStatus(documentNo);
                            if (ID != 0)
                                {
                                Session["InvoiceMasterID"] = ID.ToString();
                                eInvoiceModelContext.UpdateNewDocumentNoChanges(ID, HttpContext.User.Identity.Name.ToString(), Convert.ToBoolean(Session["LoadFromSAPClicked"]));
                                PopulateRoutingDetailsTab(routingDetailsViewModel, documentNo, userId, ID, SN);
                                LogManager.Debug("InvoiceDetails: END");
                                return PartialView("_RoutingDetails", routingDetailsViewModel);
                                }
                            else
                                {
                                Session["InvoiceMasterID"] = null;
                                LogManager.Debug("InvoiceDetails: END");
                                return Content("Document No is not valid");
                                }
                            }
                        else
                            {
                            string submittedUserId = eInvoiceModelContext.GetAPSubmittedByUserId(ID);
                            string currentStatus = eInvoiceModelContext.GetStatus(ID);
                            int currentDocMasterID = eInvoiceModelContext.GetInvoiceMasterIDFilterOnStatus(documentNo);

                            //if ((currentStatus == "AP Review" || currentStatus == "FAP Review") && 
                            //      (submittedUserId.ToLower() == userId.ToLower()) && 
                            //      (ID == currentDocMasterID || currentDocMasterID == 0))
                            if ((currentDocMasterID == 0 && currentStatus != "FAP Review") || ((currentStatus == "AP Review" || currentStatus == "FAP Review") &&
                                                                                               (submittedUserId.ToLower() == userId.ToLower()) &&
                                                                                               ((ID == currentDocMasterID) || (currentDocMasterID == 0))))
                                {
                                if (currentDocMasterID == 0 && String.IsNullOrEmpty(SN) && currentStatus != "AP Review")
                                    eInvoiceLoadDocNoFromSAP.LoadDocNoFromSAP(documentNo, userId, " ");
                                else
                                    eInvoiceLoadDocNoFromSAP.LoadDocNoFromSAP(documentNo, userId, " ", ID, currentStatus);

                                ID = eInvoiceModelContext.GetInvoiceMasterIDFilterOnStatus(documentNo);
                                if (ID != 0)
                                    {
                                    eInvoiceModelContext.UpdateNewDocumentNoChanges(ID, HttpContext.User.Identity.Name.ToString(), Convert.ToBoolean(Session["LoadFromSAPClicked"]));

                                    PopulateRoutingDetailsTab(routingDetailsViewModel, documentNo, userId, ID, SN);
                                    ViewBag.ShowPODetails = true;
                                    LogManager.Debug("InvoiceDetails: END");
                                    return PartialView("_RoutingDetails", routingDetailsViewModel);
                                    }
                                else
                                    {
                                    Session["InvoiceMasterID"] = null;
                                    LogManager.Debug("InvoiceDetails: END");
                                    return Content("Document No is not valid");
                                    }
                                }
                            else
                                {
                                Session["InvoiceMasterID"] = null;
                                TempData["InvalidDocMsg"] = "Please use another Document Number, a process may have already been started for this Document Number or Check your Dashboard for Saved Invoices.";
                                LogManager.Debug("InvoiceDetails: END");
                                return Content(TempData["InvalidDocMsg"].ToString());
                                }
                            }
                        }
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("InvoiceDetails: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        // IM --  
        // Method - This is invoked for all roles except 'AP' and 'FAP' 
        // Method - returns Routing Details views depending on the logged in Role
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult RoutingDetailsReadOnlyViews(string documentNo, string status, string SN, string SharedUser = "")
            {
            try
                {
                LogManager.Debug("RoutingDetailsReadOnlyViews: START");

                using (var eInvoiceModelContext = new eInvoiceModelContext())
                    {
                    // IM -- Get Master ID for Invoice from SN
                    int SNIndex = SN.IndexOf('_');
                    string newProcId = SN.Substring(0, SNIndex);
                    int ID = eInvoiceModelContext.GetInvoiceMasterIDFromProcId(Convert.ToInt32(newProcId));
                    Session["InvoiceMasterID"] = ID;
                    invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    if (TempData["Comment"] != null)
                        routingDetailsViewModel.Comment = TempData["Comment"].ToString();
                    routingDetailsViewModel.DocumentNo = documentNo;
                    routingDetailsViewModel.SN = SN;
                    routingDetailsViewModel.SharedUser = SharedUser;
                    routingDetailsViewModel.ShowRejectButton = false;
                    routingDetailsViewModel.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);
                    routingDetailsViewModel.ShowPOWarning = eInvoiceModelContext.POValidateEndValidityDate(invoiceMasterID);
                    SaveInvoiceDetailsToTemp(routingDetailsViewModel.InvoiceDetails);
                    ViewBag.Roles = eInvoiceModelContext.GetApproverRoleNames(invoiceMasterID);
                    ViewData["defaultRole"] = eInvoiceModelContext.GetApproverRoleNames(invoiceMasterID).Select(p => p.Role).FirstOrDefault();
                    ViewBag.MemoryApprovers = GetExchangeApprovers();
                    ViewBag.PONumbers = GetPONumbers(invoiceMasterID);
                    ViewBag.InvoiceStatusApproverRoles = eInvoiceModelContext.GetApproversListForLoggedRole(invoiceMasterID);
                    ViewBag.InvoiceStatusApproverRolesNonContracting = eInvoiceModelContext.GetApproversListForLoggedRole(invoiceMasterID, 1);
                    ViewBag.InvoiceStatusApproverRolesManual = eInvoiceModelContext.GetApproversListForLoggedRole(invoiceMasterID, 2);
                    switch (status)
                        {
                        case "PM Review":
                        case "TM Review":
                        case "AA Review":
                        case "DM Review":
                            LogManager.Debug("RoutingDetailsReadOnlyViews: END");
                            return PartialView("_RoutingDetailsEditableCommentsandAttachments", routingDetailsViewModel);
                        default:
                            LogManager.Debug("RoutingDetailsReadOnlyViews: END");
                            return PartialView("_RoutingDetailsHeaderReadOnly", routingDetailsViewModel);
                        }
                    }
                }

            catch (Exception ex)
                {
                LogManager.Error("RoutingDetailsReadOnlyViews: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        // IM -- 
        // Method - This is invoked for 'FAP' role
        // Method - returns RoutingDetails view same as for the AP Role
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult RoutingDetailsFAP(string documentNo, string SN, string SharedUser = "")
            {
            try
                {
                LogManager.Debug("RoutingDetailsFAP: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                    {
                    // IM -- Get Master ID for Invoice from SN
                    int SNIndex = SN.IndexOf('_');
                    string newProcId = SN.Substring(0, SNIndex);
                    int ID = eInvoiceModelContext.GetInvoiceMasterIDFromProcId(Convert.ToInt32(newProcId));
                    //int ID = eInvoiceModelContext.GetInvoiceMasterID(documentNo);
                    if (ID != 0)
                        {
                        Session["InvoiceMasterID"] = ID;
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                        if (TempData["Comment"] != null)
                            routingDetailsViewModel.Comment = TempData["Comment"].ToString();
                        routingDetailsViewModel.DocumentNo = documentNo;
                        routingDetailsViewModel.SN = SN;
                        routingDetailsViewModel.SharedUser = SharedUser;
                        routingDetailsViewModel.ActionText = "- select Approve/Reject/Route to -";
                        routingDetailsViewModel.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);
                        routingDetailsViewModel.ShowPOWarning = eInvoiceModelContext.POValidateEndValidityDate(invoiceMasterID);
                        SaveInvoiceDetailsToTemp(routingDetailsViewModel.InvoiceDetails);
                        ViewBag.Roles = eInvoiceModelContext.GetApproverRoleNames(invoiceMasterID);
                        ViewData["defaultRole"] = eInvoiceModelContext.GetApproverRoleNames(invoiceMasterID).Select(p => p.Role).FirstOrDefault();
                        ViewBag.MemoryApprovers = GetExchangeApprovers();
                        //ViewBag.MemoryApprovers = new List<ExchangeEmployeeProfile>();
                        ViewBag.PONumbers = GetPONumbers(invoiceMasterID);
                        ViewBag.InvoiceStatusApproverRoles = eInvoiceModelContext.GetApproversListForLoggedRole(invoiceMasterID);
                        ViewBag.InvoiceStatusApproverRolesNonContracting = eInvoiceModelContext.GetApproversListForLoggedRole(invoiceMasterID, 1);
                        ViewBag.InvoiceStatusApproverRolesManual = eInvoiceModelContext.GetApproversListForLoggedRole(invoiceMasterID, 2);
                        LogManager.Debug("RoutingDetailsFAP: END");
                        return PartialView("_RoutingDetailsMain", routingDetailsViewModel);
                        }
                    else
                        LogManager.Debug("RoutingDetailsFAP: END");
                    return Content("Document No is not valid");
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("RoutingDetailsFAP: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        // IM -- 
        // Method - This is invoked for saved 'AP' role
        // Method - returns RoutingDetails view saved data
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult RoutingDetailsAP(string documentNo, int InvoiceMasterID)
            {
            try
                {
                LogManager.Debug("RoutingDetailsAP: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                    {
                    // IM -- Get Master ID for Invoice when Doc No is passed
                    //int ID = eInvoiceModelContext.GetInvoiceMasterID(documentNo);
                    int ID = InvoiceMasterID;
                    if (ID != 0)
                        {
                        Session["InvoiceMasterID"] = ID;
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                        if (TempData["Comment"] != null)
                            routingDetailsViewModel.Comment = TempData["Comment"].ToString();
                        routingDetailsViewModel.DocumentNo = documentNo;
                        routingDetailsViewModel.ShowSaveButton = true;
                        routingDetailsViewModel.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);
                        routingDetailsViewModel.ShowPOWarning = eInvoiceModelContext.POValidateEndValidityDate(invoiceMasterID);
                        SaveInvoiceDetailsToTemp(routingDetailsViewModel.InvoiceDetails);
                        ViewBag.Roles = eInvoiceModelContext.GetApproverRoleNames(invoiceMasterID);
                        ViewData["defaultRole"] = eInvoiceModelContext.GetApproverRoleNames(invoiceMasterID).Select(p => p.Role).FirstOrDefault();
                        ViewBag.MemoryApprovers = GetExchangeApprovers();
                        // ViewBag.MemoryApprovers = new List<ExchangeEmployeeProfile>();
                        ViewBag.PONumbers = GetPONumbers(invoiceMasterID);
                        ViewBag.InvoiceStatusApproverRoles = eInvoiceModelContext.GetApproversListForLoggedRole(invoiceMasterID);
                        ViewBag.InvoiceStatusApproverRolesNonContracting = eInvoiceModelContext.GetApproversListForLoggedRole(invoiceMasterID, 1);
                        ViewBag.InvoiceStatusApproverRolesManual = eInvoiceModelContext.GetApproversListForLoggedRole(invoiceMasterID, 2);
                        ViewBag.ShowAllTabs = false;
                        ViewBag.ShowPODetails = true;
                        LogManager.Debug("RoutingDetailsAP: END");
                        return PartialView("_RoutingDetailsMain", routingDetailsViewModel);
                        }
                    else
                        LogManager.Debug("RoutingDetailsAP: END");
                    return Content("Document No is not valid");
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("RoutingDetailsAP: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        //Report View for Routing Details tab
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult RoutingDetailsReportView(string documentNo, int InvoiceMasterID)
            {
            try
                {
                LogManager.Debug("RoutingDetailsReportView: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                    {
                    if (InvoiceMasterID != 0)
                        {
                        invoiceMasterID = InvoiceMasterID;
                        Session["InvoiceMasterID"] = invoiceMasterID;
                        }
                    else
                        {
                        Session["InvoiceMasterID"] = eInvoiceModelContext.GetInvoiceMasterID(documentNo);
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                        }
                    routingDetailsViewModel.DocumentNo = documentNo;
                    routingDetailsViewModel.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);
                    routingDetailsViewModel.ShowPOWarning = eInvoiceModelContext.POValidateEndValidityDate(invoiceMasterID);
                    ViewBag.Roles = eInvoiceModelContext.GetApproverRoleNames(invoiceMasterID);
                    ViewData["defaultRole"] = eInvoiceModelContext.GetApproverRoleNames(invoiceMasterID).Select(p => p.Role).FirstOrDefault();
                    ViewBag.MemoryApprovers = GetExchangeApprovers();
                    ViewBag.PONumbers = GetPONumbers(invoiceMasterID);
                    ViewBag.InvoiceStatusApproverRoles = eInvoiceModelContext.GetApproversListForLoggedRole(invoiceMasterID);
                    
                    
                    LogManager.Debug("RoutingDetailsReportView: END");
                    return PartialView("_RoutingDetailsReadOnly", routingDetailsViewModel);
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("RoutingDetailsReportView: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }

        // IM -- 
        // Method - This is invoked from Approvers grid filter
        // Method - It returns the data for Role filter of Approvers grid
        public ActionResult BindRoleFilter()
            {
            try
                {
                LogManager.Debug("BindRoleFilter: START");
                using (eInvoiceModelContext context = new eInvoiceModelContext())
                    {
                    List<ConfigRole> roles = context.GetApproverRoleNames();
                    LogManager.Debug("BindRoleFilter: END");
                    return Json(roles.Distinct(), JsonRequestBehavior.AllowGet);
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("BindRoleFilter: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }

        // IM -- 
        // Method - This is invoked from Approvers grid Read event
        // Method - It returns the data to display records in Approvers Grid
        public ActionResult Approvers_Read([DataSourceRequest]DataSourceRequest request)
            {
            try
                {
                LogManager.Debug("Approvers_Read: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    routingDetailsViewModel.ConfigRoles = eInvoiceModelContext.GetApproverRoleNames(invoiceMasterID);
                    string status = eInvoiceModelContext.GetStatus(invoiceMasterID);

                    //if (Session["LoadFromSAPClicked"] != null && (status == "AP Review" || status == "FAP Review") && (Convert.ToBoolean(Session["LoadFromSAPClicked"]) == true))
                    // routingDetailsViewModel.InvoiceApproversList = eInvoiceModelContext.GetDestinationApproversList(invoiceMasterID, true);
                    //else
                    routingDetailsViewModel.InvoiceApproversList = eInvoiceModelContext.GetDestinationApproversList(invoiceMasterID, false);

                    DataSourceResult result = new DataSourceResult();
                    if (status == "TM Review" || status == "PM Review" || status == "DM Review" || status == "AA Review")
                        {
                        result = routingDetailsViewModel.InvoiceApproversList.ToDataSourceResult(request, p => new ApproversViewModel
                        {
                            InvoicePOApproverID = p.InvoicePOApproverID,
                            InvoiceMasterID = invoiceMasterID,
                            RoleName = p.Role,
                            PONumberField = p.PONumber.TrimStart('0'),
                            POLine = p.POLine,
                            ApproverUserId = (p.ApproverUserID == null ? String.Empty : p.ApproverUserID),
                            Approver = ((p.ApproverUserID == null || p.ApproverUserID == String.Empty) ? String.Empty : PDFHelper.FetchExchangeApprover(p.ApproverUserID)),
                            ApproverSuggestedBySAP = p.ApproverSuggestedbySAP
                        });
                        }
                    else
                        {

                        result = routingDetailsViewModel.InvoiceApproversList.ToDataSourceResult(request, p => new ApproversViewModel
                        {
                            InvoicePOApproverID = p.InvoicePOApproverID,
                            InvoiceMasterID = invoiceMasterID,
                            RoleName = p.Role,
                            PONumberField = p.PONumber,
                            POLine = p.POLine,
                            ApproverUserId = (p.ApproverUserID == null ? String.Empty : p.ApproverUserID),
                            Approver = ((p.ApproverUserID == null || p.ApproverUserID == String.Empty) ? String.Empty : PDFHelper.FetchExchangeApprover(p.ApproverUserID)),
                            ApproverSuggestedBySAP = p.ApproverSuggestedbySAP
                        });
                        }
                    LogManager.Debug("Approvers_Read: END");
                    return Json(result);
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("Approvers_Read: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        // IM -- 
        // Method - This is invoked from Approvers grid Create event
        // Method - Create new row in Approvers Grid
        [HttpPost]
        public ActionResult Approvers_Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ApproversViewModel> approvers)
            {
            try
                {
                LogManager.Debug("Approvers_Create: START");
                using (var eInvoiceModelcontext = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        {
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                        }

                    DataSourceResult dataSourceResult = new DataSourceResult();
                    //routingDetailsViewModel.ConfigRoles = eInvoiceModelcontext.GetApproverRoleNames(invoiceMasterID);
                    //ViewBag.Roles = routingDetailsViewModel.ConfigRoles;
                    //ViewBag.Roles = context.GetApproverRoleNames(invoiceMasterID);
                    //ViewData["defaultRole"] = context.GetApproverRoleNames(invoiceMasterID).Select(p => p.Role).FirstOrDefault();
                    List<InvoicePOApprover> approverslist = new List<InvoicePOApprover>();
                    List<InvoicePOApprover> result = new List<InvoicePOApprover>();
                    string invoiceType = TempData["InvoiceType"].ToString();

                    if (approvers != null && ModelState.IsValid)
                        {
                        ModelState.Clear();
                        foreach (var approver in approvers)
                            {
                            if (approver.PONumberField == " ")
                                approver.POLine = 0;
                            if (approver.RoleName != null)
                                {
                                if (invoiceType == "Manual")
                                    approver.RoleName = "Manual Approver";
                                else if (invoiceType == "NonContract")
                                    approver.RoleName = "Other Approver";
                                approverslist.Add(new InvoicePOApprover
                                {
                                    
                                    InvoiceMasterID = (approver.InvoiceMasterID == 0 ? invoiceMasterID : approver.InvoiceMasterID),
                                    InvoicePOApproverID = approver.InvoicePOApproverID,
                                    Role = approver.RoleName,
                                    PONumber = approver.PONumberField,
                                    POLine = approver.POLine,
                                    ApproverUserID = (approver.ApproverUserId == null ? String.Empty : approver.ApproverUserId),
                                    ApproverSuggestedbySAP = approver.ApproverSuggestedBySAP
                                });
                                TempData["InvoiceType"] = invoiceType;
                                }
                            else
                                approver.RoleName = String.Empty;
                            }
                        if (approverslist.Count > 0)
                            {
                            result = eInvoiceModelcontext.SaveInvoiceApproversList(approverslist);

                            foreach (var m in approvers)
                                {
                                //var approver = (from r in result
                                //                where r.InvoiceMasterID == invoiceMasterID && r.POLine == m.POLine && r.PONumber == m.PONumberField && r.Role == m.RoleName && r.ApproverUserID == (String.IsNullOrEmpty(m.ApproverUserId) ? string.Empty : m.ApproverUserId)
                                //                select r).FirstOrDefault();
                                m.InvoicePOApproverID = (from r in result select r.InvoicePOApproverID).FirstOrDefault();
                                if (m.ApproverUserId == null)
                                    m.ApproverUserId = string.Empty;
                                if (m.RoleName == null)
                                    m.RoleName = string.Empty;
                                }
                            }
                        dataSourceResult = approvers.ToDataSourceResult(request, ModelState);
                        }

                    LogManager.Debug("Approvers_Create: END");
                    return Json(dataSourceResult);
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("Approvers_Create: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }

            }

        // IM -- 
        // Method - This is invoked from Approvers grid Update event
        // Method - Updates Approvers Grid
        [HttpPost]
        public ActionResult Approvers_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ApproversViewModel> approvers)
            {
            try
                {
                LogManager.Debug("Approvers_Update: START");
                using (var eInvoiceModelcontext = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    List<InvoicePOApprover> approverslist = new List<InvoicePOApprover>();
                    if (approvers != null && ModelState.IsValid)
                        {
                        ModelState.Clear();
                        foreach (var approver in approvers)
                            {
                            if (approver.PONumberField == " ")
                                approver.POLine = 0;
                            approverslist.Add(new InvoicePOApprover
                            {
                                InvoiceMasterID = (approver.InvoiceMasterID == 0 ? invoiceMasterID : approver.InvoiceMasterID),
                                InvoicePOApproverID = approver.InvoicePOApproverID,
                                Role = approver.RoleName,
                                PONumber = approver.PONumberField,
                                POLine = approver.POLine,
                                ApproverUserID = ((String.IsNullOrEmpty(approver.ApproverUserId)) ? String.Empty : approver.ApproverUserId),
                                ApproverSuggestedbySAP = approver.ApproverSuggestedBySAP,
                            });
                            }
                        eInvoiceModelcontext.SaveInvoiceApproversList(approverslist);
                        }
                    foreach (ApproversViewModel vm in approvers)
                        {
                        if (vm.ApproverUserId == null)
                            vm.ApproverUserId = string.Empty;
                        if (vm.RoleName == null)
                            vm.RoleName = string.Empty;
                        }
                    LogManager.Debug("Approvers_Update: END");
                    return Json(approvers.ToDataSourceResult(request, ModelState));
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("Approvers_Update: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        // IM -- 
        // Method - This is invoked from Approvers grid Destroy event
        //Method - Deletes Approvers in Approvers Grid
        [HttpPost]
        public ActionResult Approvers_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ApproversViewModel> approvers)
            {
            try
                {
                LogManager.Debug("Approvers_Destroy: START");
                using (var eInvoiceModelcontext = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    List<InvoicePOApprover> approverslist = new List<InvoicePOApprover>();
                    if (approvers != null && ModelState.IsValid)
                        {
                        ModelState.Clear();
                        foreach (var approver in approvers)
                            {
                            approverslist.Add(new InvoicePOApprover
                            {
                                InvoiceMasterID = (approver.InvoiceMasterID == 0 ? invoiceMasterID : approver.InvoiceMasterID),
                                InvoicePOApproverID = approver.InvoicePOApproverID,
                                Role = approver.RoleName,
                                PONumber = approver.PONumberField,
                                POLine = approver.POLine,
                                ApproverUserID = ((String.IsNullOrEmpty(approver.ApproverUserId)) ? String.Empty : approver.ApproverUserId),
                                ApproverSuggestedbySAP = approver.ApproverSuggestedBySAP
                            });
                            }
                        eInvoiceModelcontext.DeleteInvoiceApprovers(approverslist);
                        }
                    foreach (ApproversViewModel vm in approvers)
                        {
                        if (vm.ApproverUserId == null)
                            vm.ApproverUserId = string.Empty;
                        if (vm.RoleName == null)
                            vm.RoleName = string.Empty;
                        }
                    LogManager.Debug("Approvers_Destroy: END");
                    return Json(approvers.ToDataSourceResult(request, ModelState));
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("Approvers_Destroy: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        [HttpPost]
        public ActionResult AP_Save([Bind(Prefix = "updated")]List<ApproversViewModel> updatedApprovers,
                                    [Bind(Prefix = "new")]List<ApproversViewModel> newApprovers,
                                    [Bind(Prefix = "deleted")]List<ApproversViewModel> deletedApprovers)
            {
            try
                {
                LogManager.Debug("AP_Save: START");
                SaveInvoiceApproversGrid(updatedApprovers, newApprovers, deletedApprovers);
                string userId = GetCurrentUserId();
                SaveInvoiceUserDetails(userId, DateTime.Now);
                //SaveInvoiceComments();
                LogManager.Debug("AP_Save: END");
                return Json("Success!");
                }
            catch (Exception ex)
                {
                LogManager.Error("AP_Save: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        [HttpPost]
        public ActionResult Approvers_CreateUpdateDelete([Bind(Prefix = "updated")]List<ApproversViewModel> updatedApprovers,
                                                         [Bind(Prefix = "new")]List<ApproversViewModel> newApprovers,
                                                         [Bind(Prefix = "deleted")]List<ApproversViewModel> deletedApprovers)
            {
            try
                {
                LogManager.Debug("Approvers_CreateUpdateDelete: START");
                SaveInvoiceApproversGrid(updatedApprovers, newApprovers, deletedApprovers);
                LogManager.Debug("Approvers_CreateUpdateDelete: END");
                return Json("Success!");
                }
            catch (Exception ex)
                {
                LogManager.Error("Approvers_CreateUpdateDelete: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        // IM -- 
        // Method - This is invoked from Attachments grid Read event
        //Method - Displays Attachments in Attachments Grid
        public ActionResult Attachments_Read([DataSourceRequest]DataSourceRequest request)
            {
            try
                {
                LogManager.Debug("Attachments_Read: START");

                using (var eInvoiceModelContext = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        {
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                        }
                    routingDetailsViewModel.InvoiceAttachments = eInvoiceModelContext.GetInvoiceAttachments(invoiceMasterID);
                    DataSourceResult result = new DataSourceResult();
                    result = routingDetailsViewModel.InvoiceAttachments.ToDataSourceResult(request, p => new AttachmentsViewModel
                    {
                        InvoiceAttachmentID = p.InvoiceAttachmentID,
                        InvoiceMasterID = p.InvoiceMasterID,
                        FileName = p.FileName,
                        UploadedUserID = p.UploadedUserID,
                        LoggedUserID = HttpContext.User.Identity.Name.ToString()
                    });
                    LogManager.Debug("Attachments_Read: END");
                    return Json(result);
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("Attachments_Read: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        // IM -- 
        // Method - This is invoked from Attachments grid Destroy event
        //Method - Delete Attachments in Attachments Grid
        [HttpPost]
        public ActionResult Attachments_Destroy([DataSourceRequest] DataSourceRequest request, AttachmentsViewModel model)
            {
            try
                {
                string invoiceNo, vendorNo, docNo;
                LogManager.Debug("Attachments_Destroy: START");
                using (var eInvoiceModelcontext = new eInvoiceModelContext())
                    {
                    if (model != null)
                        {
                        if (Session["InvoiceMasterID"] != null)
                            {
                            invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                            }

                        InvoiceAttachment attachment = new InvoiceAttachment();
                        attachment.InvoiceAttachmentID = model.InvoiceAttachmentID;
                        attachment.FileAttachment = model.FileAttachment;
                        attachment.UploadedUserID = model.UploadedUserID;
                        attachment.FileName = model.FileName;
                        eInvoiceModelcontext.DeleteInvoiceAttachment(attachment.InvoiceAttachmentID);

                        //Deleting the same attachment from Sharepoint
                        InvoiceMaster invoiceMasterData = eInvoiceModelcontext.GetRoutingDetailsHeader(invoiceMasterID);
                        invoiceNo = invoiceMasterData.InvoiceNo;
                        vendorNo = invoiceMasterData.VendorNo;
                        docNo = invoiceMasterData.DocumentNo;
                        PDFHelper.DeleteDocFromSharePoint(attachment.FileName, invoiceNo, vendorNo, docNo);
                        }
                    LogManager.Debug("Attachments_Destroy: END");
                    return Json(new[] { model }.ToDataSourceResult(request, ModelState));
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("Attachments_Destroy: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        // IM -- 
        // Method - This is invoked from Upload Control save event
        //Method - Save Attachments to Database


        [HttpPost]
        public JsonResult UploadAttachments()
            {
            try
                {
                foreach (string file in Request.Files)
                    {
                    var fileContent = Request.Files[file];
                    if (fileContent != null && fileContent.ContentLength > 0)
                        {
                        // get a stream
                        var stream = fileContent.InputStream;
                        // and optionally write the file to disk
                        var fileName = Path.GetFileName(file);
                        var path = Path.Combine(Server.MapPath("~/App_Data/Images"), fileName);
                        using (var fileStream = System.IO.File.Create(path))
                            {
                            stream.CopyTo(fileStream);
                            }
                        }
                    }
                }
            catch (Exception)
                {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
                }

            return Json("File uploaded successfully");
            }

        [HttpPost]
        // [AsyncTimeout(600000)]
        [NoAsyncTimeoutAttribute()]
        public ActionResult SaveAttachment(IEnumerable<HttpPostedFileBase> files)
            {
            try
                {
                string documentNo, invoiceNo, vendorNo, vendorName, poNumber, description;
                decimal? invoiceAmount;
                DateTime invoiceDate;
                LogManager.Debug("SaveAttachment: START");
                using (var context = new eInvoiceModelContext())
                    {
                    if (files != null)
                        {
                        foreach (var file in files)
                            {
                            if (Session["InvoiceMasterID"] != null)
                                {
                                invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                                }
                            else
                                {
                                //if (TempData["DocumentNo"] != null)
                                //{
                                //    documentNo = Convert.ToString(TempData["DocumentNo"]);
                                //    invoiceMasterID = context.GetInvoiceMasterIDFilterOnStatus(documentNo);
                                //}
                                }

                            using (var memoryStream = new MemoryStream())
                                {
                                file.InputStream.CopyTo(memoryStream);
                                var uploadAttachment = new InvoiceAttachment
                                {
                                    FileAttachment = memoryStream.ToArray(),
                                    InvoiceMasterID = invoiceMasterID,
                                    FileName = Path.GetFileName(file.FileName),
                                    UploadedUserID = HttpContext.User.Identity.Name
                                };
                                var result = context.SaveInvoiceAttachment(uploadAttachment);
                                uploadAttachment.InvoiceAttachmentID = result;

                                var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), Path.GetFileName(file.FileName));
                                file.SaveAs(path);

                                // uploading the attachment to SharePoint after saving it to DataBase
                                //FileStream fileStream = new FileStream(file.FileName, FileMode.Open);
                                FileStream fileStream = new FileStream(path, FileMode.Open);
                                InvoiceMaster invoiceMasterData = context.GetRoutingDetailsHeader(invoiceMasterID);
                                documentNo = invoiceMasterData.DocumentNo;
                                invoiceNo = invoiceMasterData.InvoiceNo;
                                vendorNo = invoiceMasterData.VendorNo;
                                vendorName = invoiceMasterData.VendorName;
                                poNumber = invoiceMasterData.ContractNo;
                                invoiceDate = invoiceMasterData.InvoiceDate;
                                invoiceAmount = invoiceMasterData.InvoiceAmount;
                                description = invoiceMasterData.Period;
                                
                                string fileName = Path.GetFileName(file.FileName);

                                PDFHelper.UploadDocToSharePoint(fileStream, fileName, documentNo, invoiceNo, vendorNo, vendorName, poNumber, invoiceDate,
                                    invoiceAmount, description, false);

                                // 08/31/2015 - Post Prod Changes
                                string completePath = Path.Combine(Server.MapPath("~/App_Data/uploads"), Path.GetFileName(file.FileName));
                                if (System.IO.File.Exists(completePath))
                                    {
                                    System.IO.File.Delete(completePath);
                                    }

                                fileStream.Close();
                                }
                            }
                        }
                    }
                LogManager.Debug("SaveAttachment: END");
                return Json(new { data = "" }, "text/plain");
                }
            catch (Exception ex)
                {
                LogManager.Error("SaveAttachment: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        // IM -- 
        // Method - This is invoked on Uploading an Attachment
        // Method - returns the uploaded FileStream
        public ActionResult DownloadAttachment(int attachmentID)
            {
            try
                {
                LogManager.Debug("DownloadAttachment: START");

                using (var context = new eInvoiceModelContext())
                    {
                    InvoiceAttachment attachment;
                    //var result = context.DisplayAttachment(attachmentID);
                    //string fileName = result.FileName;
                    //int index = fileName.IndexOf('.');
                    //string contentType = fileName.Substring(index);
                    //var stream = new MemoryStream(result.FileAttachment);
                    //LogManager.Debug("DownloadAttachment: END");
                    // return new FileStreamResult(stream, contentType);

                    attachment = context.DisplayAttachment(attachmentID);
                    LogManager.Debug("DownloadAttachment: END");
                    return File(attachment.FileAttachment, "application/force-download", Path.GetFileName(attachment.FileName));
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("DownloadAttachment: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        // IM -- 
        // Method - This is invoked from Read Event of Comments Grid
        //Method - Display Comments in Comments grid
        public ActionResult Comments_Read([DataSourceRequest]DataSourceRequest request)
            {
            try
                {
                LogManager.Debug("Comments_Read: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    routingDetailsViewModel.InvoiceComments = eInvoiceModelContext.GetInvoiceComments(invoiceMasterID);
                    DataSourceResult result = new DataSourceResult();
                    result = routingDetailsViewModel.InvoiceComments.ToDataSourceResult(request, p => new CommentsViewModel
                    {
                        InvoiceCommentID = p.InvoiceCommentID,
                        InvoiceMasterID = p.InvoiceMasterID,
                        Comment = p.Comment,
                        CommentBy = p.CommentBy,
                        CommentDate = p.CommentDate.ToString("MM/dd/yyyy hh:mm tt"),
                        //  CommentDate = p.CommentDate.Date.ToShortDateString(),
                    });
                    LogManager.Debug("Comments_Read: END");
                    return Json(result);
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("Comments_Read: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        // IM -- 
        // Method - This is invoked from Submit button click of Routing Details tab
        //Method -  Validates all the tabs and sets the ShowValidations flag
        public ActionResult Validate(int RouteTo, bool isReadOnly, string DocumentNo, string SN)
            {
            try
                {
                LogManager.Debug("Validate: START");
                List<TempErrorMessage> validations = new List<TempErrorMessage>();
                using (var context = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    else
                        {
                        if (!String.IsNullOrEmpty(SN))
                            {
                            // IM -- Get Master ID for Invoice from SN
                            int SNIndex = SN.IndexOf('_');
                            string newProcId = SN.Substring(0, SNIndex);
                            invoiceMasterID = context.GetInvoiceMasterIDFromProcId(Convert.ToInt32(newProcId));
                            Session["InvoiceMasterID"] = invoiceMasterID;
                            }
                        else
                            {
                            // Session variable - InvoiceMasterID Recovery
                            // invoiceMasterID = context.GetInvoiceMasterID(DocumentNo);
                            invoiceMasterID = context.GetInvoiceMasterIDFilterOnStatus(DocumentNo);
                            Session["InvoiceMasterID"] = invoiceMasterID;
                            }
                        }

                    validations = context.ValidateRoutingDetails(invoiceMasterID, RouteTo, isReadOnly, "Submit");
                    if (validations.Count > 0)
                        {
                        TempData["Message"] = validations;
                        ViewBag.ShowValidations = true;
                        }
                    else
                        ViewBag.ShowValidations = false;
                    LogManager.Debug("Validate: END");

                    return Json(ViewBag.ShowValidations, JsonRequestBehavior.AllowGet);
                    }
                }

            catch (Exception ex)
                {
                LogManager.Error("Validate: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }

        // IM -- 
        // Method - This is invoked from Submit button click of Routing Details tab
        //Method -  Displays validation messages
        public ActionResult DisplayValidations()
            {
            try
                {
                LogManager.Debug("DisplayValidations: START");

                List<TempErrorMessage> errorMsgs = new List<TempErrorMessage>();
                if (TempData["Message"] != null)
                    errorMsgs = TempData["Message"] as List<TempErrorMessage>;
                LogManager.Debug("DisplayValidations: END");
                return PartialView("_ValidationSummary", errorMsgs);
                }
            catch (Exception ex)
                {
                LogManager.Error("DisplayValidations: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        // IM -- 
        // Method - This is invoked  when Submit is clicked on Routing Details tab and there are no validation errors
        //Method - Saves data from all the tabs; starts workflow if status is 'AP Review'; else invokes 'ActionWorklistItem' method with action name as 'Submit'
        public ActionResult Submit(int RouteTo, bool isReadOnly, string serialNo, string documentNo, string Comment, string SharedUser = "")
            {
            try
                {
                LogManager.Debug("Submit: START");
                string invoiceNo = string.Empty;
                using (var context = new eInvoiceModelContext())
                    {
                    TempData["Comment"] = Comment;
                    if (Comment != "")
                        {
                        SaveInvoiceComments();
                        }
                    TempData["Comment"] = null;

                    string userId = GetCurrentUserId();

                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    else
                        {
                        if (!String.IsNullOrEmpty(serialNo))
                            {
                            // IM -- Get Master ID for Invoice from SN
                            int SNIndex = serialNo.IndexOf('_');
                            string newProcId = serialNo.Substring(0, SNIndex);
                            invoiceMasterID = context.GetInvoiceMasterIDFromProcId(Convert.ToInt32(newProcId));
                            Session["InvoiceMasterID"] = invoiceMasterID;
                            }
                        else
                            {
                            // Session variable - InvoiceMasterID Recovery
                            // invoiceMasterID = context.GetInvoiceMasterID(documentNo);
                            invoiceMasterID = context.GetInvoiceMasterIDFilterOnStatus(documentNo);
                            Session["InvoiceMasterID"] = invoiceMasterID;
                            }
                        }

                    string currentStatus = context.GetStatus(invoiceMasterID);

                    //Populate InvoicePODetailChanges table with InvoicePODetail table to initialize it;
                    //Populate InvoiceShortPayLetter table to initialize few fields like AddressedTo,SentFrom,Date etc
                    //Populate InvoiceCATTFindings table to initialize few fields like AddressedTo,SentFrom,Date etc
                    //Inserting 'Submit' Action into 'InvoiceRoutingRecord' table when status is 'AP Review'
                    if (currentStatus.Equals("AP Review"))
                        {
                        SaveInvoiceUserDetails(userId, DateTime.Now);
                        InitializeInvoiceTabs();
                        int roleId = context.GetRoleIdForAP(userId);
                        context.InsertRoutingRecord(invoiceMasterID, roleId, userId);
                        // If AP Review
                        if (TempData["InvoiceNo"] != null)
                            invoiceNo = TempData["InvoiceNo"].ToString();
                        K2Service.StarteInvoiceRequest(invoiceMasterID, invoiceNo, RouteTo, userId);
                        }
                    else
                        {
                        string actionName = context.GetActionNameForCurrentRole(invoiceMasterID, "Submit");
                        //invokes ActionWorklistItem for all roles other than 'AP'
                        K2Service.ActionWorklistItem(serialNo, invoiceMasterID, RouteTo, actionName, userId, SharedUser);
                        }
                    LogManager.Debug("Submit: END");
                    return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("Submit: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }

        // IM -- 
        // Method - This is invoked from Approve button click of Routing Details tab
        //Method -  Validates all the tabs and sets the ShowValidations flag
        public ActionResult ValidateApproveClick(string documentNo, bool isReadOnly, string SN)
            {
            try
                {
                LogManager.Debug("ValidateApproveClick: START");
                int num1;
                List<TempErrorMessage> validations = new List<TempErrorMessage>();
                using (var context = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    else
                        {  // IM -- Get Master ID for Invoice from SN
                        if (!String.IsNullOrEmpty(SN))
                            {
                            int SNIndex = SN.IndexOf('_');
                            string newProcId = SN.Substring(0, SNIndex);
                            invoiceMasterID = context.GetInvoiceMasterIDFromProcId(Convert.ToInt32(newProcId));
                            Session["InvoiceMasterID"] = invoiceMasterID;
                            }
                        else
                            {
                            // Session variable - InvoiceMasterID Recovery
                            // invoiceMasterID = context.GetInvoiceMasterID(documentNo);
                            invoiceMasterID = context.GetInvoiceMasterIDFilterOnStatus(documentNo);
                            Session["InvoiceMasterID"] = invoiceMasterID;
                            }
                        }

                    string currentStatus = context.GetStatus(invoiceMasterID);

                    // Depending on Status - Role, determine the RouteToRoleID
                    // If PC, need additional logic to determine if TM or FAP
                    String ApproverRouteToRoleId = context.GetApproverRoleId(currentStatus, invoiceMasterID);

                    bool res = int.TryParse(ApproverRouteToRoleId, out num1);
                    if (num1 != 0)
                        {
                        // Validations - same as Submit, but RouteToRole is Approver RoleId
                        validations = context.ValidateRoutingDetails(invoiceMasterID, Convert.ToInt32(ApproverRouteToRoleId), isReadOnly, "Approve");
                        if (validations.Count > 0)
                            {
                            TempData["Message"] = validations;
                            ViewBag.ShowValidations = true;
                            }
                        else
                            ViewBag.ShowValidations = false;
                        LogManager.Debug("ValidateApproveClick: END");
                        return Json(ViewBag.ShowValidations, JsonRequestBehavior.AllowGet);
                        }
                    else
                        LogManager.Debug("ValidateApproveClick: END");
                    return Json(ApproverRouteToRoleId, JsonRequestBehavior.AllowGet);
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("ValidateApproveClick: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }

        // IM -- 
        // Method - This is invoked from Approve button click after all the validations are passed and no errors
        //Method -  This invokes the ActionWorkListItem for all users with action name as 'Approve'
        public ActionResult Approve(string serialNo, string documentNo, string Comment, string SharedUser)
            {
            try
                {
                LogManager.Debug("Approve: START");
                using (var context = new eInvoiceModelContext())
                    {
                    string result, routeToApprover = "";
                    TempData["Comment"] = Comment;
                    if (Comment != "")
                        {
                        SaveInvoiceComments();
                        }
                    TempData["Comment"] = null;

                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    else
                        {  // IM -- Get Master ID for Invoice from SN
                        if (!String.IsNullOrEmpty(serialNo))
                            {
                            int SNIndex = serialNo.IndexOf('_');
                            string newProcId = serialNo.Substring(0, SNIndex);
                            invoiceMasterID = context.GetInvoiceMasterIDFromProcId(Convert.ToInt32(newProcId));
                            Session["InvoiceMasterID"] = invoiceMasterID;
                            }
                        else
                            {
                            // Session variable - InvoiceMasterID Recovery
                            // invoiceMasterID = context.GetInvoiceMasterID(documentNo);
                            invoiceMasterID = context.GetInvoiceMasterIDFilterOnStatus(documentNo);
                            Session["InvoiceMasterID"] = invoiceMasterID;
                            }
                        }

                    string currentStatus = context.GetStatus(invoiceMasterID);
                    string ApproverRouteToRoleId = context.GetApproverRoleId(currentStatus, invoiceMasterID);
                    string vendorNo;
                    decimal invoiceAmount = context.GetInvoiceAmount(invoiceMasterID);
                    decimal configuredDMAmount = context.GetConfiguredDMAmount();
                    using (var eInvoiceModelContext = new eInvoiceModelContext())
                        {
                        InvoiceMaster invoiceMasterData = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);
                        vendorNo = invoiceMasterData.VendorNo.TrimStart('0');
                        }
                    if ((currentStatus == "FAP Review" && ApproverRouteToRoleId == "8" && (invoiceAmount < configuredDMAmount))
                        || (currentStatus == "DM Review"))
                        {
                        string docStatus = string.Empty;
                        result = eInvoiceLoadDocNoFromSAP.ValidateDocNoWithSAP(documentNo, out docStatus);
                        if (docStatus.ToLower() == "v")
                            routeToApprover = "Cannot complete approval process for a Parked document";
                        else
                            {
                            if (result.ToLower() == "free for payment")
                                {
                                //if (PDFHelper.CheckDocLibExists(vendorNo))
                                    //{
                                    routeToApprover = InvokeActionWorklistItem(serialNo, documentNo, "Approve", SharedUser);
                                    System.Threading.Thread.Sleep(8000);
                                    if (String.IsNullOrEmpty(routeToApprover))
                                        return PartialView("Error");
                                    PDFHelper.SavePDF(invoiceMasterID, this);
                                    //}
                                //else
                                    //routeToApprover = "Please create Vendor Document Library in SharePoint before you Approve this Invoice";
                                }
                            else
                                {
                                routeToApprover = "Please release the hold in SAP before you Approve this Invoice";
                                }
                            }
                        }
                    else
                        {
                        routeToApprover = InvokeActionWorklistItem(serialNo, documentNo, "Approve", SharedUser);
                        if (String.IsNullOrEmpty(routeToApprover))
                            return PartialView("Error");
                        }
                    LogManager.Debug("Approve: END");
                    return Json(routeToApprover, JsonRequestBehavior.AllowGet);
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("Approve: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        public ActionResult FixUploadIssue()
            {
            try
                {
                LogManager.Debug("FixUploadIssue: START");

                using (var eInvoiceModelContext = new eInvoiceModelContext())
                    {
                    List<InvoiceMaster> invoiceMasterList = new List<InvoiceMaster>();

                    invoiceMasterList = eInvoiceModelContext.GetInvoiceMasterIDForUploadFix();
                    foreach (var invoiceMaster in invoiceMasterList)
                        {
                        int invMasterID = 0;
                        string userApprover = "";
                        invMasterID = invoiceMaster.InvoiceMasterID;
                        Session["InvoiceMasterID"] = invMasterID;
                        userApprover = invoiceMaster.APSubmittedByUserID;
                        InitializeInvoiceTabs(userApprover);
                        PDFHelper.SavePDF(invMasterID, this);
                        }
                    LogManager.Debug("FixUploadIssue: END");

                    return PartialView("~/views/Home/Home.cshtml");
                    }

                }
            catch (Exception ex)
                {
                LogManager.Error("Approve: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }

        // IM -- 
        // Method - This is invoked from Reject button click for FAP Role
        //Method -  This invokes the ActionWorkListItem for FAP when 'Reject' is clicked
        public ActionResult Reject(string serialNo, string documentNo, string Comment, string SharedUser)
            {
            try
                {
                LogManager.Debug("Reject: START");
                using (var context = new eInvoiceModelContext())
                    {
                    TempData["Comment"] = Comment;
                    if (Comment != "")
                        {
                        SaveInvoiceComments();
                        }
                    TempData["Comment"] = null;

                    InvokeActionWorklistItem(serialNo, documentNo, "Reject", SharedUser);
                    LogManager.Debug("Reject: END");
                    TempData["Comment"] = null;
                    return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("Reject: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }

        //07/01/2015 - Post UAT Changes - Build 8
        public ActionResult ViewSharePointInvoices(string documentNo, string vendorNo, string invoiceNo)
            {
            vendorNo = vendorNo.TrimStart('0');
            string SPUrl = System.Configuration.ConfigurationManager.AppSettings["SharePointSiteURL"].ToString();
            string SPFolder = System.Configuration.ConfigurationManager.AppSettings["SharePointFolder"].ToString();
            string docLibNameURL = System.Configuration.ConfigurationManager.AppSettings["SharePointDocLibraryURLName"].ToString();
            string docFolderName = documentNo;
            String sharePointUrl = SPUrl + "/" + docLibNameURL + "/" + vendorNo + "/" + docFolderName;
            return Redirect(sharePointUrl);
            }

        public ActionResult RoutingDetailsUploadFile()
            {
            try
                {
                LogManager.Debug("RoutingDetailsUploadFile");
                return View("RoutingDetailsUploadFile");
                }
            catch (Exception ex)
                {
                LogManager.Error("RoutingDetailsUploadFile: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }

        public ActionResult UploadInvoiceToSharePoint()
            {
            try
                {
                LogManager.Debug("RoutingDetailsUploadInvoiceToSharePoint");
                return View("RoutingDetailsUploadInvoiceToSharePoint");
                }
            catch (Exception ex)
                {
                LogManager.Error("RoutingDetailsUploadInvoiceToSharePoint: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }

        [HttpPost]
        [NoAsyncTimeoutAttribute()]
        public ActionResult SaveInvoiceToSharePoint(IEnumerable<HttpPostedFileBase> files)
            {
            try
                {
                string documentNo, invoiceNo, vendorNo, vendorName, poNumber, description;
                decimal? invoiceAmount;
                DateTime invoiceDate;
                LogManager.Debug("SaveInvoiceToSharePoint: START");
                using (var context = new eInvoiceModelContext())
                    {
                    if (files != null)
                        {
                        foreach (var file in files)
                            {
                            using (var memoryStream = new MemoryStream())
                                {
                                file.InputStream.CopyTo(memoryStream);
                                if (Session["InvoiceMasterID"] != null)
                                    {
                                    invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                                    }
                                var path = Path.Combine(Server.MapPath("~/App_Data/SharePointuploads"), Path.GetFileName(file.FileName));
                                file.SaveAs(path);

                                FileStream fileStream = new FileStream(path, FileMode.Open);
                                InvoiceMaster invoiceMasterData = context.GetRoutingDetailsHeader(invoiceMasterID);
                                documentNo = invoiceMasterData.DocumentNo;
                                invoiceNo = invoiceMasterData.InvoiceNo;
                                vendorNo = invoiceMasterData.VendorNo;
                                vendorName = invoiceMasterData.VendorName;
                                poNumber = invoiceMasterData.ContractNo;
                                invoiceDate = invoiceMasterData.InvoiceDate;
                                invoiceAmount = invoiceMasterData.InvoiceAmount;
                                description = invoiceMasterData.Period;

                                string fileName = Path.GetFileName(file.FileName);

                                bool uploadStatus = PDFHelper.UploadDocToSharePoint(fileStream, fileName, documentNo, invoiceNo, vendorNo, vendorName, poNumber,
                                    invoiceDate, invoiceAmount, description, false);

                                // 08/31/2015 - Post Prod Changes
                                string completePath = Path.Combine(Server.MapPath("~/App_Data/SharePointuploads"), Path.GetFileName(file.FileName));
                                if (System.IO.File.Exists(completePath))
                                    {
                                    System.IO.File.Delete(completePath);
                                    }

                                fileStream.Close();
                                }
                            }
                        }
                    }
                LogManager.Debug("SaveInvoiceToSharePoint: END");
                return Json(new { data = "" }, "text/plain");
                }
            catch (Exception ex)
                {
                LogManager.Error("SaveInvoiceToSharePoint: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }

        #region Public/Private Methods

        private void PopulateRoutingDetailsTab(RoutingDetailsViewModel routingDetailsViewModel, string documentNo, string userId, int ID, string SN)
            {
            try
                {
                LogManager.Debug("PopulateRoutingDetailsTab: START");
                using (eInvoiceModelContext eInvoiceModelContext = new eInvoiceModelContext())
                    {
                    Session["InvoiceMasterID"] = ID;
                    invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    if (TempData["Comment"] != null)
                        routingDetailsViewModel.Comment = TempData["Comment"].ToString();
                    routingDetailsViewModel.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);
                    routingDetailsViewModel.ShowPOWarning = eInvoiceModelContext.POValidateEndValidityDate(invoiceMasterID);
                    routingDetailsViewModel.DocumentType = eInvoiceModelContext.GetDocumentTypeFromInvoiceMasterID(invoiceMasterID);
                    //List<string> checkValues = new List<string> { "KG", "KA", "KR", "RC", "KN", "KC", "RN", "RG", "RP" };
                    //if (checkValues.Contains(routingDetailsViewModel.DocumentType))
                    //    routingDetailsViewModel.CheckNonContractingInvoice = true;


                    SaveInvoiceDetailsToTemp(routingDetailsViewModel.InvoiceDetails);
                    ViewBag.Roles = eInvoiceModelContext.GetApproverRoleNames(invoiceMasterID);
                    ViewData["defaultRole"] = eInvoiceModelContext.GetApproverRoleNames(invoiceMasterID).Select(p => p.Role).FirstOrDefault();
                    ViewBag.MemoryApprovers = GetExchangeApprovers();
                    ViewBag.PONumbers = GetPONumbers(invoiceMasterID);
                    ViewBag.InvoiceStatusApproverRoles = eInvoiceModelContext.GetApproversListForLoggedRole(invoiceMasterID);
                    ViewBag.InvoiceStatusApproverRolesNonContracting = eInvoiceModelContext.GetApproversListForLoggedRole(invoiceMasterID, 1);
                    ViewBag.InvoiceStatusApproverRolesManual = eInvoiceModelContext.GetApproversListForLoggedRole(invoiceMasterID, 2);
                    // GetCATTThresholdsAjax();
                    ViewBag.ShowAllTabs = false;
                    ViewBag.ShowPODetails = true;
                    routingDetailsViewModel.DocumentNo = documentNo;
                    routingDetailsViewModel.SN = SN;
                    string currentStatus = eInvoiceModelContext.GetStatus(ID);
                    if (currentStatus == "AP Review")
                        {
                        routingDetailsViewModel.ShowSaveButton = true;
                        }
                    else
                        {
                        routingDetailsViewModel.ShowSaveButton = false;
                        }
                    }
                LogManager.Debug("PopulateRoutingDetailsTab: END");
                }

            catch (Exception ex)
                {
                LogManager.Error("PopulateRoutingDetailsTab: ERROR " + ex.Message, ex);
                }
            }


        private string InvokeActionWorklistItem(string serialNo, string documentNo, string action, string SharedUser)
            {
            try
                {
                LogManager.Debug("InvokeActionWorklistItem: START");
                using (var context = new eInvoiceModelContext())
                    {
                    int validApproverRoleId;
                    // IM -- Get Master ID for Invoice from SN
                    int SNIndex = serialNo.IndexOf('_');
                    string newProcId = serialNo.Substring(0, SNIndex);

                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    else
                        invoiceMasterID = context.GetInvoiceMasterIDFromProcId(Convert.ToInt32(newProcId));

                    string currentStatus = context.GetStatus(invoiceMasterID);
                    string ApproverRouteToRoleId = context.GetApproverRoleId(currentStatus, invoiceMasterID);
                    string actionName = context.GetActionNameForCurrentRole(invoiceMasterID, action);
                    string userId = GetCurrentUserId();
                    bool res = int.TryParse(ApproverRouteToRoleId, out validApproverRoleId);
                    //if ((num1 != 0 || currentStatus == "DM Review") && !(currentStatus == "FAP Review" && ApproverRouteToRoleId == "10"))
                    if (validApproverRoleId != 0 || currentStatus == "DM Review")
                        {
                        if (action == "Approve")
                            {
                            //invokes ActionWorklistItem for 'FAP' role, as 'Reject' button will be hidden for others
                            K2Service.ActionWorklistItem(serialNo, invoiceMasterID, Convert.ToInt32(ApproverRouteToRoleId), actionName, userId, SharedUser);
                            Session["InvoiceMasterID"] = null;
                            }
                        }
                    if (action == "Reject")
                        {
                        ApproverRouteToRoleId = "0";
                        K2Service.ActionWorklistItem(serialNo, invoiceMasterID, Convert.ToInt32(ApproverRouteToRoleId), actionName, userId, SharedUser);
                        Session["InvoiceMasterID"] = null;
                        }

                    LogManager.Debug("InvokeActionWorklistItem: END");
                    return ApproverRouteToRoleId;
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("InvokeActionWorklistItem: ERROR " + ex.Message, ex);
                return null;
                }
            }


        private string GetCurrentUserId()
            {
            try
                {
                LogManager.Debug("GetCurrentUserId: START");

                string userId = string.Empty;
                string loggedInUserId = HttpContext.User.Identity.Name;
                if (loggedInUserId.Contains('\\'))
                    {
                    int index = loggedInUserId.IndexOf('\\');
                    userId = loggedInUserId.Substring(index + 1);
                    }
                else
                    userId = loggedInUserId;
                LogManager.Debug("GetCurrentUserId: END");
                return userId;
                }
            catch (Exception ex)
                {
                LogManager.Error("GetCurrentUserId: ERROR " + ex.Message, ex);
                return null;
                }
            }

        private void SaveInvoiceApproversGrid(List<ApproversViewModel> updatedApprovers, List<ApproversViewModel> newApprovers, List<ApproversViewModel> deletedApprovers)
            {
            try
                {
                LogManager.Debug("SaveInvoiceApproversGrid: START");
                UpdateApproversGrid(updatedApprovers);
                CreateRowInApproversGrid(newApprovers);
                DeleteFromApproversGrid(deletedApprovers);
                LogManager.Debug("SaveInvoiceApproversGrid: END");
                }
            catch (Exception ex)
                {
                LogManager.Error("SaveInvoiceApproversGrid: ERROR " + ex.Message, ex);
                }
            }

        private void SaveInvoiceUserDetails(string userId, DateTime submittedDate)
            {
            try
                {
                LogManager.Debug("SaveInvoiceUserDetails: START");
                using (var context = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    var details = context.SaveInvoiceDetails(invoiceMasterID, userId, submittedDate);
                    }
                LogManager.Debug("SaveInvoiceUserDetails: END");
                }
            catch (Exception ex)
                {
                LogManager.Error("SaveInvoiceUserDetails: ERROR " + ex.Message, ex);
                }
            }


        private void SaveInvoiceComments()
            {
            try
                {
                LogManager.Debug("SaveInvoiceComments: START");
                string comment = String.Empty;
                using (var context = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);

                    if (TempData["Comment"] != null)
                        {
                        comment = TempData["Comment"].ToString();
                        var invoiceComment = new InvoiceComment
                        {
                            InvoiceMasterID = invoiceMasterID,
                            CommentDate = DateTime.Now,
                            Comment = comment,
                            CommentBy = HttpContext.User.Identity.Name
                        };
                        var result = context.SaveInvoiceComment(invoiceComment);
                        }
                    }
                LogManager.Debug("SaveInvoiceComments: END");
                }

            catch (Exception ex)
                {
                LogManager.Error("SaveInvoiceComments: ERROR " + ex.Message, ex);
                }
            }

        private void InitializeInvoiceTabs(string userApprover = "")
            {
            try
                {
                LogManager.Debug("InitializeInvoiceTabs: START");

                using (var context = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    if (String.IsNullOrEmpty(userApprover))
                        userApprover = HttpContext.User.Identity.Name.ToString();
                    context.InitializeInvoiceTabs(invoiceMasterID, userApprover);
                    }
                LogManager.Debug("InitializeInvoiceTabs: END");
                }
            catch (Exception ex)
                {
                LogManager.Error("InitializeInvoiceTabs: ERROR " + ex.Message, ex);
                }
            }

        private void UpdateApproversGrid(List<ApproversViewModel> updatedApprovers)
            {
            try
                {
                LogManager.Debug("UpdateApproversGrid: START");
                if (updatedApprovers != null && updatedApprovers.Count > 0)
                    {
                    using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                        {
                        List<InvoicePOApprover> approverslist = new List<InvoicePOApprover>();
                        foreach (var approver in updatedApprovers)
                            {
                            approverslist.Add(new InvoicePOApprover
                            {
                                InvoiceMasterID = approver.InvoiceMasterID,
                                InvoicePOApproverID = approver.InvoicePOApproverID,
                                Role = approver.RoleName,
                                //RoleID = approver.RoleId,
                                PONumber = approver.PONumberField,
                                POLine = approver.POLine,
                                ApproverUserID = ((String.IsNullOrEmpty(approver.ApproverUserId)) ? String.Empty : approver.ApproverUserId.Split(',').First()),
                                // ApproverUserID = (approver.ApproverUserId == null ? String.Empty : approver.ApproverUserId),
                                ApproverSuggestedbySAP = approver.ApproverSuggestedBySAP
                            });
                            }
                        eInvoiceModelcontext.SaveInvoiceApproversList(approverslist);
                        }
                    }
                LogManager.Debug("UpdateApproversGrid: END");
                }

            catch (Exception ex)
                {
                LogManager.Error("UpdateApproversGrid: ERROR " + ex.Message, ex);
                }
            }


        private void CreateRowInApproversGrid(List<ApproversViewModel> newApprovers)
            {
            try
                {
                LogManager.Debug("CreateRowInApproversGrid: START");
                if (newApprovers != null && newApprovers.Count > 0)
                    {
                    using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                        {
                        List<InvoicePOApprover> approverslist = new List<InvoicePOApprover>();
                        if (Session["InvoiceMasterID"] != null)
                            invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);

                        foreach (var approver in newApprovers)
                            {
                            approverslist.Add(new InvoicePOApprover
                            {
                                InvoiceMasterID = (approver.InvoiceMasterID == 0 ? invoiceMasterID : approver.InvoiceMasterID),
                                InvoicePOApproverID = approver.InvoicePOApproverID,
                                Role = approver.RoleName,
                                // RoleID = approver.RoleId,
                                PONumber = approver.PONumberField,
                                POLine = approver.POLine,
                                ApproverUserID = ((String.IsNullOrEmpty(approver.ApproverUserId)) ? String.Empty : approver.ApproverUserId.Split(',').First()),
                                // ApproverUserID = (approver.ApproverUserId == null ? String.Empty : approver.ApproverUserId),
                                ApproverSuggestedbySAP = approver.ApproverSuggestedBySAP
                            });
                            }
                        var result = eInvoiceModelcontext.SaveInvoiceApproversList(approverslist);
                        foreach (var m in newApprovers)
                            {
                            var approver = (from r in result
                                            where r.POLine == m.POLine && r.PONumber == m.PONumberField && r.Role == m.RoleName && r.ApproverUserID == ((String.IsNullOrEmpty(m.ApproverUserId)) ? String.Empty : m.ApproverUserId.Split(',').First())
                                            select r).FirstOrDefault();
                            m.InvoicePOApproverID = approver.InvoicePOApproverID;
                            if (m.ApproverUserId == null)
                                m.ApproverUserId = string.Empty;
                            }
                        }
                    }
                LogManager.Debug("CreateRowInApproversGrid: END");
                }

            catch (Exception ex)
                {
                LogManager.Error("CreateRowInApproversGrid: ERROR " + ex.Message, ex);
                }
            }


        private void DeleteFromApproversGrid(List<ApproversViewModel> deletedApprovers)
            {
            try
                {
                LogManager.Debug("DeleteFromApproversGrid: START");

                if (deletedApprovers != null && deletedApprovers.Count > 0)
                    {
                    using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                        {
                        List<InvoicePOApprover> approverslist = new List<InvoicePOApprover>();
                        foreach (var approver in deletedApprovers)
                            {
                            approverslist.Add(new InvoicePOApprover
                            {
                                InvoiceMasterID = approver.InvoiceMasterID,
                                InvoicePOApproverID = approver.InvoicePOApproverID,
                                Role = approver.RoleName,
                                // RoleID = approver.RoleId,
                                PONumber = approver.PONumberField,
                                POLine = approver.POLine,
                                ApproverUserID = ((String.IsNullOrEmpty(approver.ApproverUserId)) ? String.Empty : approver.ApproverUserId.Split(',').First()),
                                //ApproverUserID = (approver.ApproverUserId == null ? String.Empty : approver.ApproverUserId),
                                ApproverSuggestedbySAP = approver.ApproverSuggestedBySAP
                            });
                            }
                        eInvoiceModelcontext.DeleteInvoiceApprovers(approverslist);
                        foreach (ApproversViewModel vm in deletedApprovers)
                            {
                            if (vm.ApproverUserId == null)
                                vm.ApproverUserId = string.Empty;
                            }
                        }
                    }
                LogManager.Debug("DeleteFromApproversGrid: END");
                }

            catch (Exception ex)
                {
                LogManager.Error("DeleteFromApproversGrid: ERROR " + ex.Message, ex);
                }
            }


        // IM -- 
        // Method - This is invo0ked from Save Event of Comments
        // Method - Saves it to TempData
        public JsonResult SaveComment(String Comment)
            {
            try
                {
                LogManager.Debug("SaveComment: START");
                routingDetailsViewModel.Comment = Comment;
                TempData["Comment"] = Comment;
                //  SaveInvoiceComments();
                LogManager.Debug("SaveComment: END");
                return Json(Comment, JsonRequestBehavior.AllowGet);
                }
            catch (Exception ex)
                {
                LogManager.Error("SaveComment: ERROR " + ex.Message, ex);
                return null;
                }
            }



        // IM -- 
        // Method - This is invoked from Approvers Grid
        //Method - Populates POLines in Approvers grid in Edit Mode
        public JsonResult GetPOLines(string PONumber)
            {
            try
                {
                LogManager.Debug("GetPOLines: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                    {
                    List<InvoicePODetail> poDetails = new List<InvoicePODetail>();
                    List<int?> POLines = new List<int?>();
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    poDetails = eInvoiceModelContext.GetPONumbersFromInvoicePODetail(invoiceMasterID);
                    //if (PONumber != " ")
                    //{
                    POLines = (from p in poDetails where p.PONumber == PONumber select p.POLine).Distinct().ToList<int?>();
                    LogManager.Debug("GetPOLines: END");
                    return Json(POLines, JsonRequestBehavior.AllowGet);
                    //}
                    //else
                    //{
                    //    POLines.Add(0);
                    //    LogManager.Debug("GetPOLines: END");
                    //    return Json(POLines, JsonRequestBehavior.AllowGet);
                    //}
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("GetPOLines: ERROR " + ex.Message, ex);
                return null;
                }
            }


        // IM -- 
        // Method - This is invoked from  Approvers grid
        //Method - populates ApproverUserID in Approvers grid in Edit mode
        private List<ExchangeEmployeeProfile> GetExchangeApprovers()
            {
            try
                {
                LogManager.Debug("GetExchangeApprovers: START");
                using (var SAPSourceModelContext = new SAPSourceModelContext())
                    {
                    List<ExchangeEmployeeProfile> approversList = SAPSourceModelContext.FetchExchangeEmployeesList();
                    LogManager.Debug("GetExchangeApprovers: END");
                    return approversList;
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("GetExchangeApprovers: ERROR " + ex.Message, ex);
                return null;
                }
            }


        public JsonResult GetSAPExchangeEmployees(string approver)
            {
            try
                {
                LogManager.Debug("GetSAPExchangeEmployees: START");

                List<ExchangeEmployeeProfile> exchangeEmployees = null;
                using (var SAPSourceModelContext = new SAPSourceModelContext())
                    {
                    if (!string.IsNullOrEmpty(approver))
                        {
                        exchangeEmployees = SAPSourceModelContext.GetSAPExchangeEmployeeFilter(approver);
                        ViewBag.MemoryApprovers = exchangeEmployees;
                        }
                    else
                        {
                        ExchangeEmployeeProfile exchangeEmployee = new ExchangeEmployeeProfile();
                        exchangeEmployee.FirstName = "";
                        exchangeEmployee.LastName = "";
                        exchangeEmployee.UserID = "";

                        List<ExchangeEmployeeProfile> emptyList = new List<ExchangeEmployeeProfile>();
                        emptyList.Add(exchangeEmployee);
                        exchangeEmployees = emptyList;
                        ViewBag.MemoryApprovers = exchangeEmployees;
                        }
                    }
                LogManager.Debug("GetSAPExchangeEmployees: END");
                return Json(exchangeEmployees, JsonRequestBehavior.AllowGet);
                }
            catch (Exception ex)
                {
                LogManager.Error("GetSAPExchangeEmployees: ERROR " + ex.Message, ex);
                return null;
                }
            }


        // IM -- 
        // Method - This is invoked while editing Invoice Details - Project
        //Method - Saves Invoice Details Edited Project field to TempData
        public JsonResult SaveProject(string Project)
            {
            try
                {
                LogManager.Debug("SaveProject: START");
                using (eInvoiceModelContext context = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    TempData["Project"] = Project;
                    context.SaveInvoiceProject(invoiceMasterID, Project);
                    }
                LogManager.Debug("SaveProject: END");
                return Json(Project, JsonRequestBehavior.AllowGet);
                }

            catch (Exception ex)
                {
                LogManager.Error("SaveProject: ERROR " + ex.Message, ex);
                return null;
                }
            }

        // IM -- 
        // Method - This is invoked while editing Invoice Details - Period
        //Method - Saves Invoice Details Edited Period field to TempData
        public JsonResult SavePeriod(string Period)
            {
            try
                {
                LogManager.Debug("SavePeriod: START");
                using (eInvoiceModelContext context = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    TempData["Period"] = Period;
                    context.SaveInvoicePeriod(invoiceMasterID, Period);
                    }
                LogManager.Debug("SavePeriod: END");
                return Json(Period, JsonRequestBehavior.AllowGet);
                }
            catch (Exception ex)
                {
                LogManager.Error("SavePeriod: ERROR " + ex.Message, ex);
                return null;
                }
            }

        // IM -- 
        // Method - This is invoked  while editing Invoice Details - PaymentDueBy
        //Method - Saves Invoice Details Edited PaymentDueBy field to TempData
        public JsonResult SavePaymentDueBy(string PaymentDueBy)
            {
            try
                {
                LogManager.Debug("SavePaymentDueBy: START");
                using (eInvoiceModelContext context = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    if (PaymentDueBy != null)
                        {
                        DateTime paymentDate = DateTime.Parse(PaymentDueBy);
                        TempData["PaymentDueBy"] = paymentDate;
                        context.SaveInvoicePaymentDueBy(invoiceMasterID, paymentDate);
                        }
                    }
                LogManager.Debug("SavePaymentDueBy: END");
                return Json(PaymentDueBy, JsonRequestBehavior.AllowGet);
                }
            catch (Exception ex)
                {
                LogManager.Error("SavePaymentDueBy: ERROR " + ex.Message, ex);
                return null;
                }
            }

        // IM -- 
        // Method - This is invoked  while editing Invoice Details - CATTThreshold
        //Method - Saves Invoice Details Edited CATTThreshold field to TempData
        public JsonResult SaveCATTThreshold(string CATTThreshold)
            {
            try
                {
                LogManager.Debug("SaveCATTThreshold: START");
                using (eInvoiceModelContext context = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    TempData["CATTThreshold"] = CATTThreshold;
                    context.SaveInvoiceCATTThreshold(invoiceMasterID, Convert.ToInt32(CATTThreshold));
                    }
                LogManager.Debug("SaveCATTThreshold: END");
                return Json(CATTThreshold, JsonRequestBehavior.AllowGet);
                }

            catch (Exception ex)
                {
                LogManager.Error("SaveCATTThreshold: ERROR " + ex.Message, ex);
                return null;
                }
            }


        // IM -- 
        // Method - This is invoked  while editing Invoice Details - CATTApproval
        //Method - Saves Invoice Details Edited CATTApproval field to TempData
        public JsonResult SaveCATTApproval(bool CATTApproval)
            {
            try
                {
                LogManager.Debug("SaveCATTApproval: START");
                using (eInvoiceModelContext context = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    TempData["CATTApproval"] = CATTApproval;
                    context.SaveInvoiceCATTApproval(invoiceMasterID, Convert.ToBoolean(CATTApproval));
                    }
                LogManager.Debug("SaveCATTApproval: END");
                return Json(CATTApproval, JsonRequestBehavior.AllowGet);
                }
            catch (Exception ex)
                {
                LogManager.Error("SaveCATTApproval: ERROR " + ex.Message, ex);
                return null;
                }
            }


        // SM - 01/13/2016 - PlaceHolder 
        // Method - This is invoked  while editing Invoice Details - NonContractingStatus
        //Method - Saves Invoice Details Edited CATTApproval field to TempData
        public JsonResult SaveNonContractingStatus(bool NonContractingStatus)
            {
            try
                {
                LogManager.Debug("SwitchNonContractingInvoice: START");
                using (eInvoiceModelContext context = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    TempData["NonContractingStatus"] = NonContractingStatus;
                    // SM -- Load data for Roles DD
                    context.SaveInvoiceNonContractingStatus(invoiceMasterID, Convert.ToBoolean(NonContractingStatus));
                    //ViewBag.InvoiceStatusApproverRoles = context.GetApproversListForLoggedRole(invoiceMasterID);
                    
                    }
                LogManager.Debug("SwitchNonContractingInvoice: END");
                return Json(NonContractingStatus, JsonRequestBehavior.AllowGet);
                }
            catch (Exception ex)
                {
                LogManager.Error("SwitchNonContractingInvoice: ERROR " + ex.Message, ex);
                return null;
                }
            }

        public JsonResult SaveInvoiceType(string InvoiceType)
            {
            try
                {
                LogManager.Debug("SaveInvoiceType: START");
                using (eInvoiceModelContext context = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    TempData["InvoiceType"] = InvoiceType;
                    // SM -- Load data for Roles DD
                    context.SaveInvoiceType(invoiceMasterID, InvoiceType);
                    //ViewBag.InvoiceStatusApproverRoles = context.GetApproversListForLoggedRole(invoiceMasterID);
                    //ViewBag.Roles = context.GetApproverRoleNames(invoiceMasterID);
                    //ViewData["defaultRole"] = context.GetApproverRoleNames(invoiceMasterID).Select(p => p.Role).FirstOrDefault();
                    
                   
                    }
                LogManager.Debug("SaveInvoiceType: END");
                return Json(InvoiceType, JsonRequestBehavior.AllowGet);
                }
            catch (Exception ex)
                {
                LogManager.Error("SaveInvoiceType: ERROR " + ex.Message, ex);
                return null;
                }
            }

        // IM -- 
        // Method - This is invoked from Approvers Grid
        //Method - Populates POLines in Approvers grid in Edit Mode
        //public JsonResult GetRoleDDAgain()
        //    {
        //    try
        //        {
        //        LogManager.Debug("GetPOLines: START");
        //        using (var eInvoiceModelContext = new eInvoiceModelContext())
        //            {
        //            List<InvoicePODetail> poDetails = new List<InvoicePODetail>();
        //            List<int?> POLines = new List<int?>();
        //            if (Session["InvoiceMasterID"] != null)
        //                invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
        //            poDetails = eInvoiceModelContext.GetPONumbersFromInvoicePODetail(invoiceMasterID);
                    
        //            POLines = (from p in poDetails where p.PONumber == PONumber select p.POLine).Distinct().ToList<int?>();
        //            LogManager.Debug("GetPOLines: END");
        //            return Json(POLines, JsonRequestBehavior.AllowGet);
                  
        //            }
        //        }
        //    catch (Exception ex)
        //        {
        //        LogManager.Error("GetPOLines: ERROR " + ex.Message, ex);
        //        return null;
        //        }
        //    }

        public JsonResult SaveContractNo(string ContractNo)
            {
            try
                {
                LogManager.Debug("SaveContractNo: START");
                using (eInvoiceModelContext context = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    TempData["ContractNo"] = ContractNo;
                    context.SaveInvoiceContractNo(invoiceMasterID, ContractNo);
                    }
                LogManager.Debug("SaveContractNo: END");
                return Json(ContractNo, JsonRequestBehavior.AllowGet);
                }
            catch (Exception ex)
                {
                LogManager.Error("SaveContractNo: ERROR " + ex.Message, ex);
                return null;
                }
            }

        // IM -- 
        // Method - This is invoked after fetching Invoice Details header data from DB
        //Method - Saves Invoice Details to TempData
        private void SaveInvoiceDetailsToTemp(InvoiceMaster invoiceDetails)
            {
            try
                {
                LogManager.Debug("CATTFindingsEmp_Read: START");
                TempData["Period"] = invoiceDetails.Period;
                TempData["Project"] = invoiceDetails.Project;
                TempData["PaymentDueBy"] = invoiceDetails.PaymentDueBy;
                TempData["CATTThreshold"] = invoiceDetails.CATTThreshold;
                TempData["CATTApproval"] = invoiceDetails.CATTApprovalRequired;
                TempData["InvoiceNo"] = invoiceDetails.InvoiceNo;
                TempData["NonContractingInvoice"] = invoiceDetails.NonContractingStatus;
                TempData["InvoiceType"] = invoiceDetails.InvoiceType;
                LogManager.Debug("SaveInvoiceDetailsToTemp: END");
                }
            catch (Exception ex)
                {
                LogManager.Error("SaveInvoiceDetailsToTemp: ERROR " + ex.Message, ex);
                }
            }

        // IM -- 
        // Method - This is invoked from Invoice Details header
        //Method - Populates CATTThreshold dropdown
        private JsonResult GetCATTThresholdsAjax()
            {
            try
                {
                LogManager.Debug("GetCATTThresholdsAjax: START");

                var items = new List<SelectListItem>() 
            { 
              new SelectListItem {  Text="$50K",Value="1" }, 
              new SelectListItem {  Text="$100K",Value="2" }, 
              new SelectListItem {  Text="$110K",Value="3" }
            };
                LogManager.Debug("GetCATTThresholdsAjax: END");
                return Json(items, JsonRequestBehavior.AllowGet);
                }
            catch (Exception ex)
                {
                LogManager.Error("GetCATTThresholdsAjax: ERROR " + ex.Message, ex);
                return null;
                }
            }

        // IM -- 
        // Method - This is invoked from Approvers grid
        //Method - Populates PONumbers 
        public List<InvoicePODetail> GetPONumbers(int invoiceMasterID)
            {
            try
                {
                using (eInvoiceModelContext context = new eInvoiceModelContext())
                    {
                    LogManager.Debug("GetPONumbers: START");
                    List<InvoicePODetail> PONumbers = new List<InvoicePODetail>();
                    List<InvoicePODetail> PODetails = new List<InvoicePODetail>();
                    PODetails = context.GetPONumbersFromInvoicePODetail(invoiceMasterID);
                    InvoicePODetail empty = new InvoicePODetail();
                    empty.PONumber = " ";
                    PONumbers.Add(empty);
                    foreach (InvoicePODetail poDetail in PODetails)
                        {
                        if (poDetail.PONumber != null)
                            {
                            var pro = (from p in PONumbers where p.PONumber == poDetail.PONumber select p).FirstOrDefault();
                            if (pro == null)
                                PONumbers.Add(poDetail);
                            }
                        }
                    ViewData["defaultPONumber"] = PONumbers.Select(p => p.PONumber).FirstOrDefault();
                    LogManager.Debug("GetPONumbers: END");
                    return PONumbers;
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("GetPONumbers: ERROR " + ex.Message, ex);
                return null;
                }
            }


        // IM -- 
        // Method - This is invoked while assigning RoleName from RoleId
        //Method - Fetches Roles
        private string FetchRoleName(int roleId)
            {
            try
                {
                LogManager.Debug("FetchRoleName: START");

                string roleName = string.Empty;
                List<ConfigRole> configuredRoles = routingDetailsViewModel.ConfigRoles;
                if (configuredRoles != null && configuredRoles.Count > 0)
                    roleName = (from role in configuredRoles where role.RoleID == roleId select role.Role).FirstOrDefault();
                LogManager.Debug("FetchRoleName: END");

                return roleName;
                }
            catch (Exception ex)
                {
                LogManager.Error("FetchRoleName: ERROR " + ex.Message, ex);
                return null;
                }
            }

        //private string FetchApprover(string approverUserID)
        //{
        //    try
        //    {
        //        LogManager.Debug("FetchApprover: START");
        //        List<SAPEmployeeProfile> approversList;
        //        string concatenatedName = string.Empty;
        //        using (var SAPSourceModelContext = new SAPSourceModelContext())
        //        {
        //            approversList = SAPSourceModelContext.FetchApproversList();
        //        }
        //        if (approversList != null && approversList.Count > 0)
        //            concatenatedName = (from approver in approversList where approver.UserID.ToLower() == approverUserID.ToLower() select approver.Concatenated).FirstOrDefault();
        //        LogManager.Debug("FetchApprover: END");
        //        return concatenatedName;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error("FetchApprover: ERROR " + ex.Message, ex);
        //        return null;
        //    }
        //}




        #endregion

        //#region PDFFileSaving

        // public void SavePDF(int invoiceMasterID)
        // {
        //    // int invoiceMasterID;
        //     string documentNo, invoiceNo, vendorNo, vendorName, poNumber;
        //     PDFDocumentViewModel PDFDocumentViewModel;

        //     try
        //     {
        //         LogManager.Debug("SavePDF: START");

        //         using (var eInvoiceModelContext = new eInvoiceModelContext())
        //         {
        //             PDFDocumentViewModel = new PDFDocumentViewModel();
        //             documentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(invoiceMasterID);
        //             // invoiceMasterID = eInvoiceModelContext.GetInvoiceMasterID(documentNo);

        //             //Getting Grid Totals for display at Grid Footer..
        //             PDFDocumentViewModel.InvoiceGridTotalsforPDF = new InvoiceGridTotalsforPDF();
        //             PDFDocumentViewModel.InvoiceGridTotalsforPDF = eInvoiceModelContext.GetInvoiceGridTotalsforPDF(invoiceMasterID);

        //             //Get eInvoice Approvers...
        //             PDFDocumentViewModel.eInvoiceApprovers = new List<eInvoiceApprovers>();
        //             PDFDocumentViewModel.eInvoiceApprovers = eInvoiceModelContext.GeteInvoiceApprovers(invoiceMasterID);

        //             //Get Routing Details Data..
        //             PDFDocumentViewModel.ApproversViewModel = new List<ApproversViewModel>();
        //             PDFDocumentViewModel.ApproversViewModel = GetRoutingApprovers(eInvoiceModelContext.GetDestinationApproversList(invoiceMasterID, false));
        //             PDFDocumentViewModel.CommentsViewModel = new List<CommentsViewModel>();
        //             PDFDocumentViewModel.CommentsViewModel = GetRoutingComments(eInvoiceModelContext.GetInvoiceComments(invoiceMasterID));
        //             PDFDocumentViewModel.AttachmentsViewModel = new List<AttachmentsViewModel>();
        //             PDFDocumentViewModel.AttachmentsViewModel = GetRoutingAttachments(eInvoiceModelContext.GetInvoiceAttachments(invoiceMasterID));

        //             //Get PODetail Data...
        //             PDFDocumentViewModel.ModifyAccountingCostCodesViewModel = GetPOModifyAccountingCostCodes(eInvoiceModelContext.GetInvoicePODetailChanges(invoiceMasterID));

        //             if (PDFDocumentViewModel.ModifyAccountingCostCodesViewModel != null)
        //             {
        //                 //Add Grid Total..
        //                 PDFDocumentViewModel.ModifyAccountingCostCodesViewModel.Add(new ModifyAccountingCostCodesViewModel
        //                 {
        //                     SAPPONumber = "Total:",
        //                     InvoiceAmount = PDFDocumentViewModel.InvoiceGridTotalsforPDF.POInvoiceAmt.Value,
        //                 });
        //             }

        //             PDFDocumentViewModel.AccountingCostCodesViewModel = GetAccountingCostCodes(eInvoiceModelContext.GetInvoicePODetails(invoiceMasterID));

        //             if (PDFDocumentViewModel.AccountingCostCodesViewModel != null)
        //             {
        //                 //Add Grid Total..
        //                 PDFDocumentViewModel.AccountingCostCodesViewModel.Add(new AccountingCostCodesViewModel
        //                 {
        //                     PONumber = "Total:",
        //                     POLine = null,
        //                     InvoiceAmount = PDFDocumentViewModel.InvoiceGridTotalsforPDF.POInvoiceAmtReadOnly.Value,
        //                 });
        //             }

        //             //Get Short Pay Data..
        //             PDFDocumentViewModel.ShortPayIndexViewModel = new ShortPayIndexViewModel();
        //             PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay = eInvoiceModelContext.GetShortPayDetails(invoiceMasterID);

        //             if (PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay == null)
        //             {
        //                 PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay = new InvoiceShortPayLetter();
        //             }

        //             if (PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay != null)
        //             {
        //                 //Get CA for InvoiceMasterID, RoleID of CA = 3
        //                 PDFDocumentViewModel.ShortPayIndexViewModel.SentFrom = eInvoiceModelContext.GetCATTFindingsApprover(PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay.SentFrom);
        //                 PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay.SentFrom = BuildEmployeeCSV(PDFDocumentViewModel.ShortPayIndexViewModel.SentFrom);
        //             }

        //             //Get CATT Findings Data..
        //             string LoggedUserCATTorCA = string.Empty;
        //             PDFDocumentViewModel.CATTFindingsViewModel = new CATTFindingsViewModel();
        //             PDFDocumentViewModel.CATTFindingsViewModel.DocumentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(invoiceMasterID);
        //             PDFDocumentViewModel.CATTFindingsViewModel.RoutingDetails = new RoutingDetailsViewModel();
        //             //Get Routing Header
        //             PDFDocumentViewModel.CATTFindingsViewModel.RoutingDetails.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);

        //             //Get InvoiceCATTFindings..
        //             PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings = eInvoiceModelContext.GetInvoiceCATTFindings(invoiceMasterID);
        //             if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings == null)
        //             {
        //                 PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings = new InvoiceCATTFindings();
        //             }

        //             //Get CA for InvoiceMasterID, RoleID of CA=3
        //             PDFDocumentViewModel.CATTFindingsViewModel.ToCA = eInvoiceModelContext.GetCATTFindingsApprover(PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.AddressedTo);
        //             //Get CATT for InvoiceMasterID, RoleID of CATT=2
        //             PDFDocumentViewModel.CATTFindingsViewModel.FromCATT = eInvoiceModelContext.GetCATTFindingsApprover(PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.SentFrom);
        //             PDFDocumentViewModel.CATTFindingsViewModel.DateSubmit = PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.Date;

        //             decimal? invoiceAmt = PDFDocumentViewModel.CATTFindingsViewModel.RoutingDetails.InvoiceDetails.InvoiceAmount;

        //             if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment.HasValue)
        //                 PDFDocumentViewModel.CATTFindingsViewModel.AssetPayment = invoiceAmt - PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment.Value;
        //             else
        //             {
        //                 PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment = 0;
        //                 PDFDocumentViewModel.CATTFindingsViewModel.AssetPayment = invoiceAmt - 0;
        //             }
        //             if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment.HasValue)
        //                 PDFDocumentViewModel.CATTFindingsViewModel.ApprovedPayment = invoiceAmt - PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment.Value;
        //             else
        //             {
        //                 PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment = 0;
        //                 PDFDocumentViewModel.CATTFindingsViewModel.ApprovedPayment = invoiceAmt - 0;
        //             }
        //             PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindingsEmp = eInvoiceModelContext.GetInvoiceCATTFindingsEmp(invoiceMasterID);

        //             if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindingsEmp != null)
        //             {
        //                 //Manually Adding Totals in Grid Last row.
        //                 PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindingsEmp.Add(
        //                     new InvoiceCATTFindingsEmp
        //                     {
        //                         EmployeeName = "Total:",
        //                         RateVariance = PDFDocumentViewModel.InvoiceGridTotalsforPDF.CATTRateVariance.Value,
        //                         Total = PDFDocumentViewModel.InvoiceGridTotalsforPDF.CATTTotal.Value,
        //                     });
        //             }

        //             PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCAFindingsEmp = eInvoiceModelContext.GetInvoiceCAFindingsEmp(invoiceMasterID);

        //             if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCAFindingsEmp != null)
        //             {
        //                 //Manually Adding Totals in Grid Last row.
        //                 PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCAFindingsEmp.Add(
        //                     new InvoiceCAFindingsEmp
        //                     {
        //                         EmployeeName = "Total:",
        //                         RateVariance = PDFDocumentViewModel.InvoiceGridTotalsforPDF.CARateVariance.Value,
        //                         Total = PDFDocumentViewModel.InvoiceGridTotalsforPDF.CATotal.Value,
        //                     });
        //             }

        //             PDFDocumentViewModel.CATTFindingsViewModel.ToCACSV = BuildEmployeeCSV(PDFDocumentViewModel.CATTFindingsViewModel.ToCA);
        //             PDFDocumentViewModel.CATTFindingsViewModel.FromCATTCSV = BuildEmployeeCSV(PDFDocumentViewModel.CATTFindingsViewModel.FromCATT);

        //             SaveViewAsPDFFile("", "~/Views/eInvoicePDF/_GeneratePDF.cshtml", "eInvoice-" + documentNo + ".pdf", PDFDocumentViewModel);

        //             // ShortPayPDF - Short Pay letter should be uploaded as PDF to SharePoint only if there is CA Adjustment amount
        //             // Return a true flag, if PDF should be uploaded
        //             bool uploadShortPayPDF = false;

        //             if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings != null)
        //                 {
        //                 decimal? approvedPaymentAmt = PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment;
        //                 if (approvedPaymentAmt != null && approvedPaymentAmt != Decimal.Zero)
        //                     uploadShortPayPDF = true;
        //                 }

        //             if (uploadShortPayPDF)
        //                 SaveShortPayPDF(invoiceMasterID);

        //             LogManager.Debug("SavePDF: END");

        //             // 06/29/2015 - Post UAT Changes - In Progress - Build 6
        //             LogManager.Debug("Upload to Sharepoint: START");
        //             // Get Data to update SharePoint Document Upload Metadata
        //             InvoiceMaster invoiceMasterData = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);
        //             invoiceNo = invoiceMasterData.InvoiceNo;
        //             vendorNo = invoiceMasterData.VendorNo;
        //             vendorName = invoiceMasterData.VendorName;
        //             poNumber = invoiceMasterData.ContractNo;

        //             PDFHelper.UploadDocToSharePoint(documentNo, invoiceNo, vendorNo, vendorName, poNumber, uploadShortPayPDF);
        //             LogManager.Debug("Upload to Sharepoint: END");

        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         LogManager.Error("SavePDF: ERROR " + ex.Message, ex);
        //     }
        // }

        // public void SaveShortPayPDF(int invoiceMasterID)
        //     {
        //     string documentNo = "";
        //     //if (string.IsNullOrEmpty(documentNo)) { throw new Exception("Document No. cannot be empty"); }

        //     ShortPayIndexViewModel shortPayIndexViewModel = new ShortPayIndexViewModel();
        //     try
        //         {
        //         LogManager.Debug("GenerateShortPayPDF: START");
        //         using (var eInvoiceModelContext = new eInvoiceModelContext())
        //             {
        //             //invoiceMasterID = eInvoiceModelContext.GetInvoiceMasterID(documentNo);
        //             documentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(invoiceMasterID);
        //             if (invoiceMasterID == 0) { throw new Exception("Document No. is not valid"); }

        //             shortPayIndexViewModel.ShortPay = eInvoiceModelContext.GetShortPayDetails(invoiceMasterID);

        //              //in case CA is not avilable during PDF Generation..
        //             if (shortPayIndexViewModel.SentFrom != null)
        //             {
        //                 //Get CA for InvoiceMasterID, RoleID of CA = 3
        //                 shortPayIndexViewModel.SentFrom = eInvoiceModelContext.GetCATTFindingsApprover(shortPayIndexViewModel.ShortPay.SentFrom);
        //                 //  shortPayIndexViewModel.ShortPay.SentFrom = BuildEmployeeCSV(shortPayIndexViewModel.SentFrom);
        //                 shortPayIndexViewModel.ShortPay.SentFrom = shortPayIndexViewModel.SentFrom.FirstOrDefault().ApproverName;
        //             }
        //             shortPayIndexViewModel.RoutingDetails = new RoutingDetailsViewModel();
        //             shortPayIndexViewModel.RoutingDetails.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);

        //             shortPayIndexViewModel.ShortPayNotesDefault = eInvoiceModelContext.GetConfigMiscData().Where(p => p.ConfiguredCol == "ShortPayNotesDefault").FirstOrDefault().ConfiguredColText;

        //             //in case CA is not avilable during PDF Generation..
        //             if (shortPayIndexViewModel.SentFrom != null)
        //             {
        //                 shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("@CAFullName", shortPayIndexViewModel.ShortPay.SentFrom);
        //                 shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("@emailaddress", shortPayIndexViewModel.SentFrom.FirstOrDefault().WorkEmail);
        //             }
        //             else
        //             {
        //                 shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("@CAFullName", string.Empty);
        //                 shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("@emailaddress", string.Empty);
        //             }
        //             shortPayIndexViewModel.InvoiceGridTotalsforPDF = new InvoiceGridTotalsforPDF();
        //             shortPayIndexViewModel.InvoiceGridTotalsforPDF = eInvoiceModelContext.GetInvoiceGridTotalsforPDF(invoiceMasterID);

        //             string ApprovedAmt = String.Format("{0:C}", shortPayIndexViewModel.InvoiceGridTotalsforPDF.CATotal.Value);
        //             shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("$CAApprovedAdjustment", ApprovedAmt);

        //             shortPayIndexViewModel.InvoiceCAFindingsEmp = new List<InvoiceCAFindingsEmp>();
        //             //CA Line Items Grid..
        //             shortPayIndexViewModel.InvoiceCAFindingsEmp = eInvoiceModelContext.GetInvoiceCAFindingsEmp(invoiceMasterID);

        //             if (shortPayIndexViewModel.InvoiceCAFindingsEmp != null)
        //                 {
        //                 //Manually Adding Totals in Grid Last row.
        //                 shortPayIndexViewModel.InvoiceCAFindingsEmp.Add(
        //                     new InvoiceCAFindingsEmp
        //                     {
        //                         EmployeeName = "Total:",
        //                         RateVariance = shortPayIndexViewModel.InvoiceGridTotalsforPDF.CARateVariance.Value,
        //                         Total = shortPayIndexViewModel.InvoiceGridTotalsforPDF.CATotal.Value,
        //                     });
        //                 }
        //             }
        //         LogManager.Debug("SaveShortPayPDF: START");
        //         SaveViewAsPDFFile("", "~/Views/ShortPayPDF/_GeneratePDF.cshtml", "eInvoiceShortPay-" + documentNo + ".pdf", shortPayIndexViewModel);
        //         LogManager.Debug("SaveShortPayPDF: END");
        //         }

        //     catch (Exception ex)
        //         {
        //         LogManager.Error("GenerateShortPayPDF: ERROR " + ex.Message, ex);
        //         ViewBag.ErrorMessage = ex.Message;
        //         //return PartialView("Error");
        //         }
        //     }

        // public bool SaveViewAsPDFFile(string pageTitle, string viewName, string fileName, object model)
        // {
        //     try
        //     {
        //         LogManager.Debug("SaveViewAsPDFFile: START");

        //         // Render the view html to a string.
        //         HtmlViewRenderer html = new HtmlViewRenderer();
        //         string htmlText = html.RenderViewToString(this, viewName, model);


        //         string cssfile = Server.MapPath("~") + "\\Content\\Custom.css";

        //         System.IO.StreamReader myFile = new System.IO.StreamReader(cssfile);
        //         string cssText = myFile.ReadToEnd();

        //         myFile.Close();

        //         // Let the html be rendered into a PDF document through iTextSharp.
        //         StandardPdfRenderer spdfr = new StandardPdfRenderer();
        //         byte[] buffer = spdfr.Render(htmlText, pageTitle, cssText);

        //         string filepath = Server.MapPath("~") + "\\PDFDocs\\" + fileName;

        //         System.IO.File.WriteAllBytes(filepath, buffer);

        //         LogManager.Debug("SaveViewAsPDFFile: END");

        //         return true;
        //     }

        //     catch (Exception ex)
        //     {
        //         LogManager.Error("SaveViewAsPDFFile: ERROR " + ex.Message, ex);
        //         return false;
        //     }
        // }


        // private List<AccountingCostCodesViewModel> GetAccountingCostCodes(IEnumerable<InvoicePODetail> enumerable)
        // {
        //     try
        //     {

        //         LogManager.Debug("GetAccountingCostCodes: START");

        //         List<AccountingCostCodesViewModel> resultList = new List<AccountingCostCodesViewModel>();

        //         foreach (InvoicePODetail p in enumerable)
        //         {
        //             AccountingCostCodesViewModel result = new AccountingCostCodesViewModel();
        //             result.InvoiceDetailID = p.InvoiceDetailID;
        //             result.InvoiceMasterID = p.InvoiceMasterID;
        //             result.PONumber = p.PONumber;
        //             result.POLine = p.POLine;
        //             result.GLAccount = p.GLAccount;
        //             result.CostCenter = p.CostCenter;
        //             result.WBS = p.WBS;
        //             result.Fund = p.Fund;
        //             result.FunctionalArea = p.FunctionalArea;
        //             result.GrantNumber = p.GrantNumber;
        //             result.InternalOrder = p.InternalOrder;
        //             result.InvoiceAmount = p.InvoiceAmount;
        //             resultList.Add(result);
        //         }
        //         LogManager.Debug("GetAccountingCostCodes: END");
        //         return resultList;
        //     }
        //     catch (Exception ex)
        //     {
        //         LogManager.Error("GetAccountingCostCodes: ERROR " + ex.Message, ex);
        //         return null;
        //     }
        // }

        // private List<ModifyAccountingCostCodesViewModel> GetPOModifyAccountingCostCodes(IEnumerable<InvoicePODetailChanges> enumerable)
        // {
        //     try
        //     {
        //         LogManager.Debug("GetPOModifyAccountingCostCodes: START");
        //         List<ModifyAccountingCostCodesViewModel> resultList = new List<ModifyAccountingCostCodesViewModel>();

        //         foreach (InvoicePODetailChanges p in enumerable)
        //         {
        //             ModifyAccountingCostCodesViewModel result = new ModifyAccountingCostCodesViewModel();
        //             result.InvoiceDetailChangesID = p.InvoiceDetailChangesID;
        //             result.InvoiceMasterID = p.InvoiceMasterID;
        //             result.SAPPONumber = p.PONumber;
        //             result.SAPPOLine = p.POLine;
        //             result.GLAccount = p.GLAccount;
        //             result.CostCenter = p.CostCenter;
        //             result.WBSNo = p.WBS;
        //             result.Fund = p.Fund;
        //             result.FunctionalArea = p.FunctionalArea;
        //             result.GrantNumber = p.GrantNumber;
        //             result.InternalOrder = p.InternalOrder;
        //             result.InvoiceAmount = (p.InvoiceAmount == null ? Decimal.Zero : p.InvoiceAmount);
        //             result.Notes = p.Notes;
        //             resultList.Add(result);
        //         }
        //         LogManager.Debug("GetPOModifyAccountingCostCodes: END");
        //         return resultList;
        //     }
        //     catch (Exception ex)
        //     {
        //         LogManager.Error("GetPOModifyAccountingCostCodes: ERROR " + ex.Message, ex);
        //         return null;
        //     }
        // }


        // private List<AttachmentsViewModel> GetRoutingAttachments(List<InvoiceAttachment> list)
        // {
        //     try
        //     {
        //         LogManager.Debug("GetRoutingAttachments: START");
        //         List<AttachmentsViewModel> resultList = new List<AttachmentsViewModel>();
        //         foreach (InvoiceAttachment p in list)
        //         {
        //             AttachmentsViewModel result = new AttachmentsViewModel();
        //             result.InvoiceAttachmentID = p.InvoiceAttachmentID;
        //             result.InvoiceMasterID = p.InvoiceMasterID;
        //             result.FileName = p.FileName;
        //             result.FileAttachment = p.FileAttachment;
        //             result.UploadedUserID = p.UploadedUserID;
        //             result.LoggedUserID = HttpContext.User.Identity.Name.ToString();
        //             resultList.Add(result);
        //         }
        //         LogManager.Debug("GetRoutingAttachments: END");
        //         return resultList;
        //     }
        //     catch (Exception ex)
        //     {
        //         LogManager.Error("GetRoutingAttachments: ERROR " + ex.Message, ex);
        //         return null;
        //     }
        // }


        // private List<CommentsViewModel> GetRoutingComments(List<InvoiceComment> list)
        // {
        //     try
        //     {
        //         LogManager.Debug("GetRoutingComments: START");
        //         List<CommentsViewModel> resultList = new List<CommentsViewModel>();
        //         foreach (InvoiceComment p in list)
        //         {
        //             CommentsViewModel result = new CommentsViewModel();
        //             result.InvoiceCommentID = p.InvoiceCommentID;
        //             result.InvoiceMasterID = p.InvoiceMasterID;
        //             result.Comment = p.Comment;
        //             result.CommentBy = p.CommentBy;
        //             // result.CommentDate = p.CommentDate.Date.ToShortDateString();
        //             result.CommentDate = p.CommentDate.ToString("MM/dd/yyyy hh:mm tt");
        //             resultList.Add(result);
        //         }
        //         LogManager.Debug("GetRoutingComments: END");

        //         return resultList;
        //     }
        //     catch (Exception ex)
        //     {
        //         LogManager.Error("GetRoutingComments: ERROR " + ex.Message, ex);
        //         return null;
        //     }
        // }


        // private List<ApproversViewModel> GetRoutingApprovers(IEnumerable<InvoicePOApprover> invoicePOApprover)
        // {
        //     try
        //     {
        //         LogManager.Debug("GetRoutingApprovers: START");
        //         List<ApproversViewModel> resultList = new List<ApproversViewModel>();

        //         foreach (InvoicePOApprover p in invoicePOApprover)
        //         {
        //             ApproversViewModel result = new ApproversViewModel();
        //             result.InvoicePOApproverID = p.InvoicePOApproverID;
        //             result.InvoiceMasterID = invoiceMasterID;
        //             result.RoleName = p.Role;
        //             result.PONumberField = p.PONumber;
        //             result.POLine = p.POLine;
        //             result.Approver = ((p.ApproverUserID == null || p.ApproverUserID == String.Empty) ? String.Empty : FetchExchangeApprover(p.ApproverUserID));
        //             result.ApproverSuggestedBySAP = p.ApproverSuggestedbySAP;
        //             resultList.Add(result);
        //         }
        //         LogManager.Debug("GetRoutingApprovers: END");
        //         return resultList;
        //     }

        //     catch (Exception ex)
        //     {
        //         LogManager.Error("GetRoutingApprovers: ERROR " + ex.Message, ex);
        //         return null;
        //     }
        // }


        // private string BuildEmployeeCSV(IEnumerable<ExchangeEmployeeProfile> employees)
        //  {
        //      try
        //      {
        //          LogManager.Debug("BuildEmployeeCSV: START");
        //          string empcsv = string.Empty;
        //          foreach (ExchangeEmployeeProfile sepCATT in employees)
        //          {
        //              empcsv = empcsv + (string.IsNullOrEmpty(empcsv) ? sepCATT.ApproverName : ", " + sepCATT.ApproverName);
        //          }
        //          LogManager.Debug("BuildEmployeeCSV: END");
        //          return empcsv;
        //      }

        //      catch (Exception ex)
        //      {
        //          LogManager.Error("BuildEmployeeCSV: ERROR " + ex.Message, ex);
        //          return null;
        //      }
        //  }


        // #endregion
        }
    }
