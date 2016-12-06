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
using System.Globalization;
using System.Web.UI;


namespace eInvoiceAutomationWeb.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None)]
    [SessionTimeOutFilter]
    public class PurchaseOrderDetailsController : Controller
    {
        private static readonly log4net.ILog LogManager = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        int invoiceMasterID;
        PurchaseOrderDetailsViewModel purchaseOrderDetailsViewModel;

        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult PODetails(string documentNo, string status, string SN, bool ReadOnly, int defaultInvoiceMasterID = 0)
        {
            try
            {
                LogManager.Debug("PODetails: START");
                               
                //// check if session has timed out here
                //if (Session["LoggedInUserName"] == null)
                //    {
                //        return PartialView("SessionTimeOut");
                //    //return RedirectToAction("~/Home/SessionTimeOut");
                //        //RedirectResult("~/Home/SessionTimeOut");
                //    }

                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    purchaseOrderDetailsViewModel = new PurchaseOrderDetailsViewModel();

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
                        purchaseOrderDetailsViewModel.DocumentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(invoiceMasterID);
                    }
                    else
                    {
                        if (Session["InvoiceMasterID"] != null)
                        {
                            invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                            purchaseOrderDetailsViewModel.DocumentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(invoiceMasterID);
                        }
                    }
                    if (Session["InvoiceMasterID"] != null)
                    {
                        TempData["DocumentNo"] = purchaseOrderDetailsViewModel.DocumentNo;
                        purchaseOrderDetailsViewModel.RoutingDetails = new RoutingDetailsViewModel();
                        purchaseOrderDetailsViewModel.RoutingDetails.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);
                        purchaseOrderDetailsViewModel.InvoicePODetails = eInvoiceModelContext.GetInvoicePODetails(invoiceMasterID);
                        purchaseOrderDetailsViewModel.ShowModifyFlag = eInvoiceModelContext.GetInvoicePODetailsModifyFlag(invoiceMasterID);
                        ViewBag.SAPPONumbers = GetSAPPONumbers();
                        ViewBag.SAPCostCenterNo = GetSAPCostCenterNos();
                        ViewBag.SAPInternalOrder = GetSAPInternalOrder();
                        purchaseOrderDetailsViewModel.ShowPOWarning = eInvoiceModelContext.POValidateEndValidityDate(invoiceMasterID);
                        if (ReadOnly == false)
                        {
                            switch (status)
                            {
                                case "CATT Review":
                                case "CA Review":
                                case "PC Review":
                                case "OA Review":
                                    LogManager.Debug("PODetails: END");
                                    return PartialView("_PurchaseOrderDetails", purchaseOrderDetailsViewModel);
                                default:
                                    LogManager.Debug("PODetails: END");
                                    return PartialView("_PurchaseOrderDetailsReadOnly", purchaseOrderDetailsViewModel);
                            }
                        }
                        else
                        {
                            LogManager.Debug("PODetails: END");
                            return PartialView("_PurchaseOrderDetailsReadOnly", purchaseOrderDetailsViewModel);
                        }
                    }
                    else
                    {
                        TempData["DocumentNo"] = null;
                        ViewBag.ErrorMessage = "Either Document Number is not valid or a process may have already been started for this Document Number;Check your Dashboard for Saved Invoices.";
                        return PartialView("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("PODetails: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        
        public ActionResult DisplayLogHistory()
        {
            try
            {
                LogManager.Debug("DisplayLogHistory: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    if (Session["InvoiceMasterID"] != null)
                       invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    purchaseOrderDetailsViewModel = new PurchaseOrderDetailsViewModel();
                    purchaseOrderDetailsViewModel.InvoicePODetailChangesLog = eInvoiceModelContext.GetInvoicePODetailChangesLog(invoiceMasterID);
                }
                LogManager.Debug("DisplayLogHistory: END");
                return PartialView("_DisplayLogHistory", purchaseOrderDetailsViewModel);
            }
            catch (Exception ex)
            {
                LogManager.Error("DisplayLogHistory: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        public ActionResult AccountingCostCodes_Read([DataSourceRequest]DataSourceRequest request)
        {
            try
            { 
                LogManager.Debug("AccountingCostCodes_Read: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    purchaseOrderDetailsViewModel = new PurchaseOrderDetailsViewModel();
                    purchaseOrderDetailsViewModel.InvoicePODetails = eInvoiceModelContext.GetInvoicePODetails(invoiceMasterID);
                    DataSourceResult result = new DataSourceResult();
                    result = purchaseOrderDetailsViewModel.InvoicePODetails.ToDataSourceResult(request, p => new AccountingCostCodesViewModel
                    {
                        InvoiceDetailID = p.InvoiceDetailID,
                        InvoiceMasterID = p.InvoiceMasterID,
                        PONumber = (p.PONumber ?? "").TrimStart('0'),
                        POLine = p.POLine,
                        GLAccount = p.GLAccount,
                        CostCenter = p.CostCenter,
                        WBS = p.WBS,
                        Fund = p.Fund,
                        FunctionalArea = p.FunctionalArea,
                        GrantNumber = p.GrantNumber,
                        InternalOrder = p.InternalOrder,
                        InvoiceAmount = p.InvoiceAmount
                    });

                    LogManager.Debug("AccountingCostCodes_Read: END");
                    return Json(result);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("AccountingCostCodes_Read: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        public ActionResult ModifyAccountingCostCodes_Read([DataSourceRequest]DataSourceRequest request)
        {
            try
            { 
                 LogManager.Debug("ModifyAccountingCostCodes_Read: START");

                 using (var eInvoiceModelContext = new eInvoiceModelContext())
                 {
                     if (Session["InvoiceMasterID"] != null)
                         invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                     string status = eInvoiceModelContext.GetStatus(invoiceMasterID);
                     purchaseOrderDetailsViewModel = new PurchaseOrderDetailsViewModel();
                     purchaseOrderDetailsViewModel.InvoicePODetailChanges = eInvoiceModelContext.GetInvoicePODetailChanges(invoiceMasterID);
                     DataSourceResult result = new DataSourceResult();
                     if (status == "CATT Review" || status == "CA Review" || status == "PC Review" || status == "OA Review")
                     {
                         result = purchaseOrderDetailsViewModel.InvoicePODetailChanges.ToDataSourceResult(request, p => new ModifyAccountingCostCodesViewModel
                         {
                             InvoiceDetailChangesID = p.InvoiceDetailChangesID,
                             InvoiceMasterID = p.InvoiceMasterID,
                             SAPPONumber = p.PONumber,
                             SAPPOLine = p.POLine,
                             GLAccount = p.GLAccount,
                             CostCenter = p.CostCenter,
                             WBSNo = p.WBS,
                             Fund = p.Fund,
                             FunctionalArea = p.FunctionalArea,
                             GrantNumber = p.GrantNumber,
                             InternalOrder = p.InternalOrder,
                             InvoiceAmount = (p.InvoiceAmount == null ? Decimal.Zero : p.InvoiceAmount),
                             Notes = p.Notes,
                             EditedFlag = (p.EditedFlag == null ? "" : p.EditedFlag)
                         });
                     }
                     else
                     {
                         result = purchaseOrderDetailsViewModel.InvoicePODetailChanges.ToDataSourceResult(request, p => new ModifyAccountingCostCodesViewModel
                         {
                             InvoiceDetailChangesID = p.InvoiceDetailChangesID,
                             InvoiceMasterID = p.InvoiceMasterID,
                             SAPPONumber = (p.PONumber ?? "").TrimStart('0'),
                             SAPPOLine = p.POLine,
                             GLAccount = p.GLAccount,
                             CostCenter = p.CostCenter,
                             WBSNo = p.WBS,
                             Fund = p.Fund,
                             FunctionalArea = p.FunctionalArea,
                             GrantNumber = p.GrantNumber,
                             InternalOrder = p.InternalOrder,
                             InvoiceAmount = (p.InvoiceAmount == null ? Decimal.Zero : p.InvoiceAmount),
                             Notes = p.Notes,
                             EditedFlag = (p.EditedFlag == null ? "" : p.EditedFlag)
                         });
                     }

                     LogManager.Debug("ModifyAccountingCostCodes_Read: END");
                     return Json(result);
                 }
            }
            catch (Exception ex)
            {
                LogManager.Error("ModifyAccountingCostCodes_Read: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        [HttpPost]
        public ActionResult ModifyAccountingCostCodes_Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ModifyAccountingCostCodesViewModel> modifyCostCodes)
        {
            try
            { 
                 LogManager.Debug("ModifyAccountingCostCodes_Create: START");
                 using (var eInvoiceModelcontext = new eInvoiceModelContext())
                 {
                     if (Session["InvoiceMasterID"] != null)
                         invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                     IEnumerable<InvoicePODetailChanges> result = null;
                     List<InvoicePODetailChanges> invoicePODetailChanges = new List<InvoicePODetailChanges>();
                     if (modifyCostCodes != null && ModelState.IsValid)
                     {
                         foreach (var costCode in modifyCostCodes)
                         {
                             costCode.EditedFlag = "Yes";

                             if (costCode.SAPPONumber == " ")
                                costCode.SAPPOLine = 0;
                             invoicePODetailChanges.Add(new InvoicePODetailChanges
                             {
                                 InvoiceMasterID = (costCode.InvoiceMasterID == 0 ? invoiceMasterID : costCode.InvoiceMasterID),
                                 PONumber = costCode.SAPPONumber,
                                 POLine = costCode.SAPPOLine,
                                 CostCenter = costCode.CostCenter,
                                 WBS = costCode.WBSNo,
                                 Fund = costCode.Fund,
                                 FunctionalArea = costCode.FunctionalArea,
                                 GrantNumber = costCode.GrantNumber,
                                 InternalOrder = costCode.InternalOrder,
                                 InvoiceAmount = (costCode.InvoiceAmount == null ? Decimal.Zero : costCode.InvoiceAmount),
                                 Notes = costCode.Notes,
                                 EditedByUserID = RemoveDomainName(),
                                 EditedByDate = DateTime.Now,
                                 EditedFlag = costCode.EditedFlag
                             });
                         }
                         result = eInvoiceModelcontext.SaveAccountingCostCodes(invoicePODetailChanges);
                         foreach (var m in modifyCostCodes)
                         {
                             var code = (from r in result
                                         where r.InvoiceMasterID == invoiceMasterID && r.POLine == m.SAPPOLine && r.PONumber == m.SAPPONumber && ((r.InvoiceAmount == null ? Decimal.Zero : r.InvoiceAmount) == (m.InvoiceAmount == null ? Decimal.Zero : m.InvoiceAmount)) && r.InternalOrder == m.InternalOrder && r.CostCenter == m.CostCenter && r.WBS == m.WBSNo && r.FunctionalArea == m.FunctionalArea && r.Fund == m.Fund
                                             && r.GrantNumber == m.GrantNumber && r.Notes == m.Notes select r).FirstOrDefault();
                             if (code != null)
                             {
                                 m.InvoiceDetailChangesID = code.InvoiceDetailChangesID;
                                 m.GLAccount = code.GLAccount;
                                 if (m.InvoiceAmount == null)
                                   m.InvoiceAmount = Decimal.Zero;
                             }
                         }
                     }
                     LogManager.Debug("ModifyAccountingCostCodes_Create: END");
                     return Json(modifyCostCodes.ToDataSourceResult(request, ModelState));
                 }
            }
            catch (Exception ex)
            {
                LogManager.Error("ModifyAccountingCostCodes_Create: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        [HttpPost]
        public ActionResult ModifyAccountingCostCodes_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ModifyAccountingCostCodesViewModel> modifyCostCodes)
        {
            try
            { 
                 LogManager.Debug("ModifyAccountingCostCodes_Update: START");
                 using (var eInvoiceModelcontext = new eInvoiceModelContext())
                 {
                     if (Session["InvoiceMasterID"] != null)
                       invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                     IEnumerable<InvoicePODetailChanges> result = null;
                     List<InvoicePODetailChanges> invoicePODetailChanges = new List<InvoicePODetailChanges>();
                     if (modifyCostCodes != null && ModelState.IsValid)
                     {
                         foreach (var costCode in modifyCostCodes)
                         {
                             costCode.EditedFlag = "Yes";

                             if (costCode.SAPPONumber == " ")
                                costCode.SAPPOLine = 0;
                             InvoicePODetailChanges poDetailChange = eInvoiceModelcontext.GetInvoicePODetailChangeProperties(costCode.InvoiceDetailChangesID);
                             string oldWbs = poDetailChange.WBS;
                             int? oldCostCenter = poDetailChange.CostCenter;
                             if (oldWbs != costCode.WBSNo)
                             {
                                 costCode.CostCenter = null;
                                 costCode.GLAccount = 570000;
                             }
                             else if (oldCostCenter != costCode.CostCenter)
                             {
                                 costCode.WBSNo = null;
                                 costCode.GLAccount = null;
                             }
                             
                             invoicePODetailChanges.Add(new InvoicePODetailChanges
                             {
                                 InvoiceDetailChangesID = costCode.InvoiceDetailChangesID,
                                 InvoiceMasterID = (costCode.InvoiceMasterID == 0 ? invoiceMasterID : costCode.InvoiceMasterID),
                                 PONumber = costCode.SAPPONumber,
                                 POLine = costCode.SAPPOLine,
                                 CostCenter = costCode.CostCenter,
                                 WBS = costCode.WBSNo,
                                 GLAccount = costCode.GLAccount,
                                 Fund = costCode.Fund,
                                 FunctionalArea = costCode.FunctionalArea,
                                 GrantNumber = costCode.GrantNumber,
                                 InternalOrder = costCode.InternalOrder,
                                 InvoiceAmount = (costCode.InvoiceAmount == null ? Decimal.Zero : costCode.InvoiceAmount),
                                 Notes = costCode.Notes,
                                 EditedByUserID = RemoveDomainName(),
                                 EditedByDate = DateTime.Now,
                                 EditedFlag = costCode.EditedFlag
                             });
                        }
                       result = eInvoiceModelcontext.SaveAccountingCostCodes(invoicePODetailChanges);
                       foreach (var m in modifyCostCodes)
                       {
                             var code = (from r in result where r.InvoiceDetailChangesID == m.InvoiceDetailChangesID select r).FirstOrDefault();
                             m.GLAccount = code.GLAccount;
                             if (m.InvoiceAmount == null)
                                m.InvoiceAmount = Decimal.Zero;
                       }
                   }
                   LogManager.Debug("ModifyAccountingCostCodes_Update: END");
                   return Json(modifyCostCodes.ToDataSourceResult(request, ModelState));
               }
            }
            catch (Exception ex)
            {
                LogManager.Error("ModifyAccountingCostCodes_Update: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        [HttpPost]
        public ActionResult ModifyAccountingCostCodes_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ModifyAccountingCostCodesViewModel> modifyCostCodes)
        {
            try
            {
                LogManager.Debug("ModifyAccountingCostCodes_Destroy: START");
                using (var eInvoiceModelcontext = new eInvoiceModelContext())
                {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    List<InvoicePODetailChanges> invoicePODetailChanges = new List<InvoicePODetailChanges>();
                    if (modifyCostCodes != null && ModelState.IsValid)
                    {
                        foreach (var costCode in modifyCostCodes)
                        {
                            invoicePODetailChanges.Add(new InvoicePODetailChanges
                            {
                                InvoiceDetailChangesID = costCode.InvoiceDetailChangesID,
                                InvoiceMasterID = (costCode.InvoiceMasterID == 0 ? invoiceMasterID : costCode.InvoiceMasterID),
                                PONumber = costCode.SAPPONumber,
                                POLine = costCode.SAPPOLine,
                                GLAccount = costCode.GLAccount,
                                CostCenter = costCode.CostCenter,
                                WBS = costCode.WBSNo,
                                Fund = costCode.Fund,
                                FunctionalArea = costCode.FunctionalArea,
                                GrantNumber = costCode.GrantNumber,
                                InternalOrder = costCode.InternalOrder,
                                InvoiceAmount = (costCode.InvoiceAmount == null ? Decimal.Zero : costCode.InvoiceAmount),
                                Notes = costCode.Notes,
                                EditedByUserID = RemoveDomainName(),
                                EditedByDate = DateTime.Now,
                                EditedFlag = costCode.EditedFlag
                            });
                        }
                        eInvoiceModelcontext.DeleteAccountingCostCodes(invoicePODetailChanges);
                    }
                    LogManager.Debug("ModifyAccountingCostCodes_Destroy: END");
                    return Json(modifyCostCodes.ToDataSourceResult(request, ModelState));
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("ModifyAccountingCostCodes_Destroy: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        [HttpPost]
        public ActionResult AccountingCostCodesChangeHistory_Read([DataSourceRequest]DataSourceRequest request)
        {
            try
            {
                LogManager.Debug("AccountingCostCodesChangeHistory_Read: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    purchaseOrderDetailsViewModel = new PurchaseOrderDetailsViewModel();
                    purchaseOrderDetailsViewModel.InvoicePODetailChangesLog = eInvoiceModelContext.GetInvoicePODetailChangesLog(invoiceMasterID);
                    DataSourceResult result = new DataSourceResult();
                    result = purchaseOrderDetailsViewModel.InvoicePODetailChangesLog.ToDataSourceResult(request, p => new AccountingCostCodesChangeHistoryViewModel
                    {
                        LogID = p.LogID,
                        InvoiceDetailChangesID = p.InvoiceDetailChangesID,
                        InvoiceMasterID = p.InvoiceMasterID,
                        PONumber = (p.PONumber ?? "").TrimStart('0'),
                        POLine = p.POLine,
                        GLAccount = p.GLAccount,
                        CostCenter = p.CostCenter,
                        WBS = p.WBS,
                        Fund = p.Fund,
                        FunctionalArea = p.FunctionalArea,
                        GrantNumber = p.GrantNumber,
                        InternalOrder = p.InternalOrder,
                        InvoiceAmount = p.InvoiceAmount,
                        Notes = p.Notes,
                        LogAction = p.LogAction,
                        EditedByUserID = p.EditedByUserID,
                        EditedByDate = p.EditedByDate.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture)
                    });
                    LogManager.Debug("AccountingCostCodesChangeHistory_Read: END");
                    return Json(result);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("AccountingCostCodesChangeHistory_Read: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        [HttpPost]
        public ActionResult ModifyAccountingCostCodes_CreateUpdateDelete(
            [Bind(Prefix = "updated")]List<ModifyAccountingCostCodesViewModel> updatedCostCodes,
            [Bind(Prefix = "new")]List<ModifyAccountingCostCodesViewModel> newCostCodes,
            [Bind(Prefix = "deleted")]List<ModifyAccountingCostCodesViewModel> deletedCostCodes)
        {
            try
            {
                LogManager.Debug("ModifyAccountingCostCodes_CreateUpdateDelete: START");
                SaveAccountingCostCodesGrid(updatedCostCodes, newCostCodes, deletedCostCodes);
                LogManager.Debug("ModifyAccountingCostCodes_CreateUpdateDelete: END");
                return Json("Success!");
            }
            catch (Exception ex)
            {
                LogManager.Error("ModifyAccountingCostCodes_CreateUpdateDelete: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        public JsonResult SaveSESNumber(string SESNumber)
        {
            try
            {
                LogManager.Debug("SaveSESNumber: START");
                using (var context = new eInvoiceModelContext())
                {
                    if (Session["InvoiceMasterID"] != null)
                      invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    context.UpdateSESNumber(invoiceMasterID, SESNumber);
                }
                LogManager.Debug("SaveSESNumber: END");
                return Json(SESNumber, JsonRequestBehavior.AllowGet);
            }


            catch (Exception ex)
            {
                LogManager.Error("SaveSESNumber: ERROR " + ex.Message, ex);
                return null;
            }
        }


        #region Methods

        public string RemoveDomainName()
        {
            try
            {
                LogManager.Debug("RemoveDomainName: START");
                string loggedInUserId = HttpContext.User.Identity.Name.ToString();
                int index = loggedInUserId.IndexOf('\\');
                string userId = loggedInUserId.Substring(index + 1);

                LogManager.Debug("RemoveDomainName: END");

                return userId;
            }
            catch (Exception ex)
            {
                LogManager.Error("RemoveDomainName: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public List<SAPPurchaseOrder> GetSAPPONumbers()
        {
            try
            {
                LogManager.Debug("GetSAPPONumbers: START");
                List<SAPPurchaseOrder> SAPPurchaseOrders = new List<SAPPurchaseOrder>();
                List<SAPPurchaseOrder> PONumbers = new List<SAPPurchaseOrder>();
                using (var SAPSourceModelContext = new SAPSourceModelContext())
                {
                    SAPPurchaseOrders = SAPSourceModelContext.GetPONumbers();
                    SAPPurchaseOrder empty = new SAPPurchaseOrder();
                    empty.PONumber = " ";
                    PONumbers.Add(empty);
                    foreach (SAPPurchaseOrder po in SAPPurchaseOrders)
                    {
                        if (po.PONumber != null)
                        {
                            var pro = (from p in PONumbers where p.PONumber == po.PONumber select p).FirstOrDefault();
                            if (pro == null)
                                PONumbers.Add(po);
                        }
                    }
                }
                ViewData["defaultPONumber"] = PONumbers.Select(p => p.PONumber).FirstOrDefault();
                LogManager.Debug("GetSAPPONumbers: END");
                return PONumbers;
            }
            catch (Exception ex)
            {
                LogManager.Error("GetSAPPONumbers: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public JsonResult GetSAPPOLines(string SAPPONumber)
        {
            try
            {
                LogManager.Debug("GetSAPPOLines: START");
                List<SAPPurchaseOrder> purchaseOrders = new List<SAPPurchaseOrder>();
                if (SAPPONumber != null)
                {
                    using (var SAPSourceModelContext = new SAPSourceModelContext())
                    {
                        purchaseOrders = SAPSourceModelContext.GetSAPPurchaseOrders();
                    }
                    var POLines = (from p in purchaseOrders where p.PONumber == SAPPONumber select p.POLine).Distinct();
                    return Json(POLines, JsonRequestBehavior.AllowGet);
                }
                LogManager.Debug("GetSAPPOLines: END");
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error("GetSAPPOLines: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public List<int> GetSAPCostCenterNos()
        {
            try
            {
                LogManager.Debug("GetSAPCostCenterNos: START");
                List<int> costCenterNos = new List<int>();
                using (var SAPSourceModelContext = new SAPSourceModelContext())
                {
                    costCenterNos = SAPSourceModelContext.GetSAPCostCenterNos();
                }

                LogManager.Debug("GetSAPCostCenterNos: END");

                return costCenterNos;
            }
            catch (Exception ex)
            {
                LogManager.Error("GetSAPCostCenterNos: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public JsonResult GetSAPWBS(string wbsNo)
        {
            try
            {
                LogManager.Debug("GetSAPWBS: START");

                IEnumerable<SAPWBS> wbs;
                using (var SAPSourceModelContext = new SAPSourceModelContext())
                {
                    if (!string.IsNullOrEmpty(wbsNo))
                    {
                        wbs = SAPSourceModelContext.GetSAPWBSFilter(wbsNo.Trim());
                    }
                    else
                    {
                        SAPWBS empty = new SAPWBS();
                        empty.WBSNo = "";
                        empty.Description = "";
                        List<SAPWBS> emptyList = new List<SAPWBS>();
                        emptyList.Add(empty);
                        wbs = emptyList;
                    }
                }
                LogManager.Debug("GetSAPWBS: END");

                return Json(wbs, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                LogManager.Error("GetSAPWBS: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public List<string> GetSAPInternalOrder()
        {
            try
            {
                LogManager.Debug("GetSAPInternalOrder: START");
                List<string> internalOrder = new List<string>();
                using (var SAPSourceModelContext = new SAPSourceModelContext())
                {
                    internalOrder = SAPSourceModelContext.GetSAPInternalOrder();
                }
                LogManager.Debug("GetSAPInternalOrder: END");
                return internalOrder;
            }

            catch (Exception ex)
            {
                LogManager.Error("GetSAPInternalOrder: ERROR " + ex.Message, ex);
                return null;
            }
        }


        private void SaveAccountingCostCodesGrid(List<ModifyAccountingCostCodesViewModel> updatedCostCodes, List<ModifyAccountingCostCodesViewModel> newCostCodes, List<ModifyAccountingCostCodesViewModel> deletedCostCodes)
        {
            try
            {
                LogManager.Debug("SaveAccountingCostCodesGrid: START");
                UpdateAccountingCostCodesGrid(updatedCostCodes);
                CreateRowInAccountingCostCodesGrid(newCostCodes);
                DeleteFromAccountingCostCodesGrid(deletedCostCodes);
                LogManager.Debug("SaveAccountingCostCodesGrid: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveAccountingCostCodesGrid: ERROR " + ex.Message, ex);
                
            }
        }


        private void UpdateAccountingCostCodesGrid(List<ModifyAccountingCostCodesViewModel> updatedCostCodes)
        {
            try
            {
                LogManager.Debug("UpdateAccountingCostCodesGrid: START");
                if (updatedCostCodes != null && updatedCostCodes.Count > 0)
                {
                    using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                    {
                        List<InvoicePODetailChanges> invoicePODetailChanges = new List<InvoicePODetailChanges>();
                        foreach (var costCode in updatedCostCodes)
                        {
                            invoicePODetailChanges.Add(new InvoicePODetailChanges
                            {
                                InvoiceDetailChangesID = costCode.InvoiceDetailChangesID,
                                InvoiceMasterID = costCode.InvoiceMasterID,
                                PONumber = costCode.SAPPONumber,
                                POLine = costCode.SAPPOLine,
                                GLAccount = null,
                                CostCenter = costCode.CostCenter,
                                WBS = costCode.WBSNo,
                                Fund = costCode.Fund,
                                FunctionalArea = costCode.FunctionalArea,
                                GrantNumber = costCode.GrantNumber,
                                InternalOrder = costCode.InternalOrder,
                                InvoiceAmount = costCode.InvoiceAmount,
                                Notes = costCode.Notes,
                                EditedByUserID = HttpContext.User.Identity.Name.ToString(),
                                EditedByDate = DateTime.Now
                            });
                        }
                        eInvoiceModelcontext.SaveAccountingCostCodes(invoicePODetailChanges);
                    }
                    LogManager.Debug("UpdateAccountingCostCodesGrid: END");
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("UpdateAccountingCostCodesGrid: ERROR " + ex.Message, ex);

            }

        }


        private void CreateRowInAccountingCostCodesGrid(List<ModifyAccountingCostCodesViewModel> newCostCodes)
        {
            try
            {
                LogManager.Debug("CreateRowInAccountingCostCodesGrid: START");
                if (newCostCodes != null && newCostCodes.Count > 0)
                {
                    using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                    {
                        List<InvoicePODetailChanges> invoicePODetailChanges = new List<InvoicePODetailChanges>();
                        if (Session["InvoiceMasterID"] != null)
                            invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                        foreach (var costCode in newCostCodes)
                        {
                            invoicePODetailChanges.Add(new InvoicePODetailChanges
                            {
                                InvoiceMasterID = (costCode.InvoiceMasterID == 0 ? invoiceMasterID : costCode.InvoiceMasterID),
                                PONumber = costCode.SAPPONumber,
                                POLine = costCode.SAPPOLine,
                                CostCenter = costCode.CostCenter,
                                WBS = costCode.WBSNo,
                                Fund = costCode.Fund,
                                FunctionalArea = costCode.FunctionalArea,
                                GrantNumber = costCode.GrantNumber,
                                InternalOrder = costCode.InternalOrder,
                                InvoiceAmount = costCode.InvoiceAmount,
                                Notes = costCode.Notes,
                                EditedByUserID = HttpContext.User.Identity.Name.ToString(),
                                EditedByDate = DateTime.Now
                            });
                        }
                        var result = eInvoiceModelcontext.SaveAccountingCostCodes(invoicePODetailChanges);
                        foreach (var m in newCostCodes)
                        {
                            var code = (from r in result
                                        where r.InvoiceMasterID == invoiceMasterID && r.POLine == m.SAPPOLine && r.PONumber == m.SAPPONumber && r.InvoiceAmount == m.InvoiceAmount && r.InternalOrder == m.InternalOrder && r.CostCenter == m.CostCenter && r.WBS == m.WBSNo && r.FunctionalArea == m.FunctionalArea && r.Fund == m.Fund
                                            && r.GrantNumber == m.GrantNumber && r.Notes == m.Notes
                                        select r).FirstOrDefault();
                            m.InvoiceDetailChangesID = code.InvoiceDetailChangesID;
                            m.GLAccount = code.GLAccount;
                        }
                    }
                }
                LogManager.Debug("CreateRowInAccountingCostCodesGrid: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("CreateRowInAccountingCostCodesGrid: ERROR " + ex.Message, ex);
                
            }
        }

         
        private void DeleteFromAccountingCostCodesGrid(List<ModifyAccountingCostCodesViewModel> deletedCostCodes)
        {
            try
            {
                LogManager.Debug("DeleteFromAccountingCostCodesGrid: START");
                if (deletedCostCodes != null && deletedCostCodes.Count > 0)
                {
                    using (eInvoiceModelContext eInvoiceModelcontext = new eInvoiceModelContext())
                    {
                        List<InvoicePODetailChanges> invoicePODetailChanges = new List<InvoicePODetailChanges>();
                        foreach (var costCode in deletedCostCodes)
                        {
                            invoicePODetailChanges.Add(new InvoicePODetailChanges
                            {
                                InvoiceDetailChangesID = costCode.InvoiceDetailChangesID,
                                InvoiceMasterID = (costCode.InvoiceMasterID == 0 ? invoiceMasterID : costCode.InvoiceMasterID),
                                PONumber = costCode.SAPPONumber,
                                POLine = costCode.SAPPOLine,
                                GLAccount = null,
                                CostCenter = costCode.CostCenter,
                                WBS = costCode.WBSNo,
                                Fund = costCode.Fund,
                                FunctionalArea = costCode.FunctionalArea,
                                GrantNumber = costCode.GrantNumber,
                                InternalOrder = costCode.InternalOrder,
                                InvoiceAmount = costCode.InvoiceAmount,
                                Notes = costCode.Notes,
                                EditedByUserID = HttpContext.User.Identity.Name.ToString(),
                                EditedByDate = DateTime.Now
                            });
                        }
                        eInvoiceModelcontext.DeleteAccountingCostCodes(invoicePODetailChanges);
                    }
                }
                LogManager.Debug("DeleteFromAccountingCostCodesGrid: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("DeleteFromAccountingCostCodesGrid: ERROR " + ex.Message, ex);
               
            }
        }

        #endregion
    }
}
