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

namespace eInvoiceAutomationWeb.Controllers
{
    public class CATTFindingsControllerADCopy : Controller
    {
        int invoiceMasterID;
        int InvoiceCATTFindingsID;

        CATTFindingsViewModel cattfindingsViewModel;

        public ActionResult CATTFinding(string documentNo, string status, bool ReadOnly)
        {
            using (var eInvoiceModelContext = new eInvoiceModelContext())
            {
                string LoggedUserCATTorCA = string.Empty;
                cattfindingsViewModel = new CATTFindingsViewModel();
                Session["InvoiceMasterID"] = eInvoiceModelContext.GetInvoiceMasterID(documentNo);
                invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                cattfindingsViewModel.DocumentNo = documentNo;
                cattfindingsViewModel.RoutingDetails = new RoutingDetailsViewModel();
                //Get Routing Header
                cattfindingsViewModel.RoutingDetails.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);

                //Get InvoiceCATTFindings..
                cattfindingsViewModel.InvoiceCATTFindings = eInvoiceModelContext.GetInvoiceCATTFindings(invoiceMasterID);
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
                    cattfindingsViewModel.AssetPayment = 0;
                }
                if (cattfindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment.HasValue)
                    cattfindingsViewModel.ApprovedPayment = invoiceAmt - cattfindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment.Value;
                else
                {
                    cattfindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment = 0;
                    cattfindingsViewModel.ApprovedPayment = 0;
                }

                //If not ReadOnly try Load editable view..
                if (!ReadOnly)
                {
                    string InvoiceStatus = eInvoiceModelContext.GetStatus(invoiceMasterID);
                   
                    if (InvoiceStatus == "CATT Review")
                    {
                        //if Logged in User Role is CATT
                        LoggedUserCATTorCA = GetUserRoleName(0, "CATT");
                        cattfindingsViewModel.LoggedinUserType = LoggedUserCATTorCA;
                    }
                    if (InvoiceStatus == "CA Review")
                    {
                        //if Logged in User Role is CA
                        LoggedUserCATTorCA = GetUserRoleName(invoiceMasterID, "CA");
                        cattfindingsViewModel.LoggedinUserType = LoggedUserCATTorCA;
                    }
                }

