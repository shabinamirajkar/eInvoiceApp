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

namespace eInvoiceAutomationWeb.Controllers
{
    public class AdminToolsController : Controller
    {
        private static readonly log4net.ILog LogManager = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //public ActionResult AD()
        //{
        //    // return RedirectToAction("AccessDenied", "Home");
        //    return Redirect("~/Home/AccessDenied");
        //}

        // GET: AdminTools
        [AdminToolsPrivilegeFilter]
        public ActionResult Index()
        {
            FetchUserName();
            return PartialView("_Worklist");
        }
        // GET: All Worklist
        [AdminToolsPrivilegeFilter]
        public ActionResult AllWorklistAll()
            {
            FetchUserName();
            return PartialView("_WorklistAll");
            }


        // GET: Dashboard
        [AdminToolsPrivilegeFilter]
        public ActionResult Dashboard()
        {
            try
            {
                LogManager.Debug("Dashboard: START");
                FetchUserName();
                ViewBag.AddSavedInvoiceTab = false;
                ViewBag.AddErrorLogReportTab = false;
                //Only AP user is allowed to see "SavedInvoice" Tab. if logged in user is not AP, Tab is not displayed in UI
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    string userId = User.Identity.Name;
                    string logginUserName = string.Empty;
                    if (!String.IsNullOrEmpty(userId))
                    {
                        int index = userId.IndexOf("\\");
                        logginUserName = userId.Substring(index + 1);
                    }
                    var result = eInvoiceModelContext.GetConfiguredUser("AP", 0);
                    var authenticated = (from configuredName in result where configuredName.ToLower() == logginUserName.ToLower() select configuredName);
                    if (authenticated != null && authenticated.Count() > 0)
                    {
                        ViewBag.AddSavedInvoiceTab = true;
                    }
                    result = eInvoiceModelContext.GetConfiguredUser("SADMN", 0);
                    authenticated = (from configuredName in result where configuredName.ToLower() == logginUserName.ToLower() select configuredName);
                    if (authenticated != null && authenticated.Count() > 0)
                    {
                        ViewBag.AddErrorLogReportTab = true;
                    }

                }
                LogManager.Debug("Dashboard: END");
                return View("Dashboard", ViewBag);
            }

