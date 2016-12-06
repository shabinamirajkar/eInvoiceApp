using eInvoiceApplication.DomainModel;
using eInvoiceAutomationWeb.ViewModels;
using Kendo.Mvc.UI;
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
using SAPSourceMasterApplication.DomainModel;
using eInvoice.K2Access;
using eInvoiceAutomationWeb.Active_Directory_Access;
using System.Web.UI;

namespace eInvoiceAutomationWeb.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None)]
    [SessionTimeOutFilter]
    public class CATTFindingsController : Controller
    {
        private static readonly log4net.ILog LogManager = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        int InvoiceMasterID;
        int InvoiceCATTFindingsID;

        CATTFindingsViewModel cattfindingsViewModel;

      
        public ActionResult CATTFinding(string documentNo, string status, string SN, bool ReadOnly, int defaultInvoiceMasterID = 0)
        {
            try
            {
                LogManager.Debug("CATTFinding: START");

                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    string LoggedUserCATTorCA = string.Empty;
                    cattfindingsViewModel = new CATTFindingsViewModel();

                    //if InvoiceMasterID is passed use that, and store in session..
                    if (defaultInvoiceMasterID > 0)
                    {
                        Session["InvoiceMasterID"] = defaultInvoiceMasterID;
                    }

                    if (!String.IsNullOrEmpty(SN))
                    {
                        int SNIndex = SN.IndexOf('_');
                        string newProcId = SN.Substring(0, SNIndex);
                        InvoiceMasterID = eInvoiceModelContext.GetInvoiceMasterIDFromProcId(Convert.ToInt32(newProcId));
                        Session["InvoiceMasterID"] = InvoiceMasterID;
                        cattfindingsViewModel.DocumentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(InvoiceMasterID);
                    }
                    else
                    {
                        if (Session["InvoiceMasterID"] != null)
                        {
                            InvoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                            cattfindingsViewModel.DocumentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(InvoiceMasterID);
                        }
                    }
                    if (Session["InvoiceMasterID"] != null)
                    {
                        //Store Session ID in Session..
                        Session["InvoiceMasterID"] = InvoiceMasterID;
                        cattfindingsViewModel.DocumentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(InvoiceMasterID);
                        TempData["DocumentNo"] = cattfindingsViewModel.DocumentNo;
                        cattfindingsViewModel.RoutingDetails = new RoutingDetailsViewModel();
                        //Get Routing Header
                        cattfindingsViewModel.RoutingDetails.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(InvoiceMasterID);

                        //Get InvoiceCATTFindings..
                        cattfindingsViewModel.InvoiceCATTFindings = eInvoiceModelContext.GetInvoiceCATTFindings(InvoiceMasterID);
                        //Get CA for InvoiceMasterID, RoleID of CA=3
                        cattfindingsViewModel.ToCA = eInvoiceModelContext.GetCATTFindingsApprover(cattfindingsViewModel.InvoiceCATTFindings.AddressedTo);
                        //Get CATT for InvoiceMasterID, RoleID of CATT=2
                        cattfindingsViewModel.FromCATT = eInvoiceModelContext.GetCATTFindingsApprover(cattfindingsViewModel.InvoiceCATTFindings.SentFrom);
                        cattfindingsViewModel.DateSubmit = cattfindingsViewModel.InvoiceCATTFindings.Date;
                        //Store InvoiceCATTFindingsID in session for Future User..
                        Session["InvoiceCATTFindingsID"] = cattfindingsViewModel.InvoiceCATTFindings.InvoiceCATTFindingsID;
                        decimal? invoiceAmt = cattfindingsViewModel.RoutingDetails.InvoiceDetails.InvoiceAmount;

                        if (cattfindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment.HasValue)
                            cattfindingsViewModel.AssetPayment = invoiceAmt - cattfindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment.Value;
                        else
                        {
                            cattfindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment = 0;
                            cattfindingsViewModel.AssetPayment = invoiceAmt - 0;
                        }
                        if (cattfindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment.HasValue)
                            cattfindingsViewModel.ApprovedPayment = invoiceAmt - cattfindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment.Value;
                        else
                        {
                            cattfindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment = 0;
                            cattfindingsViewModel.ApprovedPayment = invoiceAmt - 0;
                        }

                        //If not ReadOnly try Load editable view..
                        if (!ReadOnly)
                        {
                            string InvoiceStatus = eInvoiceModelContext.GetStatus(InvoiceMasterID);

                            if (InvoiceStatus == "CATT Review")
                            {
                                //if Logged in User Role is CATT
                                LoggedUserCATTorCA = GetUserRoleName(0, "CATT");
                                cattfindingsViewModel.LoggedinUserType = LoggedUserCATTorCA;
                            }
                            if (InvoiceStatus == "CA Review")
                            {
                                //if Logged in User Role is CA
                                LoggedUserCATTorCA = GetUserRoleName(InvoiceMasterID, "CA");
                                cattfindingsViewModel.LoggedinUserType = LoggedUserCATTorCA;
                            }
                        }


                        //if Logged in User (CATT or CA) was found for CATT Review or CA Review..Load Editable View..else ReadOnly
                        if (LoggedUserCATTorCA.Length > 0)
                        {
                            if (cattfindingsViewModel.LoggedinUserType == "CATT")
                            {
                                List<InvoiceCATTFindingsEmp> cattfindingsEmp = eInvoiceModelContext.GetInvoiceCATTFindingsEmp(InvoiceMasterID);
                                if (cattfindingsEmp.Count > 0)
                                {
                                    LogManager.Debug("CATTFinding: END");
                                    return PartialView("_CATTFindingsReadOnlyForCATT", cattfindingsViewModel);
                                }
                                else
                                {
                                    LogManager.Debug("CATTFinding: END");
                                    return PartialView("_CATTFindings", cattfindingsViewModel);
                                }
                            }
                            else
                            {
                                LogManager.Debug("CATTFinding: END");
                                return PartialView("_CATTFindings", cattfindingsViewModel);
                            }
                        }
                        else
                        {
                            cattfindingsViewModel.ToCACSV = BuildEmployeeCSV(cattfindingsViewModel.ToCA);
                            cattfindingsViewModel.FromCATTCSV = BuildEmployeeCSV(cattfindingsViewModel.FromCATT);
                            LogManager.Debug("CATTFinding: END");
                            return PartialView("_CATTFindingsReadOnly", cattfindingsViewModel);
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
                LogManager.Error("CATTFinding: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
               // return PartialView("_CATTFindingsReadOnly", cattfindingsViewModel);
            }
           
        }


        private string BuildEmployeeCSV(IEnumerable<ExchangeEmployeeProfile> employees)
        {
            try
            {
                LogManager.Debug("BuildEmployeeCSV: START");
                string empcsv = string.Empty;
                foreach (ExchangeEmployeeProfile sepCATT in employees)
                {
                    string empName = sepCATT.FirstName + " " + sepCATT.LastName;
                    empcsv = empcsv + (string.IsNullOrEmpty(empcsv) ? empName : ", " + empName);
                }
                LogManager.Debug("BuildEmployeeCSV: END");
                return empcsv;
            }

            catch (Exception ex)
            {
                LogManager.Error("BuildEmployeeCSV: ERROR " + ex.Message, ex);
                return null;
            }
        }


        private string GetUserRoleName(int InvoiceMasterID, string RoleName)
        {
            try
            {
                LogManager.Debug("GetUserRoleName: START");

                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    string userId = HttpContext.User.Identity.Name.ToString();
                    // Removing Domain name from the logged in user name
                    if (!String.IsNullOrEmpty(userId))
                    {
                        string loggedUser = String.Empty;
                        int index = userId.IndexOf("\\");
                        loggedUser = userId.Substring(index + 1);
                        var result = eInvoiceModelContext.GetConfiguredUser(RoleName, InvoiceMasterID);
                        var authenticated = (from configuredName in result where configuredName.ToLower() == loggedUser.ToLower() select configuredName);
                        if (authenticated != null && authenticated.Count() > 0)
                        {
                            LogManager.Debug("GetUserRoleName: END");
                            return RoleName;
                        }
                        else
                        {
                            LogManager.Debug("GetUserRoleName: END");
                            return string.Empty;
                        }
                    }
                }
                LogManager.Debug("GetUserRoleName: END");
                return string.Empty;
            }
            catch (Exception ex)
            {
                LogManager.Error("GetUserRoleName: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public JsonResult FetchApprovers(string text)
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

        public ActionResult CAFindingsEmp_Read([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                LogManager.Debug("CAFindingsEmp_Read: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    InvoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    List<InvoiceCAFindingsEmp> cafindingsEmp = eInvoiceModelContext.GetInvoiceCAFindingsEmp(InvoiceMasterID);
                    LogManager.Debug("CAFindingsEmp_Read: END");
                    return Json(cafindingsEmp.ToDataSourceResult(request));
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("CAFindingsEmp_Read: ERROR " + ex.Message, ex);
                return PartialView("Error");
            }
        }

        [HttpPost]
        public ActionResult CAFindingsEmp_Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]List<InvoiceCAFindingsEmp> cafindingsEmps)
        {
            try
            {
                LogManager.Debug("CAFindingsEmp_Create: START");

                using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                {
                    List<InvoiceCAFindingsEmp> invoiceCAFindingsEmpChanges = new List<InvoiceCAFindingsEmp>();
                    if (cafindingsEmps != null && cafindingsEmps.Count > 0 && ModelState.IsValid)
                    {
                        if (Session["InvoiceCATTFindingsID"] != null)
                            InvoiceCATTFindingsID = Convert.ToInt32(Session["InvoiceCATTFindingsID"]);

                        foreach (var cattfindingsEmp in cafindingsEmps)
                        {
                            if (cattfindingsEmp.InvoiceRate == null) { cattfindingsEmp.InvoiceRate = Decimal.Zero; }
                            if (cattfindingsEmp.ApprovedRate == null) { cattfindingsEmp.ApprovedRate = Decimal.Zero; }
                            if (cattfindingsEmp.InvoiceHours == null) { cattfindingsEmp.InvoiceHours = Decimal.Zero; }
                            if (cattfindingsEmp.ApprovedHours == null) { cattfindingsEmp.ApprovedHours = Decimal.Zero; }
                            if (cattfindingsEmp.Total == null) { cattfindingsEmp.Total = Decimal.Zero; }

                            invoiceCAFindingsEmpChanges.Add(new InvoiceCAFindingsEmp
                            {
                                InvoiceCAFindingsEmpID = cattfindingsEmp.InvoiceCAFindingsEmpID,
                                InvoiceCATTFindingsID = InvoiceCATTFindingsID,
                                EmployeeName = cattfindingsEmp.EmployeeName,
                                Classification = cattfindingsEmp.Classification,
                                InvoiceRate = cattfindingsEmp.InvoiceRate,
                                ApprovedRate = cattfindingsEmp.ApprovedRate,
                                RateVariance = cattfindingsEmp.InvoiceRate - cattfindingsEmp.ApprovedRate,
                                InvoiceHours = cattfindingsEmp.InvoiceHours,
                                ApprovedHours = cattfindingsEmp.ApprovedHours,
                                VarianceHours = cattfindingsEmp.InvoiceHours - cattfindingsEmp.ApprovedHours,
                                Total = cattfindingsEmp.Total,
                                Notes = cattfindingsEmp.Notes,
                            });
                        }
                        invoiceCAFindingsEmpChanges = eInvoiceModelcontext.SaveInvoiceCAFindingsEmp(invoiceCAFindingsEmpChanges);
                    }

                    foreach (InvoiceCAFindingsEmp emp in cafindingsEmps)
                    {
                        emp.InvoiceCAFindingsEmpID = invoiceCAFindingsEmpChanges.FirstOrDefault().InvoiceCAFindingsEmpID;
                        emp.RateVariance = invoiceCAFindingsEmpChanges.FirstOrDefault().RateVariance;
                        emp.Total = invoiceCAFindingsEmpChanges.FirstOrDefault().Total;
                    }
                    LogManager.Debug("CAFindingsEmp_Create: END");
                    return Json(cafindingsEmps.ToDataSourceResult(request, ModelState));
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("CAFindingsEmp_Create: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        [HttpPost]
        public ActionResult CAFindingsEmp_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]List<InvoiceCAFindingsEmp> cafindingsEmps)
        {
            try
            {
                LogManager.Debug("CAFindingsEmp_Update: START");

                using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                {
                    List<InvoiceCAFindingsEmp> invoiceCAFindingsEmpChanges = new List<InvoiceCAFindingsEmp>();
                    if (cafindingsEmps != null && cafindingsEmps.Count > 0 && ModelState.IsValid)
                    {
                        if (Session["InvoiceCATTFindingsID"] != null)
                            InvoiceCATTFindingsID = Convert.ToInt32(Session["InvoiceCATTFindingsID"]);

                        foreach (var cattfindingsEmp in cafindingsEmps)
                        {
                            if (cattfindingsEmp.InvoiceRate == null) { cattfindingsEmp.InvoiceRate = Decimal.Zero; }
                            if (cattfindingsEmp.ApprovedRate == null) { cattfindingsEmp.ApprovedRate = Decimal.Zero; }
                            if (cattfindingsEmp.InvoiceHours == null) { cattfindingsEmp.InvoiceHours = Decimal.Zero; }
                            if (cattfindingsEmp.ApprovedHours == null) { cattfindingsEmp.ApprovedHours = Decimal.Zero; }
                            if (cattfindingsEmp.Total == null) { cattfindingsEmp.Total = Decimal.Zero; }

                            if (cattfindingsEmp.InvoiceRate.Value != 0 || cattfindingsEmp.ApprovedRate.Value != 0 || cattfindingsEmp.InvoiceHours.Value != 0 || cattfindingsEmp.ApprovedHours.Value != 0)
                            {
                                decimal RateVariance = Decimal.Zero;
                                RateVariance = cattfindingsEmp.InvoiceRate.Value - cattfindingsEmp.ApprovedRate.Value;

                                if (RateVariance != 0)
                                {
                                    cattfindingsEmp.Total = RateVariance * cattfindingsEmp.InvoiceHours;
                                }
                                else if (RateVariance == 0)
                                {
                                    cattfindingsEmp.Total = (cattfindingsEmp.InvoiceHours - cattfindingsEmp.ApprovedHours) * cattfindingsEmp.ApprovedRate;
                                }
                            }

                            invoiceCAFindingsEmpChanges.Add(new InvoiceCAFindingsEmp
                            {
                                InvoiceCAFindingsEmpID = cattfindingsEmp.InvoiceCAFindingsEmpID,
                                InvoiceCATTFindingsID = InvoiceCATTFindingsID,
                                EmployeeName = cattfindingsEmp.EmployeeName,
                                Classification = cattfindingsEmp.Classification,
                                InvoiceRate = cattfindingsEmp.InvoiceRate,
                                ApprovedRate = cattfindingsEmp.ApprovedRate,
                                RateVariance = cattfindingsEmp.InvoiceRate - cattfindingsEmp.ApprovedRate,
                                InvoiceHours = cattfindingsEmp.InvoiceHours,
                                ApprovedHours = cattfindingsEmp.ApprovedHours,
                                VarianceHours = cattfindingsEmp.InvoiceHours - cattfindingsEmp.ApprovedHours,
                                Total = cattfindingsEmp.Total,
                                Notes = cattfindingsEmp.Notes,
                            });
                        }
                        invoiceCAFindingsEmpChanges = eInvoiceModelcontext.SaveInvoiceCAFindingsEmp(invoiceCAFindingsEmpChanges);
                    }

                    foreach (InvoiceCAFindingsEmp emp in cafindingsEmps)
                    {
                        emp.InvoiceCAFindingsEmpID = invoiceCAFindingsEmpChanges.FirstOrDefault().InvoiceCAFindingsEmpID;
                        emp.RateVariance = invoiceCAFindingsEmpChanges.FirstOrDefault().RateVariance;
                        emp.Total = invoiceCAFindingsEmpChanges.FirstOrDefault().Total;
                    }
                    LogManager.Debug("CAFindingsEmp_Update: END");
                    return Json(cafindingsEmps.ToDataSourceResult(request, ModelState));
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("CAFindingsEmp_Update: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        [HttpPost]
        public ActionResult CAFindingsEmp_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]List<InvoiceCAFindingsEmp> cafindingsEmps)
        {
            try
            {
                LogManager.Debug("CAFindingsEmp_Destroy: START");
                using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                {
                    List<InvoiceCAFindingsEmp> invoiceCAFindingsEmpChanges = new List<InvoiceCAFindingsEmp>();
                    if (cafindingsEmps != null && cafindingsEmps.Count > 0 && ModelState.IsValid)
                    {
                        if (Session["InvoiceCATTFindingsID"] != null)
                            InvoiceCATTFindingsID = Convert.ToInt32(Session["InvoiceCATTFindingsID"]);

                        foreach (var cattfindingsEmp in cafindingsEmps)
                        {
                            if (cattfindingsEmp.Total == null) { cattfindingsEmp.Total = Decimal.Zero; }
                            invoiceCAFindingsEmpChanges.Add(new InvoiceCAFindingsEmp
                            {
                                InvoiceCAFindingsEmpID = cattfindingsEmp.InvoiceCAFindingsEmpID,
                                InvoiceCATTFindingsID = InvoiceCATTFindingsID,
                                EmployeeName = cattfindingsEmp.EmployeeName,
                                Classification = cattfindingsEmp.Classification,
                                InvoiceRate = cattfindingsEmp.InvoiceRate,
                                ApprovedRate = cattfindingsEmp.ApprovedRate,
                                RateVariance = cattfindingsEmp.InvoiceRate - cattfindingsEmp.ApprovedRate,
                                InvoiceHours = cattfindingsEmp.InvoiceHours,
                                ApprovedHours = cattfindingsEmp.ApprovedHours,
                                VarianceHours = cattfindingsEmp.InvoiceHours - cattfindingsEmp.ApprovedHours,
                                Total = cattfindingsEmp.Total,
                                Notes = cattfindingsEmp.Notes
                            });
                        }
                        invoiceCAFindingsEmpChanges = eInvoiceModelcontext.DeleteInvoiceCAFindingsEmp(invoiceCAFindingsEmpChanges);

                        foreach (InvoiceCAFindingsEmp emp in cafindingsEmps)
                        {
                            emp.InvoiceCAFindingsEmpID = invoiceCAFindingsEmpChanges.FirstOrDefault().InvoiceCAFindingsEmpID;
                            emp.RateVariance = invoiceCAFindingsEmpChanges.FirstOrDefault().RateVariance;
                            emp.Total = invoiceCAFindingsEmpChanges.FirstOrDefault().Total;
                        }
                    }
                    LogManager.Debug("CAFindingsEmp_Destroy: END");
                    return Json(cafindingsEmps.ToDataSourceResult(request, ModelState));
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("CAFindingsEmp_Destroy: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        public ActionResult CATTFindingsEmp_Read([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                LogManager.Debug("CATTFindingsEmp_Read: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    //  ActiveDirectoryHelper adhelper = new ActiveDirectoryHelper();
                    InvoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    List<InvoiceCATTFindingsEmp> cattfindingsEmp = eInvoiceModelContext.GetInvoiceCATTFindingsEmp(InvoiceMasterID);
                    LogManager.Debug("CATTFindingsEmp_Read: END");
                    return Json(cattfindingsEmp.ToDataSourceResult(request));
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("CATTFindingsEmp_Read: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        [HttpPost]
        public ActionResult CATTFindingsEmp_Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]List<InvoiceCATTFindingsEmp> cattfindingsEmps)
        {
            try
            {
                LogManager.Debug("CATTFindingsEmp_Create: START");

                //if (Session["LoggedInUserName"] == null)
                //    {
                //    LogManager.Debug("CATTFindingsEmp_Create: SessionTimeOut");
                //    return RedirectToAction("SessionTimeOut","Home");
                //    //return PartialView("SessionTimeOut");
                //    }

                using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                {
                    List<InvoiceCATTFindingsEmp> invoiceCATTFindingsEmpChanges = new List<InvoiceCATTFindingsEmp>();
                    if (cattfindingsEmps != null && cattfindingsEmps.Count > 0 && ModelState.IsValid)
                    {
                        if (Session["InvoiceCATTFindingsID"] != null)
                            InvoiceCATTFindingsID = Convert.ToInt32(Session["InvoiceCATTFindingsID"]);

                        foreach (var cattfindingsEmp in cattfindingsEmps)
                        {
                            if (cattfindingsEmp.InvoiceRate == null) { cattfindingsEmp.InvoiceRate = Decimal.Zero; }
                            if (cattfindingsEmp.ApprovedRate == null) { cattfindingsEmp.ApprovedRate = Decimal.Zero; }
                            if (cattfindingsEmp.InvoiceHours == null) { cattfindingsEmp.InvoiceHours = Decimal.Zero; }
                            if (cattfindingsEmp.ApprovedHours == null) { cattfindingsEmp.ApprovedHours = Decimal.Zero; }
                            if (cattfindingsEmp.Total == null) { cattfindingsEmp.Total = Decimal.Zero; }

                            invoiceCATTFindingsEmpChanges.Add(new InvoiceCATTFindingsEmp
                            {
                                InvoiceCATTFindingsEmpID = cattfindingsEmp.InvoiceCATTFindingsEmpID,
                                InvoiceCATTFindingsID = InvoiceCATTFindingsID,
                                EmployeeName = cattfindingsEmp.EmployeeName,
                                Classification = cattfindingsEmp.Classification,
                                InvoiceRate = cattfindingsEmp.InvoiceRate,
                                ApprovedRate = cattfindingsEmp.ApprovedRate,
                                RateVariance = cattfindingsEmp.InvoiceRate - cattfindingsEmp.ApprovedRate,
                                InvoiceHours = cattfindingsEmp.InvoiceHours,
                                ApprovedHours = cattfindingsEmp.ApprovedHours,
                                VarianceHours = cattfindingsEmp.InvoiceHours - cattfindingsEmp.ApprovedHours,
                                Total = cattfindingsEmp.Total,
                                Notes = cattfindingsEmp.Notes,
                            });
                        }
                        invoiceCATTFindingsEmpChanges = eInvoiceModelcontext.SaveInvoiceCATTFindingsEmp(invoiceCATTFindingsEmpChanges);
                    }

                    foreach (InvoiceCATTFindingsEmp emp in cattfindingsEmps)
                    {
                        emp.InvoiceCATTFindingsEmpID = invoiceCATTFindingsEmpChanges.FirstOrDefault().InvoiceCATTFindingsEmpID;
                        emp.RateVariance = invoiceCATTFindingsEmpChanges.FirstOrDefault().RateVariance;
                        emp.Total = invoiceCATTFindingsEmpChanges.FirstOrDefault().Total;
                    }
                    LogManager.Debug("CATTFindingsEmp_Create: END");
                    return Json(cattfindingsEmps.ToDataSourceResult(request, ModelState));
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("CATTFindingsEmp_Create: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        [HttpPost]
        public ActionResult CATTFindingsEmp_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]List<InvoiceCATTFindingsEmp> cattfindingsEmps)
        {
            try
            {
                LogManager.Debug("CATTFindingsEmp_Update: START");
                using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                {
                    List<InvoiceCATTFindingsEmp> invoiceCATTFindingsEmpChanges = new List<InvoiceCATTFindingsEmp>();
                    if (cattfindingsEmps != null && cattfindingsEmps.Count > 0 && ModelState.IsValid)
                    {
                        if (Session["InvoiceCATTFindingsID"] != null)
                            InvoiceCATTFindingsID = Convert.ToInt32(Session["InvoiceCATTFindingsID"]);

                        foreach (var cattfindingsEmp in cattfindingsEmps)
                        {
                            if (cattfindingsEmp.InvoiceRate == null) { cattfindingsEmp.InvoiceRate = Decimal.Zero; }
                            if (cattfindingsEmp.ApprovedRate == null) { cattfindingsEmp.ApprovedRate = Decimal.Zero; }
                            if (cattfindingsEmp.InvoiceHours == null) { cattfindingsEmp.InvoiceHours = Decimal.Zero; }
                            if (cattfindingsEmp.ApprovedHours == null) { cattfindingsEmp.ApprovedHours = Decimal.Zero; }
                            if (cattfindingsEmp.Total == null) { cattfindingsEmp.Total = Decimal.Zero; }

                            if (cattfindingsEmp.InvoiceRate.Value != 0 || cattfindingsEmp.ApprovedRate.Value != 0 || cattfindingsEmp.InvoiceHours.Value != 0 || cattfindingsEmp.ApprovedHours.Value != 0)
                            {
                                decimal RateVariance = Decimal.Zero;
                                RateVariance = cattfindingsEmp.InvoiceRate.Value - cattfindingsEmp.ApprovedRate.Value;

                                if (RateVariance != 0)
                                {
                                    cattfindingsEmp.Total = RateVariance * cattfindingsEmp.InvoiceHours;
                                }
                                else if (RateVariance == 0)
                                {
                                    cattfindingsEmp.Total = (cattfindingsEmp.InvoiceHours - cattfindingsEmp.ApprovedHours) * cattfindingsEmp.ApprovedRate;
                                }
                            }

                            invoiceCATTFindingsEmpChanges.Add(new InvoiceCATTFindingsEmp
                            {
                                InvoiceCATTFindingsEmpID = cattfindingsEmp.InvoiceCATTFindingsEmpID,
                                InvoiceCATTFindingsID = InvoiceCATTFindingsID,
                                EmployeeName = cattfindingsEmp.EmployeeName,
                                Classification = cattfindingsEmp.Classification,
                                InvoiceRate = cattfindingsEmp.InvoiceRate,
                                ApprovedRate = cattfindingsEmp.ApprovedRate,
                                RateVariance = cattfindingsEmp.InvoiceRate - cattfindingsEmp.ApprovedRate,
                                InvoiceHours = cattfindingsEmp.InvoiceHours,
                                ApprovedHours = cattfindingsEmp.ApprovedHours,
                                VarianceHours = cattfindingsEmp.InvoiceHours - cattfindingsEmp.ApprovedHours,
                                Total =cattfindingsEmp.Total,
                                Notes = cattfindingsEmp.Notes,
                            });
                        }
                        invoiceCATTFindingsEmpChanges = eInvoiceModelcontext.SaveInvoiceCATTFindingsEmp(invoiceCATTFindingsEmpChanges);

                        foreach (InvoiceCATTFindingsEmp emp in cattfindingsEmps)
                        {
                            emp.InvoiceCATTFindingsEmpID = invoiceCATTFindingsEmpChanges.FirstOrDefault().InvoiceCATTFindingsEmpID;
                            emp.RateVariance = invoiceCATTFindingsEmpChanges.FirstOrDefault().RateVariance;
                            emp.Total = invoiceCATTFindingsEmpChanges.FirstOrDefault().Total;
                        }
                    }
                    LogManager.Debug("CATTFindingsEmp_Update: END");
                    return Json(cattfindingsEmps.ToDataSourceResult(request, ModelState));
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("CATTFindingsEmp_Update: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        [HttpPost]
        public ActionResult CATTFindingsEmp_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]List<InvoiceCATTFindingsEmp> cattfindingsEmps)
        {
            try
            {
                LogManager.Debug("CATTFindingsEmp_Destroy: START");
                using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                {
                    List<InvoiceCATTFindingsEmp> invoiceCATTFindingsEmpChanges = new List<InvoiceCATTFindingsEmp>();
                    if (cattfindingsEmps != null && cattfindingsEmps.Count > 0 && ModelState.IsValid)
                    {
                        if (Session["InvoiceCATTFindingsID"] != null)
                            InvoiceCATTFindingsID = Convert.ToInt32(Session["InvoiceCATTFindingsID"]);

                        foreach (var cattfindingsEmp in cattfindingsEmps)
                        {
                            if (cattfindingsEmp.Total == null) { cattfindingsEmp.Total = Decimal.Zero; }
                            invoiceCATTFindingsEmpChanges.Add(new InvoiceCATTFindingsEmp
                            {
                                InvoiceCATTFindingsEmpID = cattfindingsEmp.InvoiceCATTFindingsEmpID,
                                InvoiceCATTFindingsID = InvoiceCATTFindingsID,
                                EmployeeName = cattfindingsEmp.EmployeeName,
                                Classification = cattfindingsEmp.Classification,
                                InvoiceRate = cattfindingsEmp.InvoiceRate,
                                ApprovedRate = cattfindingsEmp.ApprovedRate,
                                RateVariance = cattfindingsEmp.InvoiceRate - cattfindingsEmp.ApprovedRate,
                                InvoiceHours = cattfindingsEmp.InvoiceHours,
                                ApprovedHours = cattfindingsEmp.ApprovedHours,
                                VarianceHours = cattfindingsEmp.InvoiceHours - cattfindingsEmp.ApprovedHours,
                                Total = cattfindingsEmp.Total,
                                Notes = cattfindingsEmp.Notes
                            });
                        }
                        invoiceCATTFindingsEmpChanges = eInvoiceModelcontext.DeleteInvoiceCATTFindingsEmp(invoiceCATTFindingsEmpChanges);

                        foreach (InvoiceCATTFindingsEmp emp in cattfindingsEmps)
                        {
                            emp.InvoiceCATTFindingsEmpID = invoiceCATTFindingsEmpChanges.FirstOrDefault().InvoiceCATTFindingsEmpID;
                            emp.RateVariance = invoiceCATTFindingsEmpChanges.FirstOrDefault().RateVariance;
                            emp.Total = invoiceCATTFindingsEmpChanges.FirstOrDefault().Total;
                        }
                    }
                    LogManager.Debug("CATTFindingsEmp_Destroy: END");
                    return Json(cattfindingsEmps.ToDataSourceResult(request, ModelState));
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("CATTFindingsEmp_Destroy: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        public JsonResult UpdateInvoiceCATTFindings(InvoiceCATTFindings cattfind)
        {
            try
            {
                LogManager.Debug("UpdateInvoiceCATTFindings: START");
                using (var context = new eInvoiceModelContext())
                {
                    if (Session["InvoiceMasterID"] != null)
                        InvoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    if (Session["InvoiceCATTFindingsID"] != null)
                        InvoiceCATTFindingsID = Convert.ToInt32(Session["InvoiceCATTFindingsID"]);
                    InvoiceCATTFindings cattFind = new InvoiceCATTFindings();
                    cattFind.InvoiceCATTFindingsID = InvoiceCATTFindingsID;
                    cattFind.AddressedTo = cattfind.AddressedTo;
                    cattFind.SentFrom = cattfind.SentFrom;
                    cattFind.Date = cattfind.Date;
                    cattFind.CATTRecommendedAdjustment = cattfind.CATTRecommendedAdjustment;
                    cattFind.CARecommendedAdjustment = cattfind.CARecommendedAdjustment;
                    cattFind.CANotes = cattfind.CANotes;
                    cattFind.CATTNotes = cattfind.CATTNotes;
                    cattFind.InvoiceMasterID = InvoiceMasterID;
                    InvoiceCATTFindings result = context.UpdateInvoiceCATTFindings(cattFind);
                }

                LogManager.Debug("UpdateInvoiceCATTFindings: END");
                return Json(cattfind, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error("UpdateInvoiceCATTFindings: ERROR " + ex.Message, ex);
                return null;
            }
        }
        
    }
}
    
 