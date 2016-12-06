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
    public class eInvoicePDFController : PdfViewController
    {
        private static readonly log4net.ILog LogManager = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        int invoiceMasterID;
        int InvoiceCATTFindingsID;

        public ActionResult GeneratePDF(string documentNo)
        {
            try
            {
                LogManager.Debug("GeneratePDF: START");

                if (string.IsNullOrEmpty(documentNo)) { throw new Exception("Document No. cannot be empty"); }
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                    {
                    if (Session["InvoiceMasterID"] != null)
                        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                    else
                        invoiceMasterID = eInvoiceModelContext.GetInvoiceMasterIDFilterOnStatus(documentNo);
                    };
                if (invoiceMasterID == 0) { throw new Exception("Document No. is not valid"); }

                PDFDocumentViewModel PDFDocumentViewModel = new PDFDocumentViewModel();
                // Shared call to Generate PDF
                PDFHelper.GeneratePDF(invoiceMasterID, out documentNo, out PDFDocumentViewModel);

                return this.ViewPdf("", "_GeneratePDF", PDFDocumentViewModel);

                #region repeated old GeneratePDF commented code

                //using (var eInvoiceModelContext = new eInvoiceModelContext())
                //{
                    
                //    if (Session["InvoiceMasterID"] != null)
                //        invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
                //    else
                //        invoiceMasterID = eInvoiceModelContext.GetInvoiceMasterIDFilterOnStatus(documentNo);

                //    if (invoiceMasterID == 0) { throw new Exception("Document No. is not valid"); }

                //    //Getting Grid Totals for display at Grid Footer..
                //    PDFDocumentViewModel.InvoiceGridTotalsforPDF = new InvoiceGridTotalsforPDF();
                //    PDFDocumentViewModel.InvoiceGridTotalsforPDF = eInvoiceModelContext.GetInvoiceGridTotalsforPDF(invoiceMasterID);


                //    //Get eInvoice Approvers...
                //    PDFDocumentViewModel.eInvoiceApprovers = new List<eInvoiceApprovers>();
                //    PDFDocumentViewModel.eInvoiceApprovers = eInvoiceModelContext.GeteInvoiceApprovers(invoiceMasterID);

                //    //Get Routing Details Data..
                //    PDFDocumentViewModel.ApproversViewModel = new List<ApproversViewModel>();
                //    PDFDocumentViewModel.ApproversViewModel = GetRoutingApprovers(eInvoiceModelContext.GetDestinationApproversList(invoiceMasterID, false));
                //    PDFDocumentViewModel.CommentsViewModel = new List<CommentsViewModel>();
                //    PDFDocumentViewModel.CommentsViewModel = GetRoutingComments(eInvoiceModelContext.GetInvoiceComments(invoiceMasterID));
                //    PDFDocumentViewModel.AttachmentsViewModel = new List<AttachmentsViewModel>();
                //    PDFDocumentViewModel.AttachmentsViewModel = GetRoutingAttachments(eInvoiceModelContext.GetInvoiceAttachments(invoiceMasterID));


                //    //Get PODetail Data...
                //    PDFDocumentViewModel.ModifyAccountingCostCodesViewModel = GetPOModifyAccountingCostCodes(eInvoiceModelContext.GetInvoicePODetailChanges(invoiceMasterID));

                //    if (PDFDocumentViewModel.ModifyAccountingCostCodesViewModel != null)
                //    {
                //        //Add Grid Total..
                //        PDFDocumentViewModel.ModifyAccountingCostCodesViewModel.Add(new ModifyAccountingCostCodesViewModel
                //        {
                //            SAPPONumber = "Total:",
                //            InvoiceAmount = PDFDocumentViewModel.InvoiceGridTotalsforPDF.POInvoiceAmt.Value,
                //        });
                //    }

                //    PDFDocumentViewModel.AccountingCostCodesViewModel = GetAccountingCostCodes(eInvoiceModelContext.GetInvoicePODetails(invoiceMasterID));

                //    if (PDFDocumentViewModel.AccountingCostCodesViewModel != null)
                //    {
                //        //Add Grid Total..
                //        PDFDocumentViewModel.AccountingCostCodesViewModel.Add(new AccountingCostCodesViewModel
                //        {
                //            PONumber = "Total:",
                //            //  POLine=null,
                //            InvoiceAmount = PDFDocumentViewModel.InvoiceGridTotalsforPDF.POInvoiceAmtReadOnly.Value,
                //        });
                //    }

                //    //Get Short Pay Data..
                //    PDFDocumentViewModel.ShortPayIndexViewModel = new ShortPayIndexViewModel();
                //    PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay = eInvoiceModelContext.GetShortPayDetails(invoiceMasterID);

                //    if (PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay == null)
                //    {
                //        PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay = new InvoiceShortPayLetter();
                //    }

                //    //Get CA for InvoiceMasterID, RoleID of CA = 3
                //    PDFDocumentViewModel.ShortPayIndexViewModel.SentFrom = eInvoiceModelContext.GetCATTFindingsApprover(PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay.SentFrom);
                //    PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay.SentFrom = BuildEmployeeCSV(PDFDocumentViewModel.ShortPayIndexViewModel.SentFrom);
                    

                //    //Get CATT Findings Data..
                //    string LoggedUserCATTorCA = string.Empty;
                //    PDFDocumentViewModel.CATTFindingsViewModel = new CATTFindingsViewModel();
                //    PDFDocumentViewModel.CATTFindingsViewModel.DocumentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(invoiceMasterID);
                //    PDFDocumentViewModel.CATTFindingsViewModel.RoutingDetails = new RoutingDetailsViewModel();
                //    //Get Routing Header
                //    PDFDocumentViewModel.CATTFindingsViewModel.RoutingDetails.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);

                //    //Get InvoiceCATTFindings..
                //    PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings = eInvoiceModelContext.GetInvoiceCATTFindings(invoiceMasterID);
                //    if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings == null)
                //    {
                //        PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings = new InvoiceCATTFindings();
                //    }
                //        //Get CA for InvoiceMasterID, RoleID of CA=3
                //        PDFDocumentViewModel.CATTFindingsViewModel.ToCA = eInvoiceModelContext.GetCATTFindingsApprover(PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.AddressedTo);
                //        //Get CATT for InvoiceMasterID, RoleID of CATT=2
                //        PDFDocumentViewModel.CATTFindingsViewModel.FromCATT = eInvoiceModelContext.GetCATTFindingsApprover(PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.SentFrom);
                //        PDFDocumentViewModel.CATTFindingsViewModel.DateSubmit = PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.Date;

                //        decimal? invoiceAmt = PDFDocumentViewModel.CATTFindingsViewModel.RoutingDetails.InvoiceDetails.InvoiceAmount;

                //        if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment.HasValue)
                //            PDFDocumentViewModel.CATTFindingsViewModel.AssetPayment = invoiceAmt - PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment.Value;
                //        else
                //        {
                //            PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment = 0;
                //            PDFDocumentViewModel.CATTFindingsViewModel.AssetPayment = invoiceAmt - 0;
                //        }
                //        if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment.HasValue)
                //            PDFDocumentViewModel.CATTFindingsViewModel.ApprovedPayment = invoiceAmt - PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment.Value;
                //        else
                //        {
                //            PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment = 0;
                //            PDFDocumentViewModel.CATTFindingsViewModel.ApprovedPayment = invoiceAmt - 0;
                //        }
                    
                //    PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindingsEmp = eInvoiceModelContext.GetInvoiceCATTFindingsEmp(invoiceMasterID);

                //    if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindingsEmp != null)
                //    {
                //        //Manually Adding Totals in Grid Last row.
                //        PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindingsEmp.Add(
                //            new InvoiceCATTFindingsEmp
                //            {
                //                EmployeeName = "Total:",
                //                RateVariance = PDFDocumentViewModel.InvoiceGridTotalsforPDF.CATTRateVariance.Value,
                //                Total = PDFDocumentViewModel.InvoiceGridTotalsforPDF.CATTTotal.Value,
                //            });
                //    }
                //    PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCAFindingsEmp = eInvoiceModelContext.GetInvoiceCAFindingsEmp(invoiceMasterID);

                //    if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCAFindingsEmp != null)
                //    {
                //        //Manually Adding Totals in Grid Last row.
                //        PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCAFindingsEmp.Add(
                //            new InvoiceCAFindingsEmp
                //            {
                //                EmployeeName = "Total:",
                //                RateVariance = PDFDocumentViewModel.InvoiceGridTotalsforPDF.CARateVariance.Value,
                //                Total = PDFDocumentViewModel.InvoiceGridTotalsforPDF.CATotal.Value,
                //            });
                //    }

                //    PDFDocumentViewModel.CATTFindingsViewModel.ToCACSV = BuildEmployeeCSV(PDFDocumentViewModel.CATTFindingsViewModel.ToCA);
                //    PDFDocumentViewModel.CATTFindingsViewModel.FromCATTCSV = BuildEmployeeCSV(PDFDocumentViewModel.CATTFindingsViewModel.FromCATT);

                //    LogManager.Debug("GeneratePDF: END");

                //    return this.ViewPdf("", "_GeneratePDF", PDFDocumentViewModel);
                //}
                #endregion

            }

            catch (Exception ex)
            {
                LogManager.Error("GeneratePDF: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
       
        }

        //This Method has been moved to Routing Controller...
        //public ActionResult SavePDF(string documentNo)
        //{
        //    try
        //    {
        //        LogManager.Debug("SavePDF: START");

        //      //  base.Initialize(requestContext);

        //        using (var eInvoiceModelContext = new eInvoiceModelContext())
        //        {
        //            PDFDocumentViewModel = new PDFDocumentViewModel();
        //          //  Session["InvoiceMasterID"] = eInvoiceModelContext.GetInvoiceMasterID(documentNo);
        //          //  invoiceMasterID = Convert.ToInt32(Session["InvoiceMasterID"]);
        //            invoiceMasterID = eInvoiceModelContext.GetInvoiceMasterID(documentNo);

        //            //Get Routing Details Data..
        //            PDFDocumentViewModel.ApproversViewModel = new List<ApproversViewModel>();
        //            PDFDocumentViewModel.ApproversViewModel = GetRoutingApprovers(eInvoiceModelContext.GetDestinationApproversList(invoiceMasterID, false));
        //            PDFDocumentViewModel.CommentsViewModel = new List<CommentsViewModel>();
        //            PDFDocumentViewModel.CommentsViewModel = GetRoutingComments(eInvoiceModelContext.GetInvoiceComments(invoiceMasterID));
        //            PDFDocumentViewModel.AttachmentsViewModel = new List<AttachmentsViewModel>();
        //            PDFDocumentViewModel.AttachmentsViewModel = GetRoutingAttachments(eInvoiceModelContext.GetInvoiceAttachments(invoiceMasterID));


        //            //Get PODetail Data...
        //            PDFDocumentViewModel.ModifyAccountingCostCodesViewModel = GetPOModifyAccountingCostCodes(eInvoiceModelContext.GetInvoicePODetailChanges(invoiceMasterID));
        //            PDFDocumentViewModel.AccountingCostCodesViewModel = GetAccountingCostCodes(eInvoiceModelContext.GetInvoicePODetails(invoiceMasterID));


        //            //Get Short Pay Data..
        //            PDFDocumentViewModel.ShortPayIndexViewModel = new ShortPayIndexViewModel();
        //            PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay = eInvoiceModelContext.GetShortPayDetails(invoiceMasterID);

        //            //Get CA for InvoiceMasterID, RoleID of CA = 3
        //            PDFDocumentViewModel.ShortPayIndexViewModel.SentFrom = eInvoiceModelContext.GetCATTFindingsApprover(PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay.SentFrom);
        //            PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay.SentFrom = BuildEmployeeCSV(PDFDocumentViewModel.ShortPayIndexViewModel.SentFrom);

        //            //Get CATT Findings Data..
        //            string LoggedUserCATTorCA = string.Empty;
        //            PDFDocumentViewModel.CATTFindingsViewModel = new CATTFindingsViewModel();
        //            PDFDocumentViewModel.CATTFindingsViewModel.DocumentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(invoiceMasterID);
        //            PDFDocumentViewModel.CATTFindingsViewModel.RoutingDetails = new RoutingDetailsViewModel();
        //            //Get Routing Header
        //            PDFDocumentViewModel.CATTFindingsViewModel.RoutingDetails.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);

        //            //Get InvoiceCATTFindings..
        //            PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings = eInvoiceModelContext.GetInvoiceCATTFindings(invoiceMasterID);
        //            //Get CA for InvoiceMasterID, RoleID of CA=3
        //            PDFDocumentViewModel.CATTFindingsViewModel.ToCA = eInvoiceModelContext.GetCATTFindingsApprover(PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.AddressedTo);
        //            //Get CATT for InvoiceMasterID, RoleID of CATT=2
        //            PDFDocumentViewModel.CATTFindingsViewModel.FromCATT = eInvoiceModelContext.GetCATTFindingsApprover(PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.SentFrom);
        //            PDFDocumentViewModel.CATTFindingsViewModel.DateSubmit = PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.Date;
        //            //Store InvoiceCATTFindingsID in session for Future User..
        //          //  Session["InvoiceCATTFindingsID"] = PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.InvoiceCATTFindingsID;
        //            decimal? invoiceAmt = PDFDocumentViewModel.CATTFindingsViewModel.RoutingDetails.InvoiceDetails.InvoiceAmount;

        //            if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment.HasValue)
        //                PDFDocumentViewModel.CATTFindingsViewModel.AssetPayment = invoiceAmt - PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment.Value;
        //            else
        //            {
        //                PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment = 0;
        //                PDFDocumentViewModel.CATTFindingsViewModel.AssetPayment = 0;
        //            }
        //            if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment.HasValue)
        //                PDFDocumentViewModel.CATTFindingsViewModel.ApprovedPayment = invoiceAmt - PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment.Value;
        //            else
        //            {
        //                PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment = 0;
        //                PDFDocumentViewModel.CATTFindingsViewModel.ApprovedPayment = 0;
        //            }
        //            PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindingsEmp = eInvoiceModelContext.GetInvoiceCATTFindingsEmp(invoiceMasterID);
        //            PDFDocumentViewModel.CATTFindingsViewModel.ToCACSV = BuildEmployeeCSV(PDFDocumentViewModel.CATTFindingsViewModel.ToCA);
        //            PDFDocumentViewModel.CATTFindingsViewModel.FromCATTCSV = BuildEmployeeCSV(PDFDocumentViewModel.CATTFindingsViewModel.FromCATT);
        //            LogManager.Debug("SavePDF: END");
        //            this.SavePdf("", "_GeneratePDF", "eInvoice-" + documentNo + ".pdf", PDFDocumentViewModel);
        //            return null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error("SavePDF: ERROR " + ex.Message, ex);
        //        return null;
        //    }
        //}


       #region repeated old GeneratePDF helpers commented code

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
       //}

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
       //             result.Approver = ((p.ApproverUserID == null || p.ApproverUserID == String.Empty) ? String.Empty : FetchApprover(p.ApproverUserID));
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
       // {
       //     try
       //     {
       //         LogManager.Debug("BuildEmployeeCSV: START");
       //         string empcsv = string.Empty;
       //         foreach (ExchangeEmployeeProfile sepCATT in employees)
       //         {
       //             empcsv = empcsv + (string.IsNullOrEmpty(empcsv) ? sepCATT.ApproverName : ", " + sepCATT.ApproverName);
       //         }
       //         LogManager.Debug("BuildEmployeeCSV: END");
       //         return empcsv;
       //     }

       //     catch (Exception ex)
       //     {
       //         LogManager.Error("BuildEmployeeCSV: ERROR " + ex.Message, ex);
       //         return null;
       //     }
       // }


       // private string FetchApprover(string approverUserID)
       // {
       //     try
       //     {
       //         LogManager.Debug("FetchApprover: START");
       //         List<ExchangeEmployeeProfile> approversList;
       //         string concatenatedName = string.Empty;
       //         using (var SAPSourceModelContext = new SAPSourceModelContext())
       //         {
       //             approversList = SAPSourceModelContext.FetchExchangeEmployeesList();
       //         }
       //         if (approversList != null && approversList.Count > 0)
       //             concatenatedName = (from approver in approversList where approver.UserID.ToLower() == approverUserID.ToLower() select approver.Concatenated).FirstOrDefault();
       //         LogManager.Debug("FetchApprover: END");
       //         return concatenatedName;
       //     }
       //     catch (Exception ex)
       //     {
       //         LogManager.Error("FetchApprover: ERROR " + ex.Message, ex);
       //         return null;
       //     }
       // }

       #endregion

    } 
}