            catch (Exception ex)
            {
                LogManager.Error("Dashboard: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        // GET: eInvoiceReport
        [AdminToolsPrivilegeFilter]
        public ActionResult Report()
        {
            try
            {
                LogManager.Debug("Report: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    List<StatusReportDropdown> status = eInvoiceModelContext.GetStatusforReport();
                    ViewBag.ReportDDLstatus = status;
                    SAPSourceModelContext SAPmodel = new SAPSourceModelContext();
                    List<ExchangeEmployeeProfile> empList = new List<ExchangeEmployeeProfile>();
                    empList = SAPmodel.FetchExchangeEmployeesList();
                    ExchangeEmployeeProfile Allemp = new ExchangeEmployeeProfile();
                    Allemp.UserID = "All";
                    Allemp.FirstName = "All";
                    Allemp.LastName = string.Empty;
                    Allemp.DistinguishedName = "All";
                    empList.Insert(0, Allemp);
                    ViewBag.MemoryApprovers = empList;
                    string loggedInUserId = HttpContext.User.Identity.Name.ToString();
                    loggedInUserId = loggedInUserId.Split('\\')[1].ToString();
                    ViewBag.DefaultApprover = loggedInUserId;
                    string usertype = GetUserType(loggedInUserId);
                    if (usertype == "RegularUser")
                        ViewBag.HideAccessOption = "hidden";
                    else
                        ViewBag.HideAccessOption = "visible";
                    LogManager.Debug("Report: END");
                    return PartialView("_Report", ViewBag);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("Report: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        // GET: Error Log Report
        [AdminToolsPrivilegeFilter]
        public ActionResult ErrorLogReport()
        {
            try
            {
                LogManager.Debug("ErrorLogReport: START");
                FetchUserName();
                LogManager.Debug("ErrorLogReport: END");
                return PartialView("_ErrorLogReport", ViewBag);
            }
            catch (Exception ex)
            {
                LogManager.Error("ErrorLogReport: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }
    
        // GET: InvoiceSaved
        [AdminToolsPrivilegeFilter]
        public ActionResult SavedInvoice()
        {
            try
            {
                LogManager.Debug("SavedInvoice: START");
                FetchUserName();
                LogManager.Debug("SavedInvoice: END");
                return PartialView("_SavedInvoice", ViewBag);
            }
            catch (Exception ex)
            {
                LogManager.Error("SavedInvoice: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        // GET: ConfigEscalation
        [AdminToolsPrivilegeFilter]
        public ActionResult ConfigMiscData()
        {
            try
            {
                LogManager.Debug("ConfigMiscData: START");
                // eInvoiceModelContext eInvoiceModelContext = new eInvoiceModelContext();
                // ViewBag.Roles = eInvoiceModelContext.GetActivityRoleNames();
                LogManager.Debug("ConfigMiscData: END");
                return View("ConfigMiscData");
            }
            catch (Exception ex)
            {
                LogManager.Error("ConfigMiscData: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        public ActionResult ConfigMiscData_Read([DataSourceRequest] DataSourceRequest request)
        {
            try 
            {
                 LogManager.Debug("ConfigMiscData_Read: START");
                 using (var eInvoiceModelContext = new eInvoiceModelContext())
                 {
                     List<ConfigMiscData> configMiscData = eInvoiceModelContext.GetConfigMiscData();
                     LogManager.Debug("ConfigMiscData_Read: END");
                     return Json(configMiscData.ToDataSourceResult(request));
                 }
            }
            catch (Exception ex)
            {
                LogManager.Error("ConfigMiscData_Read: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ConfigMiscData_Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ConfigMiscData> configMiscDatas)
        {
            try
            {
                LogManager.Debug("ConfigMiscData_Create: START");

                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    List<ConfigMiscData> configMiscDataList = new List<ConfigMiscData>();
                    ModelState.Clear();
                    if (configMiscDatas != null && ModelState.IsValid)
                    {
                        ModelState.Clear();
                        configMiscDataList = configMiscDatas.ToList<ConfigMiscData>();
                    }

                    eInvoiceModelContext.SaveConfigMiscData(configMiscDataList);

                    LogManager.Debug("ConfigMiscData_Create: END");
                    return Json(configMiscDatas.ToDataSourceResult(request, ModelState));
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("ConfigMiscData_Create: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ConfigMiscData_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ConfigMiscData> configMiscDatas)
        {
            try
            { 
                 LogManager.Debug("ConfigMiscData_Update: START");
                 using (var eInvoiceModelContext = new eInvoiceModelContext())
                 {
                     List<ConfigMiscData> configMiscDataList = new List<ConfigMiscData>();
                     ModelState.Clear();
                     if (configMiscDatas != null && ModelState.IsValid)
                     {
                         ModelState.Clear();
                         configMiscDataList = configMiscDatas.ToList<ConfigMiscData>();
                     }
                     eInvoiceModelContext.SaveConfigMiscData(configMiscDataList);
                     LogManager.Debug("ConfigMiscData_Update: END");
                     return Json(configMiscDatas.ToDataSourceResult(request, ModelState));
                 }
            }
            catch (Exception ex)
            {
                LogManager.Error("ConfigMiscData_Update: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        // GET: ConfigEscalation
        [AdminToolsPrivilegeFilter]
        public ActionResult ConfigEscalation()
        {
            try
            {
                LogManager.Debug("ConfigEscalation: START");

                eInvoiceModelContext eInvoiceModelContext = new eInvoiceModelContext();
                ViewBag.Roles = eInvoiceModelContext.GetActivityRoleNames();
                LogManager.Debug("ConfigEscalation: END");

                return View("ConfigEscalation", ViewBag);
            }

            catch (Exception ex)
            {
                LogManager.Error("ConfigEscalation: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        public ActionResult ConfigEscalation_Read([DataSourceRequest] DataSourceRequest request)
        {
            try 
               {
                 LogManager.Debug("ConfigEscalation_Read: START");

                 using (var eInvoiceModelContext = new eInvoiceModelContext())
                 {
                     List<ConfigEscalation> adminrole = eInvoiceModelContext.GetConfigEscalation();
                     LogManager.Debug("ConfigEscalation_Read: END");
                     return Json(adminrole.ToDataSourceResult(request));
                 }
            }

            catch (Exception ex)
            {
                LogManager.Error("ConfigEscalation_Read: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ConfigEscalation_Create([DataSourceRequest] DataSourceRequest request,  [Bind(Prefix = "models")]IEnumerable<ConfigEscalation> configEscalations)
        {
            try 
              { 
                LogManager.Debug("ConfigEscalation_Create: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    List<ConfigEscalation> escalationsList = new List<ConfigEscalation>();
                    ModelState.Clear();
                    if (configEscalations != null && ModelState.IsValid)
                    {
                        ModelState.Clear();
                        escalationsList = configEscalations.ToList<ConfigEscalation>();
                    }

                    eInvoiceModelContext.SaveConfigEscalation(escalationsList);

                    LogManager.Debug("ConfigEscalation_Create: END");

                    return Json(configEscalations.ToDataSourceResult(request, ModelState));
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("ConfigEscalation_Create: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ConfigEscalation_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ConfigEscalation> configEscalations)
        {
            try
            { 
                LogManager.Debug("ConfigEscalation_Update: START");

                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    List<ConfigEscalation> escalationsList = new List<ConfigEscalation>();
                    ModelState.Clear();
                    if (configEscalations != null && ModelState.IsValid)
                    {
                        ModelState.Clear();
                        escalationsList = configEscalations.ToList<ConfigEscalation>();
                    }

                    eInvoiceModelContext.SaveConfigEscalation(escalationsList);


                    LogManager.Debug("ConfigEscalation_Update: END");

                    return Json(configEscalations.ToDataSourceResult(request, ModelState));
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("ConfigEscalation_Update: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ConfigEscalation_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ConfigEscalation> configEscalations)
        {
            try 
            {
                LogManager.Debug("ConfigEscalation_Destroy: START");

                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    List<ConfigEscalation> escalationsList = new List<ConfigEscalation>();
                    ModelState.Clear();
                    if (configEscalations != null && ModelState.IsValid)
                    {
                        ModelState.Clear();
                        escalationsList = configEscalations.ToList<ConfigEscalation>();
                    }

                    eInvoiceModelContext.DeleteConfigEscalation(escalationsList);
                    LogManager.Debug("ConfigEscalation_Destroy: END");
                    return Json(configEscalations.ToDataSourceResult(request, ModelState));
                }
            }


            catch (Exception ex)
            {
                LogManager.Error("ConfigEscalation_Destroy: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }

        }


        public JsonResult FetchApprovers(string text)
        {
            try
            {
                LogManager.Debug("FetchApprovers: START");
                SAPSourceModelContext SAPmodel = new SAPSourceModelContext();
                IEnumerable<ExchangeEmployeeProfile> selectedSAPEmp = null;
                if (!string.IsNullOrEmpty(text.Trim()))
                {
                    selectedSAPEmp = SAPmodel.GetSAPExchangeEmployeeFilter((text));
                }
                else
                {
                    ExchangeEmployeeProfile empty = new ExchangeEmployeeProfile();
                    empty.UserID = "";
                    empty.FirstName = "";
                    empty.LastName = "";
                    List<ExchangeEmployeeProfile> emptyList = new List<ExchangeEmployeeProfile>();
                    emptyList.Add(empty);
                    selectedSAPEmp = emptyList;
                }

                LogManager.Debug("FetchApprovers: END");
                return Json(selectedSAPEmp, JsonRequestBehavior.AllowGet);
            }


            catch (Exception ex)
            {
                LogManager.Error("FetchApprovers: ERROR " + ex.Message, ex);
                return null;
            }
        }

        // GET: RolesAdmin
        [AdminToolsPrivilegeFilter]
        public ActionResult RolesAdmin()
        {
            try
            {
                LogManager.Debug("RolesAdmin: START");
                eInvoiceModelContext eInvoiceModelContext = new eInvoiceModelContext();
                ViewBag.Roles = eInvoiceModelContext.GetAdminRoleNames();
                LogManager.Debug("RolesAdmin: END");
                return View("RolesAdmin", ViewBag);
            }

            catch (Exception ex)
            {
                LogManager.Error("RolesAdmin: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        public ActionResult ConfigAdmin_Read([DataSourceRequest] DataSourceRequest request)
        {
            try
            { 
                 LogManager.Debug("ConfigAdmin_Read: START");
                 using (var eInvoiceModelContext = new eInvoiceModelContext())
                 {
                     List<AdminRoleForCRUD> adminrole = eInvoiceModelContext.GetAdminRole();
                     LogManager.Debug("ConfigAdmin_Read: END");
                     return Json(adminrole.ToDataSourceResult(request));
                 }
            }
            catch (Exception ex)
            {
                LogManager.Error("ConfigAdmin_Read: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ConfigAdmin_Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<AdminRoleForCRUD> adminroles)
        {
            try
            { 
                 LogManager.Debug("ConfigAdmin_Create: START");

                 using (var eInvoiceModelContext = new eInvoiceModelContext())
                 {
                     List<AdminRoleForCRUD> adminroleList = new List<AdminRoleForCRUD>();
                     ModelState.Clear();
                     if (adminroles != null && ModelState.IsValid)
                     {
                         ModelState.Clear();
                         adminroleList = adminroles.ToList<AdminRoleForCRUD>();
                     }

                 //    List<AdminRoleForCRUD> changedrows = new List<AdminRoleForCRUD>();
                    eInvoiceModelContext.SaveAdminRole(adminroleList);

                     LogManager.Debug("ConfigAdmin_Create: END");

                     return Json(adminroles.ToDataSourceResult(request, ModelState));
                 }
            }
            catch (Exception ex)
            {
                LogManager.Error("ConfigAdmin_Create: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ConfigAdmin_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<AdminRoleForCRUD> adminroles)
        {
            try
            {
                LogManager.Debug("ConfigAdmin_Update: START");
            
            using (var eInvoiceModelContext = new eInvoiceModelContext())
            {
                List<AdminRoleForCRUD> adminroleList = new List<AdminRoleForCRUD>();
                ModelState.Clear();
                if (adminroles != null && ModelState.IsValid)
                {
                    ModelState.Clear();
                    adminroleList = adminroles.ToList<AdminRoleForCRUD>();
                }
                
                eInvoiceModelContext.SaveAdminRole(adminroleList);

                LogManager.Debug("ConfigAdmin_Update: END");

                return Json(adminroles.ToDataSourceResult(request, ModelState));
              }
            }
              catch (Exception ex)
            {
                LogManager.Error("ConfigAdmin_Update: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ConfigAdmin_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<AdminRoleForCRUD> adminroles)
        {
            try
            {
                LogManager.Debug("ConfigAdmin_Destroy: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    List<AdminRoleForCRUD> adminroleList = new List<AdminRoleForCRUD>();
                    ModelState.Clear();
                    if (adminroles != null && ModelState.IsValid)
                    {
                        ModelState.Clear();
                        adminroleList = adminroles.ToList<AdminRoleForCRUD>();
                    }
                    eInvoiceModelContext.DeleteAdminRole(adminroleList);
                    LogManager.Debug("ConfigAdmin_Destroy: END");

                    return Json(adminroles.ToDataSourceResult(request, ModelState));
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("ConfigAdmin_Destroy: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        private string GetUserType(string userName)
        {
            try
            {
                LogManager.Debug("GetUserType: START");
                string userType = "RegularUser";
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    if (!String.IsNullOrEmpty(userName))
                    {
                        var result = eInvoiceModelContext.GetConfiguredUser("SADMN", 0);
                        var authenticated = (from configuredName in result where configuredName.ToLower() == userName.ToLower() select configuredName);
                        if (authenticated != null && authenticated.Count() > 0)
                        {
                            userType = "SADMN";
                        }
                        else
                        {
                            //Check if AP
                            result = eInvoiceModelContext.GetConfiguredUser("AP", 0);
                            authenticated = (from configuredName in result where configuredName.ToLower() == userName.ToLower() select configuredName);
                            if (authenticated != null && authenticated.Count() > 0)
                            {
                                userType = "AP";
                            }
                        }
                    }
                }

                LogManager.Debug("GetUserType: END");

                return userType;
            }
            catch (Exception ex)
            {
                LogManager.Error("GetUserType: ERROR " + ex.Message, ex);
                return null;
            }
        }

        // Method - This is invoked  when Search is clicked on Error Log tab and there are no validation errors
        public ActionResult GetErrorLog([DataSourceRequest] DataSourceRequest request, DashboardReportSearch reportSearch)
        {
            try
            {
                LogManager.Debug("GetErrorLog: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    List<InvoiceErrorLog> invoiceErrorReport = eInvoiceModelContext.GetErrorLogReport(reportSearch);
                    DataSourceResult result = new DataSourceResult();
                    result = invoiceErrorReport.ToDataSourceResult(request, p => new InvoiceErrorLog
                    {
                        InvoiceErrorLogID = p.InvoiceErrorLogID,
                        Date = p.Date,
                        Exception = p.Exception,
                        Level = p.Level,
                        Message = p.Message,
                        Thread = p.Thread,
                        UserID= p.UserID,
                        UserDisplayName= p.UserDisplayName
                    });


                    LogManager.Debug("GetErrorLog: END");
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("GetErrorLog: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        // Method - This is invoked  when Submit is clicked on Saved tab and there are no validation errors
        public ActionResult SavedInvoice_Read([DataSourceRequest] DataSourceRequest request, DashboardReportSearch reportSearch)
        {
            try
            {
                LogManager.Debug("SavedInvoice_Read: START");
                var url = System.Configuration.ConfigurationManager.AppSettings["eInvoiceRuntimeBaseURL"].ToString() + "/Home/eInvoice?InvoiceMasterID=";
                string loggedInUserId = HttpContext.User.Identity.Name.ToString().Split('\\')[1].ToString();
                reportSearch.SubmittedBy = loggedInUserId;
                //Only AP can accress this section, hence send LoggedinUertyppe is AP
                reportSearch.LoggedinUserType = "AP";
                //Get All Invoices submitted by AP
                reportSearch.Access = "AllInvoices";

                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    List<DashboardReport> dashboardReports = eInvoiceModelContext.GetDashboardReport(reportSearch);

                    foreach(DashboardReport dr in dashboardReports)
                    {
                        if (dr.ContractNo == "0")
                        {
                            dr.ContractNo = "View Request";
                        }
                    }
                    LogManager.Debug("SavedInvoice_Read: END");
                    return Json(dashboardReports.ToDataSourceResult(request));
                }

                //LogManager.Debug("GetSavedInvoice: START");
                //DashboardReportSearch reportSearch = new DashboardReportSearch();
                //using (var eInvoiceModelContext = new eInvoiceModelContext())
                //{
                //    //var url = @"http://localhost/eInvoiceAutomationWeb/Home/eInvoice?InvoiceMasterID=";
                //    var url = ConfigurationManager.AppSettings["eInvoiceRuntimeBaseURL"].ToString() + "/Home/eInvoice?InvoiceMasterID=";
                //    string loggedInUserId = HttpContext.User.Identity.Name.ToString().Split('\\')[1].ToString();
                //    reportSearch.SubmittedBy = loggedInUserId;
                //    //Only AP can accress this section, hence send LoggedinUertyppe is AP
                //    reportSearch.LoggedinUserType = "AP";
                //    //Get All Invoices submitted by AP
                //    reportSearch.Access = "AllInvoices";

                //    reportSearch.DateFrom = DateTime.Today.AddYears(-1);
                //    reportSearch.DateTo = DateTime.Today;
                //    reportSearch.Status = "AP Review";

                //    List<DashboardReport> dashboardReport = eInvoiceModelContext.GetDashboardReport(reportSearch);
                //    DataSourceResult result = new DataSourceResult();
                //    result = dashboardReport.ToDataSourceResult(request, p => new DashboardReport
                //    {
                //        ContractNo = p.ContractNo == "0" ? "<a href='" + url + p.InvoiceMasterID + "'> View Request </a>" : "<a href='" + url + p.InvoiceMasterID + "'>" + p.ContractNo + "</a>",
                //        DocumentNo= p.DocumentNo,
                //        DaysPending = p.DaysPending,
                //        DestinationUser = p.DestinationUser,
                //        InvoiceAmount = p.InvoiceAmount,
                //        InvoiceNo = p.InvoiceNo,
                //        PaymentDueBy = p.PaymentDueBy,
                //        VendorName = p.VendorName,
                //    });


                //    LogManager.Debug("GetSavedInvoice: END");
                //    return Json(result);
                //  //  return Json(result, JsonRequestBehavior.AllowGet);
                // }
            }

            catch (Exception ex)
            {
                LogManager.Error("SavedInvoice_Read: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        [HttpPost]
        public ActionResult SavedInvoice_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<DashboardReport> reports)
        {
            try
            {
                LogManager.Debug("SavedInvoice_Destroy: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    List<DashboardReport> reportList = new List<DashboardReport>();

                    if (reports != null && ModelState.IsValid)
                    {
                        ModelState.Clear();
                        reportList = reports.ToList<DashboardReport>();
                    }
                    eInvoiceModelContext.DeleteDocumentNo(reportList);
                    LogManager.Debug("SavedInvoice_Destroy: END");
                    return Json(reports.ToDataSourceResult(request, ModelState));
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("SavedInvoice_Destroy: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

         // Method - This is invoked  when Submit is clicked on Report tab and there are no validation errors
        public ActionResult GetReportData([DataSourceRequest] DataSourceRequest request, DashboardReportSearch reportSearch)
        {
            try
            {
                LogManager.Debug("GetReportData: START");

                //var url = @"http://localhost/eInvoiceAutomationWeb/Home/eInvoice?InvoiceMasterID=";
                var url = ConfigurationManager.AppSettings["eInvoiceRuntimeBaseURL"].ToString() + "/Home/eInvoice?InvoiceMasterID=";
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    string loggedInUserId = HttpContext.User.Identity.Name.ToString();
                    loggedInUserId = loggedInUserId.Split('\\')[1].ToString();
                    string UserType = GetUserType(loggedInUserId);
                    reportSearch.LoggedinUserType = UserType;
                    if (UserType == "RegularUser")
                    {
                        reportSearch.Access = loggedInUserId;
                    }
                    else
                    {
                        if (reportSearch.Access == "MyInvoices") { reportSearch.Access = loggedInUserId; }
                    }
                    List<DashboardReport> dashboardReport = eInvoiceModelContext.GetDashboardReport(reportSearch);
                    DataSourceResult result = new DataSourceResult();
                    result = dashboardReport.ToDataSourceResult(request, p => new DashboardReport
                    {
                        ContractNo = p.ContractNo == "0" ? "<a href='" + url + p.InvoiceMasterID + "&ReadOnly=true' target='_blank' > View Request </a>" : "<a href='" + url + p.InvoiceMasterID + "&ReadOnly=true' target='_blank' >" + p.ContractNo + "</a>",
                        DaysPending = p.DaysPending,
                        DestinationUser = p.DestinationUser,
                        InvoiceAmount = p.InvoiceAmount,
                        InvoiceNo = p.InvoiceNo,
                        PaymentDueBy = p.PaymentDueBy,
                        DocumentNo= p.DocumentNo,
                        VendorName = p.VendorName,
                        VendorNo= p.VendorNo.TrimStart('0'),
                        InvoiceStatus = p.InvoiceStatus,
                        CompletedDate=p.CompletedDate,
                    });
                    LogManager.Debug("GetReportData: END");
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetReportData: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        public static string GetDomain(string s)
        {
            int stop = s.IndexOf("\\");
            return (stop > -1) ? s.Substring(0, stop + 1) : null;
        }

        public static string GetLogin(string s)
        {
            int stop = s.IndexOf("\\");
            return (stop > -1) ? s.Substring(stop + 1, s.Length - stop - 1) : null;
        }

        //Gets called when worklist Tab Opens...
        public ActionResult Worklist([DataSourceRequest]DataSourceRequest request)
        {
            try
            {
                LogManager.Debug("Worklist: START");
                string loggedInUserId = HttpContext.User.Identity.Name.ToString();

                string domainUser = System.IO.Path.GetFileNameWithoutExtension(loggedInUserId);
                domainUser = loggedInUserId.Split('\\')[1];

                string query = "select ISNULL(NULLIF(IM.ContractNo, ''),'0') Text1, IM.VendorName Text2, IM.InvoiceNo Text3, IM.Status Text4, IM.DocumentNo Text5, IM.InvoiceAmount Double1, IM.PaymentDueBy Date1," +
                                "DateDiff(dd,getdate(), IM.PaymentDueBy) Number2, IM.IAProcID ProcessInstanceID from InvoiceMaster_SMO IM Where IM.Status <> 'Rejected' AND IM.Status <> 'Complete'" +
                                "AND (APSubmittedByUserID='" + domainUser + "' OR Status <> 'FAP Review') ";
                
                List<CustomWorkList> worklist = CustomWorklistService.GetCustomWorklist(loggedInUserId, query);
                DataSourceResult result = new DataSourceResult();
                result = worklist.ToDataSourceResult(request, p => new CustomWorkList
                {
                    
                    Text1 = p.Text1 == "0" ? "<a href='" + p.ClientURL + "'> Open Task </a>" : "<a href='" + p.ClientURL + "'>" + p.Text1 + "</a>",
                    Text2 = p.Text2,            //Vendor Name
                    Text3 = p.Text3,            //Invoice No
                    Text5=  p.Text5,            //Document No
                    Double1 = p.Double1,        //Amount
                    Date1 = p.Date1,            //Payment Due By
                    Number2 = p.Number2,        //Days Pending
                    ActivityName = p.ActivityName,
                    Status = p.Text4,             //Status
                    SharedUser = p.SharedUser,
                    ViewFlowURL = "<a href='" + p.ViewFlowURL + "' target='_blank' >View Flow</a>",
                });
                LogManager.Debug("Worklist: END");
                return Json(result);
            }

            catch (Exception ex)
            {
                LogManager.Error("Worklist: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        //Gets called when 'All Worklist' Tab Opens...
        public ActionResult WorklistAll([DataSourceRequest]DataSourceRequest request)
            {
            try
                {
                LogManager.Debug("WorklistAll: START");

                string query = "select ISNULL(NULLIF(IM.ContractNo, ''),'0') Text1, IM.VendorName Text2, IM.InvoiceNo Text3, IM.Status Text4, IM.DocumentNo Text5, IM.APSubmittedByUserID Text6, IM.InvoiceAmount Double1, IM.PaymentDueBy Date1," +
                                "DateDiff(dd,getdate(), IM.PaymentDueBy) Number2, IM.IAProcID ProcessInstanceID from InvoiceMaster_SMO IM Where IM.Status <> 'Rejected' AND IM.Status <> 'Complete'";

                string loggedInUserId = HttpContext.User.Identity.Name.ToString();
                List<CustomWorkList> worklist = CustomWorklistService.GetCustomWorklist(loggedInUserId, query);
                DataSourceResult result = new DataSourceResult();
                result = worklist.ToDataSourceResult(request, p => new CustomWorkList
                {

                    Text1 = p.Text1 == "0" ? "<a href='" + p.ClientURL + "'> Open Task </a>" : "<a href='" + p.ClientURL + "'>" + p.Text1 + "</a>",
                    Text2 = p.Text2,            //Vendor Name
                    Text3 = p.Text3,            //Invoice No
                    Text5 = p.Text5,            //Document No
                    Text6 = p.Text6,            //AP Submitted By
                    Double1 = p.Double1,        //Amount
                    Date1 = p.Date1,            //Payment Due By
                    Number2 = p.Number2,        //Days Pending
                    ActivityName = p.ActivityName,
                    Status = p.Text4,             //Status
                    SharedUser = p.SharedUser,
                    ViewFlowURL = "<a href='" + p.ViewFlowURL + "' target='_blank' >View Flow</a>",
                });
                LogManager.Debug("WorklistAll: END");
                return Json(result);
                }

            catch (Exception ex)
                {
                LogManager.Error("WorklistAll: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }

        #region Private Methods

        private void FetchUserName()
        {
            try
            {
                LogManager.Debug("FetchUserName: START");
                string loggedInUserId = HttpContext.User.Identity.Name.ToString();
                int index = loggedInUserId.IndexOf('\\');
                string userId = loggedInUserId.Substring(index + 1);

                using (SAPSourceModelContext context = new SAPSourceModelContext())
                {
                    string userName = context.FetchLoggedInUserName(userId);
                    Session["LoggedInUserName"] = userName;
                }
                LogManager.Debug("FetchUserName: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("FetchUserName: ERROR " + ex.Message, ex);

            }
        }

        #endregion
    } 
}