                //if Logged in User (CATT or CA) was found for CATT Review or CA Review..Load Editable View..else ReadOnly
                if (LoggedUserCATTorCA.Length > 0)
                {
                    return PartialView("_CATTFindings", cattfindingsViewModel);
                }
                else
                {
                    cattfindingsViewModel.ToCACSV = BuildEmployeeCSV(cattfindingsViewModel.ToCA);
                    cattfindingsViewModel.FromCATTCSV = BuildEmployeeCSV(cattfindingsViewModel.FromCATT);
                    return PartialView("_CATTFindingsReadOnly", cattfindingsViewModel);
                }
            }
        }

        private string BuildEmployeeCSV(IEnumerable<ExchangeEmployeeProfile> employees)
        {
            string empcsv = string.Empty;
            foreach (ExchangeEmployeeProfile sepCATT in employees)
            {
                empcsv = empcsv + (string.IsNullOrEmpty(empcsv) ? sepCATT.ApproverName : ", " + sepCATT.ApproverName);
            }
            return empcsv;
        }

        private string GetUserRoleName(int InvoiceMasterID, string RoleName)
        {
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
                        return RoleName;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            return string.Empty;
        }

        public JsonResult GetADUsers(string text)
        {
            SAPSourceModelContext SAPmodel = new SAPSourceModelContext();
            List<SAPEmployeeProfile> selectedSAPEmp = new List<SAPEmployeeProfile>();
            if (!string.IsNullOrEmpty(text.Trim()))
            {
                ActiveDirectoryHelper adhelper = new ActiveDirectoryHelper();
                List<ADUserDetail> adUserList = adhelper.GetUsersByFirstName(text);
                foreach (ADUserDetail aduser in adUserList)
                {
                    SAPEmployeeProfile employee = new SAPEmployeeProfile();
                    employee.UserID = aduser.LoginName;
                    employee.FirstName = aduser.FirstName;
                    employee.LastName = aduser.LastName;
                    selectedSAPEmp.Add(employee);
                }
            }
            else
            {
                SAPEmployeeProfile empty = new SAPEmployeeProfile();
                empty.UserID = "";
                empty.FirstName = "";
                empty.LastName = "";
                List<SAPEmployeeProfile> emptyList = new List<SAPEmployeeProfile>();
                emptyList.Add(empty);
                selectedSAPEmp = emptyList;
            }
            return Json(selectedSAPEmp, JsonRequestBehavior.AllowGet);
        }


        public JsonResult FetchApprovers(string text)
        {
            SAPSourceModelContext SAPmodel = new SAPSourceModelContext();
            IEnumerable<SAPEmployeeProfile> selectedSAPEmp = null;
          //  if (!string.IsNullOrEmpty(text.Trim()))
          //  {
                selectedSAPEmp = SAPmodel.FetchApproversList();
          //  }
            //else
            //{
            //    SAPEmployeeProfile empty = new SAPEmployeeProfile();
            //    empty.UserID = "";
            //    empty.FirstName = "";
            //    empty.LastName = "";
            //    List<SAPEmployeeProfile> emptyList = new List<SAPEmployeeProfile>();
            //    emptyList.Add(empty);
            //    selectedSAPEmp = emptyList;
            //}
            return Json(selectedSAPEmp, JsonRequestBehavior.AllowGet);
        }

        private string GetADDisplayName(string userID)
        {
            ActiveDirectoryHelper adhelper = new ActiveDirectoryHelper();
            ADUserDetail aduser = adhelper.GetUserByLoginName(userID);
            if (aduser != null)
                return aduser.FirstName + " " + aduser.LastName;
            else
                return string.Empty;
        }

        public ActionResult CATTFindingsEmp_Read([DataSourceRequest] DataSourceRequest request)
        {
            using (var eInvoiceModelContext = new eInvoiceModelContext())
            {
                ActiveDirectoryHelper adhelper = new ActiveDirectoryHelper();
                invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                List<InvoiceCATTFindingsEmp> cattfindingsEmp = eInvoiceModelContext.GetInvoiceCATTFindingsEmp(invoiceMasterID);
                //Get Display Name from AD..
                foreach (InvoiceCATTFindingsEmp cattemp in cattfindingsEmp)
                {
                 //   if (string.IsNullOrEmpty(cattemp.EmployeeUserID) == false)
                   ///     cattemp.EmployeeName = GetADDisplayName(cattemp.EmployeeUserID);
                }
                return Json(cattfindingsEmp.ToDataSourceResult(request));
            }
        }

       [HttpPost]
        public ActionResult CATTFindingsEmp_Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]List<InvoiceCATTFindingsEmp> cattfindingsEmps)
        {
            List<InvoiceCATTFindingsEmp> invoiceCATTFindingsEmpChanges = null;
            if (cattfindingsEmps != null && cattfindingsEmps.Count > 0)
            {
                if (Session["InvoiceCATTFindingsID"] != null)
                    InvoiceCATTFindingsID = Convert.ToInt32(Session["InvoiceCATTFindingsID"]);

                using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                {
                    invoiceCATTFindingsEmpChanges = new List<InvoiceCATTFindingsEmp>();
                    foreach (var cattfindingsEmp in cattfindingsEmps)
                    {
                        invoiceCATTFindingsEmpChanges.Add(new InvoiceCATTFindingsEmp
                        {
                            InvoiceCATTFindingsEmpID = cattfindingsEmp.InvoiceCATTFindingsEmpID,
                            InvoiceCATTFindingsID = InvoiceCATTFindingsID,
                          //  EmployeeUserID = string.Empty,
                            EmployeeName = cattfindingsEmp.EmployeeName,
                            Classification = cattfindingsEmp.Classification,
                            InvoiceRate = cattfindingsEmp.InvoiceRate,
                            ApprovedRate = cattfindingsEmp.ApprovedRate,
                            RateVariance = cattfindingsEmp.RateVariance,
                            InvoiceHours = cattfindingsEmp.InvoiceHours,
                            ApprovedHours = cattfindingsEmp.ApprovedHours,
                            VarianceHours = cattfindingsEmp.VarianceHours
                        });
                    }
                    invoiceCATTFindingsEmpChanges = eInvoiceModelcontext.SaveInvoiceCATTFindingsEmp(invoiceCATTFindingsEmpChanges);
                    //Get Display Name from AD..
                    foreach (InvoiceCATTFindingsEmp cattemp in invoiceCATTFindingsEmpChanges)
                    {
                       // if (string.IsNullOrEmpty(cattemp.EmployeeUserID) == false)
                          //  cattemp.EmployeeName = GetADDisplayName(cattemp.EmployeeUserID);
                    }
                }

                foreach (InvoiceCATTFindingsEmp emp in cattfindingsEmps)
                {
                    emp.InvoiceCATTFindingsEmpID = invoiceCATTFindingsEmpChanges.FirstOrDefault().InvoiceCATTFindingsEmpID;
                    emp.RateVariance = invoiceCATTFindingsEmpChanges.FirstOrDefault().RateVariance;
                    emp.VarianceHours = invoiceCATTFindingsEmpChanges.FirstOrDefault().VarianceHours;

                }

                return Json(cattfindingsEmps.ToDataSourceResult(request, ModelState));
            }
            else
                return Json(cattfindingsEmps.ToDataSourceResult(request, ModelState));
        }

           [HttpPost]
        public ActionResult CATTFindingsEmp_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]List<InvoiceCATTFindingsEmp> cattfindingsEmps)
        {
            List<InvoiceCATTFindingsEmp> invoiceCATTFindingsEmpChanges = null;
            if (cattfindingsEmps != null && cattfindingsEmps.Count > 0 && ModelState.IsValid)
            {
                if (Session["InvoiceCATTFindingsID"] != null)
                    InvoiceCATTFindingsID = Convert.ToInt32(Session["InvoiceCATTFindingsID"]);

                using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                {
                    invoiceCATTFindingsEmpChanges = new List<InvoiceCATTFindingsEmp>();
                    foreach (var cattfindingsEmp in cattfindingsEmps)
                    {
                        invoiceCATTFindingsEmpChanges.Add(new InvoiceCATTFindingsEmp
                        {
                            InvoiceCATTFindingsEmpID = cattfindingsEmp.InvoiceCATTFindingsEmpID,
                            InvoiceCATTFindingsID = InvoiceCATTFindingsID,
                        //    EmployeeUserID = (cattfindingsEmp.EmployeeUserID == null ? string.Empty : cattfindingsEmp.EmployeeUserID),
                            EmployeeName = cattfindingsEmp.EmployeeName,
                            Classification = cattfindingsEmp.Classification,
                            InvoiceRate = cattfindingsEmp.InvoiceRate,
                            ApprovedRate = cattfindingsEmp.ApprovedRate,
                            RateVariance = cattfindingsEmp.InvoiceRate - cattfindingsEmp.ApprovedRate,
                            InvoiceHours = cattfindingsEmp.InvoiceHours,
                            ApprovedHours = cattfindingsEmp.ApprovedHours,
                            VarianceHours = cattfindingsEmp.InvoiceHours - cattfindingsEmp.ApprovedHours
                        });
                    }
                    invoiceCATTFindingsEmpChanges = eInvoiceModelcontext.SaveInvoiceCATTFindingsEmp(invoiceCATTFindingsEmpChanges);
                    //Get Display Name from AD..
                    foreach (InvoiceCATTFindingsEmp cattemp in invoiceCATTFindingsEmpChanges)
                    {
                     //   if (string.IsNullOrEmpty(cattemp.EmployeeUserID) == false)
                         //   cattemp.EmployeeName = GetADDisplayName(cattemp.EmployeeUserID);
                    }
                }
                return Json(invoiceCATTFindingsEmpChanges.ToDataSourceResult(request, ModelState));
            }
            else
                return Json(cattfindingsEmps.ToDataSourceResult(request, ModelState));
        }

          [HttpPost]
        public ActionResult CATTFindingsEmp_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]List<InvoiceCATTFindingsEmp> cattfindingsEmps)
        {
            List<InvoiceCATTFindingsEmp> invoiceCATTFindingsEmpChanges = null;
            if (cattfindingsEmps != null && cattfindingsEmps.Count > 0 && ModelState.IsValid)
            {
                if (Session["InvoiceCATTFindingsID"] != null)
                    InvoiceCATTFindingsID = Convert.ToInt32(Session["InvoiceCATTFindingsID"]);

                using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                {
                    invoiceCATTFindingsEmpChanges = new List<InvoiceCATTFindingsEmp>();
                    foreach (var cattfindingsEmp in cattfindingsEmps)
                    {
                        invoiceCATTFindingsEmpChanges.Add(new InvoiceCATTFindingsEmp
                        {
                            InvoiceCATTFindingsEmpID = cattfindingsEmp.InvoiceCATTFindingsEmpID,
                            InvoiceCATTFindingsID = InvoiceCATTFindingsID,
                         //   EmployeeUserID = cattfindingsEmp.EmployeeUserID,
                            EmployeeName = cattfindingsEmp.EmployeeName,
                            Classification = cattfindingsEmp.Classification,
                            InvoiceRate = cattfindingsEmp.InvoiceRate,
                            ApprovedRate = cattfindingsEmp.ApprovedRate,
                            RateVariance = cattfindingsEmp.InvoiceRate - cattfindingsEmp.ApprovedRate,
                            InvoiceHours = cattfindingsEmp.InvoiceHours,
                            ApprovedHours = cattfindingsEmp.ApprovedHours,
                            VarianceHours = cattfindingsEmp.InvoiceHours - cattfindingsEmp.ApprovedHours
                        });
                    }
                    invoiceCATTFindingsEmpChanges = eInvoiceModelcontext.DeleteInvoiceCATTFindingsEmp(invoiceCATTFindingsEmpChanges);
                    //Get Display Name from AD..
                    foreach (InvoiceCATTFindingsEmp cattemp in invoiceCATTFindingsEmpChanges)
                    {
                      //  if (string.IsNullOrEmpty(cattemp.EmployeeUserID) == false)
                       //     cattemp.EmployeeName = GetADDisplayName(cattemp.EmployeeUserID);
                    }
                }
                return Json(invoiceCATTFindingsEmpChanges.ToDataSourceResult(request, ModelState));
            }
            else
                return Json(cattfindingsEmps.ToDataSourceResult(request, ModelState));
        }
       
        public JsonResult UpdateInvoiceCATTFindings(InvoiceCATTFindings cattfind)
        {
            using (var context = new eInvoiceModelContext())
            {
                if (Session["InvoiceMasterID"] != null)
                    invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
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
                cattFind.InvoiceMasterID = invoiceMasterID;
                InvoiceCATTFindings result = context.UpdateInvoiceCATTFindings(cattFind);
            }
            return Json(cattfind, JsonRequestBehavior.AllowGet);
        }
    }
}