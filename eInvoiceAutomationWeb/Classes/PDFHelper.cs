using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.SharePoint.Client;
using eInvoiceAutomationWeb.ViewModels;
using eInvoiceApplication.DomainModel;
using SAPSourceMasterApplication.DomainModel;
using eInvoiceAutomationWeb.PDF;
using System.Web.Mvc;

namespace eInvoiceAutomationWeb
    {
    public static class PDFHelper
        {
        private static readonly log4net.ILog LogManager = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // 06/29/2015 - Post UAT Changes - In Progress - Build 6
        #region PDFFileUpload

        public static bool CheckDocLibExists(string docLibName)
            {
            bool retDocLibExists = false;
            string SPUrl = System.Configuration.ConfigurationManager.AppSettings["SharePointSiteURL"].ToString();            
            try
                {
                using (ClientContext clientContext = new ClientContext(SPUrl))
                    {
                    ListCollection listCollection = clientContext.Web.Lists;
                    clientContext.Load(listCollection, lists => lists.Include(list => list.Title).Where(list => list.Title == docLibName));

                    clientContext.ExecuteQuery();
                    if (listCollection.Count > 0)
                        {
                        retDocLibExists = true;
                        }
                    else
                        {
                        retDocLibExists = false;
                        }
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("CheckDocLibExists: Error" + ex.Message);
                return false;
                }
            return retDocLibExists;
            }

        public static bool UploadDocToSharePoint(FileStream fileStream, string uploadfileName, string documentNo, string invoiceNo, string vendorNo, 
            string vendorName, string poNumber, DateTime invoiceDate, decimal? invoiceAmount, string description, bool uploadShortPayPDF)
            {
            vendorNo = vendorNo.TrimStart('0');
            string fileName = documentNo + "-eInvoiceApproval.pdf";
            string shortPayFileName = documentNo + "-eInvoiceShortPayApproval.pdf";
            string SPUrl = System.Configuration.ConfigurationManager.AppSettings["SharePointSiteURL"].ToString();
            //string SPFolder = System.Configuration.ConfigurationManager.AppSettings["SharePointFolder"].ToString();
            string docLibNameURLName = System.Configuration.ConfigurationManager.AppSettings["SharePointDocLibraryURLName"].ToString();
            string docLibName = System.Configuration.ConfigurationManager.AppSettings["SharePointDocLibraryURLName"].ToString();
            string SPRelativeURL = System.Configuration.ConfigurationManager.AppSettings["SharePointSiteRelativeURL"].ToString();

            //string shortPayPdfFileName = documentNo + "eInvoiceApprovalShortPay.pdf";
            string fullFileNamewithPath = HttpContext.Current.Server.MapPath("~") + "\\PDFDocs\\" + fileName;

            using (ClientContext clientContext = new ClientContext(SPUrl))
                {
                try
                    {
                    LogManager.Debug("UploadDocToSharePoint: Start");
                    //List documentLibrary = clientContext.Web.Lists.GetByTitle("Documents");
                    List documentLibrary = clientContext.Web.Lists.GetByTitle(docLibName);

                    //SPCreateFolderwithCheckExists(SPUrl, docLibName, SPFolder + "/" + vendorNo);
                    //SPCreateFolderwithCheckExists(SPUrl, docLibName, SPFolder + "/" + vendorNo + "/" + invoiceNo);
                    //SPCreateFolderwithCheckExists(SPUrl, docLibName, vendorNo);
                    //SPCreateFolderwithCheckExists(SPUrl, docLibName, vendorNo + "/" + invoiceNo);

                    SPCreateFolderwithCheckExists(SPUrl, docLibName, vendorNo);

                    SPCreateFolderwithCheckExists(SPUrl, docLibName, vendorNo + "/" + documentNo);

                    clientContext.RequestTimeout = 1000000;

                    //int increaseSize = 10485760; //10MB
                    //SPWebService contentService = SPWebService.ContentService;
                    //contentService.ClientRequestServiceSettings.MaxReceivedMessageSize = increaseSize;
                    //contentService.Update(); 

                    //FileCreationInformation newUploadFile = new FileCreationInformation();
                    //FileStream fileStream = new FileStream(fullFileNamewithPath, FileMode.Open);
                    //newUploadFile.Overwrite = true;
                    //newUploadFile.Url = SPUrl + "/" + docLibName + "/" + SPFolder + "/" + vendorNo + "/" + invoiceNo + "/" + documentNo + "/" + pdfFileName;
                    //newUploadFile.Url = SPUrl + "/" + docLibNameURLName + "/" + vendorNo + "/" + invoiceNo + "/" + uploadfileName;
                    //newUploadFile.Url = SPUrl + "/" + docLibNameURLName + "/" + invoiceNo + "/" + uploadfileName;
                    //byte[] fileContents = new byte[fileStream.Length];
                    //fileStream.Read(fileContents, 0, Convert.ToInt32(fileStream.Length));
                    //fileStream.Close();
                    //newUploadFile.Content = fileContents; // "Add File Stream Array";
                    clientContext.ExecuteQuery();

                    Microsoft.SharePoint.Client.File.SaveBinaryDirect(clientContext,
                            SPRelativeURL + "/" + docLibNameURLName + "/" + vendorNo + "/" + documentNo + "/" + uploadfileName, fileStream, true);

                    Microsoft.SharePoint.Client.File file = clientContext.Web.GetFileByServerRelativeUrl(
                        SPRelativeURL + "/" + docLibNameURLName + "/" + vendorNo + "/" + documentNo + "/" + uploadfileName);

                    ListItem lstItem = file.ListItemAllFields;
                    clientContext.Load(lstItem);
                    clientContext.ExecuteQuery();

                    lstItem["Title"] = "eInvoice-" + documentNo;
                    lstItem["DocumentNumber"] = documentNo;
                    lstItem["InvoiceNumber"] = invoiceNo;
                    lstItem["VendorNumber"] = vendorNo;
                    lstItem["VendorName"] = vendorName;
                    lstItem["PONumber"] = poNumber;
                    lstItem["invoicedate"] = invoiceDate;
                    lstItem["InvoiceAmount"] = invoiceAmount;
                    lstItem["CategoryDescription"] = description;
                    lstItem.Update();
                    clientContext.ExecuteQuery();

                    fileStream.Close();

                    List oList = clientContext.Web.Lists.GetByTitle("eInvoiceDocsList");
                    LogManager.Error("TEMPUploadDocToSharePoint: Add or Update eInvoiceDocsList Start");
                    var qry = new CamlQuery();
                    qry.ViewXml = "<View Scope=\'RecursiveAll\'><Query><Where><Eq><FieldRef Name='DocumentNumber' /><Value Type='Text'>" + documentNo + "</Value></Eq></Where></Query><RowLimit>100</RowLimit></View>";
                    ListItemCollection items = oList.GetItems(qry);
                    clientContext.Load(items);
                    clientContext.ExecuteQuery();
                    LogManager.Error("TEMPUploadDocToSharePoint: Add or Update eInvoiceDocsList MID");
                    if (items.Count > 0)
                        lstItem = items[0];
                    else
                        {

                        SPCreateFolderwithCheckExists(SPUrl, "eInvoiceDocsList", vendorNo);

                        SPCreateFolderwithCheckExists(SPUrl, "eInvoiceDocsList", vendorNo + "/" + documentNo);

                        ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                        itemCreateInfo.FolderUrl = SPRelativeURL + "/Lists/" + "eInvoiceDocsList" + "/" + vendorNo + "/" + documentNo;
                        //$newItem = $spList.Items.Add($updateLstUrl + "/" + $vendorNo + "/" + $docNo,0)
                        //itemCreateInfo.LeafName = vendorNo + "/" + documentNo;
                        lstItem = oList.AddItem(itemCreateInfo);
                        lstItem["Title"] = "eInvoice-" + documentNo;
                        }
                    lstItem["DocumentNumber"] = documentNo;
                    lstItem["InvoiceNumber"] = invoiceNo;
                    lstItem["InvoiceAmount"] = invoiceAmount;
                    lstItem["VendorNumber"] = vendorNo;
                    lstItem["VendorName"] = vendorName;
                    FieldUrlValue url = new FieldUrlValue();
                    url.Url = SPUrl + "/" + docLibName + "/" + vendorNo + "/" + documentNo;
                    url.Description = "Approval Docs";
                    lstItem["ApprovalDocs"] = url;
                    
                    lstItem.Update();

                    clientContext.ExecuteQuery();

                    LogManager.Error("TEMPUploadDocToSharePoint: Add or Update eInvoiceDocsList End");

                    if (uploadShortPayPDF)
                    //Upload ShortPay letter PDF as well
                        {
                        LogManager.Error("UploadDocToSharePoint: Upload Short Pay PDF Start");

                        //newUploadFile = new FileCreationInformation();
                        fullFileNamewithPath = HttpContext.Current.Server.MapPath("~") + "\\PDFDocs\\" + shortPayFileName;
                        fileStream = new FileStream(fullFileNamewithPath, FileMode.Open);
                        //newUploadFile.Overwrite = true;
                        //newUploadFile.Url = SPUrl + "/" + docLibName + "/" + SPFolder + "/" + vendorNo + "/" + invoiceNo + "/" + documentNo + "/" + shortPayPdfFileName;
                        //newUploadFile.Url = SPUrl + "/" + docLibNameURLName + "/" + invoiceNo + "/" + shortPayFileName;

                        clientContext.ExecuteQuery();

                        Microsoft.SharePoint.Client.File.SaveBinaryDirect(clientContext,
                                SPRelativeURL + "/" + docLibNameURLName + "/" + vendorNo + "/" + documentNo + "/" + shortPayFileName, fileStream, true);
                        file = clientContext.Web.GetFileByServerRelativeUrl(
                            SPRelativeURL + "/" + docLibNameURLName + "/" + vendorNo + "/" + documentNo + "/" + shortPayFileName);

                        lstItem = file.ListItemAllFields;
                        clientContext.Load(lstItem);
                        clientContext.ExecuteQuery();

                        lstItem["Title"] = "eInvoiceShortPay-" + documentNo;
                        lstItem["DocumentNumber"] = documentNo;
                        lstItem["InvoiceNumber"] = invoiceNo;
                        lstItem["VendorNumber"] = vendorNo;
                        lstItem["VendorName"] = vendorName;
                        lstItem["PONumber"] = poNumber;
                        lstItem["invoicedate"] = invoiceDate;
                        lstItem["InvoiceAmount"] = invoiceAmount;
                        lstItem["CategoryDescription"] = description;
                        lstItem.Update();
                        clientContext.ExecuteQuery();
                        fileStream.Close();
                        LogManager.Error("UploadDocToSharePoint: Upload Short Pay PDF End");
                        }

                    LogManager.Debug("UploadDocToSharePoint: End");
                    }
                catch (Exception ex)
                    {
                    LogManager.Error("UploadDocToSharePoint: Error" + ex.Message);
                    return false;
                    }
                }
            return true;
            }

        public static bool DeleteDocFromSharePoint(string uploadfileName, string invoiceNo, string vendorNo, string docNo)
            {

            vendorNo = vendorNo.TrimStart('0');
            string SPUrl = System.Configuration.ConfigurationManager.AppSettings["SharePointSiteURL"].ToString();
            //string SPFolder = System.Configuration.ConfigurationManager.AppSettings["SharePointFolder"].ToString();
           // string docLibName = System.Configuration.ConfigurationManager.AppSettings["SharePointDocLibrary"].ToString();
            string docLibNameURL = System.Configuration.ConfigurationManager.AppSettings["SharePointDocLibraryURLName"].ToString();
            string SPRelativeURL = System.Configuration.ConfigurationManager.AppSettings["SharePointSiteRelativeURL"].ToString(); 
        
            using (ClientContext clientContext = new ClientContext(SPUrl))
                {
                try
                    {
                    LogManager.Debug("DeleteDocFromSharePoint: Start");
                    //string subSiteURL = SPUrl.Substring(SPUrl.IndexOf("/sites/"));
                    //string deleteFileURL = subSiteURL + "/" + docLibNameURL + "/" + SPFolder + "/" + vendorNo + "/" + invoiceNo + "/" + uploadfileName;
                    // SM TTD change this as per change of Library with Vendor No and Doc No Folders
                    string deleteFileURL = SPRelativeURL + "/" + docLibNameURL + "/" + vendorNo + "/" + docNo + "/" + uploadfileName;
                    Microsoft.SharePoint.Client.File deleteFile = clientContext.Web.GetFileByServerRelativeUrl(deleteFileURL);
                    clientContext.Load(deleteFile);
                    deleteFile.DeleteObject();
                    clientContext.ExecuteQuery();
                        
                    LogManager.Debug("DeleteDocFromSharePoint: End");
                    }
                catch (Exception ex)
                    {
                    LogManager.Error("DeleteDocFromSharePoint: Error" + ex.Message);
                    return false;
                    }
                }
            return true;
            }

        public static void SPCreateFolderwithCheckExists(string SPUrl, string docLibName, string folderName)
            {
            ClientContext clientContext = new ClientContext(SPUrl);
            Web web = clientContext.Web;
            List list = clientContext.Web.Lists.GetByTitle(docLibName);

            clientContext.Load(clientContext.Site);

            string targetFolderUrl = docLibName + "/" + folderName;
            Folder folder = web.GetFolderByServerRelativeUrl(targetFolderUrl);
            clientContext.Load(folder);
            bool exists = false;

            try
                {
                clientContext.ExecuteQuery();
                exists = true;
                }
            catch (Exception ex)
            { }


            if (!exists)
                {
                ContentTypeCollection listContentTypes = list.ContentTypes;
                clientContext.Load(listContentTypes, types => types.Include
                                 (type => type.Id, type => type.Name,
                                 type => type.Parent));

                var result = clientContext.LoadQuery(listContentTypes.Where
                  (c => c.Name == "Folder"));

                clientContext.ExecuteQuery();

                ContentType folderContentType = result.FirstOrDefault();

                ListItemCreationInformation newItemInfo = new ListItemCreationInformation();
                newItemInfo.UnderlyingObjectType = FileSystemObjectType.Folder;
                string newfoldername = folderName;

                if (folderName.Contains('/'))
                    newfoldername = folderName.Split('/')[1];

                newItemInfo.LeafName = folderName;
                ListItem newListItem = list.AddItem(newItemInfo);

                newListItem["ContentTypeId"] = folderContentType.Id.ToString();
                newListItem["Title"] = newfoldername;
                newListItem.Update();

                clientContext.Load(list);
                try
                    {
                    clientContext.ExecuteQuery();
                    exists = true;
                    }
                catch (Exception ex)
                { }


                }

            }

        #endregion

        #region PDFFileSave

        public static void GeneratePDF(int invoiceMasterID, out string documentNo, out PDFDocumentViewModel PDFDocumentViewModel)
            {
            #region Generate PDF
            LogManager.Debug("Generate PDF: Start");

            

            using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                PDFDocumentViewModel = new PDFDocumentViewModel();
                documentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(invoiceMasterID);
                // invoiceMasterID = eInvoiceModelContext.GetInvoiceMasterID(documentNo);

                //Getting Grid Totals for display at Grid Footer..
                PDFDocumentViewModel.InvoiceGridTotalsforPDF = new InvoiceGridTotalsforPDF();
                PDFDocumentViewModel.InvoiceGridTotalsforPDF = eInvoiceModelContext.GetInvoiceGridTotalsforPDF(invoiceMasterID);

                //Get eInvoice Approvers...
                PDFDocumentViewModel.eInvoiceApprovers = new List<eInvoiceApprovers>();
                PDFDocumentViewModel.eInvoiceApprovers = eInvoiceModelContext.GeteInvoiceApprovers(invoiceMasterID);

                //Get Routing Details Data..
                PDFDocumentViewModel.ApproversViewModel = new List<ApproversViewModel>();
                PDFDocumentViewModel.ApproversViewModel = GetRoutingApprovers(eInvoiceModelContext.GetDestinationApproversList(invoiceMasterID, false));
                PDFDocumentViewModel.CommentsViewModel = new List<CommentsViewModel>();
                PDFDocumentViewModel.CommentsViewModel = GetRoutingComments(eInvoiceModelContext.GetInvoiceComments(invoiceMasterID));
                PDFDocumentViewModel.AttachmentsViewModel = new List<AttachmentsViewModel>();
                PDFDocumentViewModel.AttachmentsViewModel = GetRoutingAttachments(eInvoiceModelContext.GetInvoiceAttachments(invoiceMasterID));

                //Get PODetail Data...
                PDFDocumentViewModel.ModifyAccountingCostCodesViewModel = GetPOModifyAccountingCostCodes(eInvoiceModelContext.GetInvoicePODetailChanges(invoiceMasterID));

                if ((PDFDocumentViewModel.ModifyAccountingCostCodesViewModel != null) && (PDFDocumentViewModel.ModifyAccountingCostCodesViewModel.Count > 0))
                    {
                    //Add Grid Total..
                    PDFDocumentViewModel.ModifyAccountingCostCodesViewModel.Add(new ModifyAccountingCostCodesViewModel
                    {
                        SAPPONumber = "Total:",
                        InvoiceAmount = PDFDocumentViewModel.InvoiceGridTotalsforPDF.POInvoiceAmt.Value,
                    });
                    }

                PDFDocumentViewModel.AccountingCostCodesViewModel = GetAccountingCostCodes(eInvoiceModelContext.GetInvoicePODetails(invoiceMasterID));

                if ((PDFDocumentViewModel.AccountingCostCodesViewModel != null) && (PDFDocumentViewModel.AccountingCostCodesViewModel.Count > 0))
                    {
                    //Add Grid Total..
                    PDFDocumentViewModel.AccountingCostCodesViewModel.Add(new AccountingCostCodesViewModel
                    {
                        PONumber = "Total:",
                        POLine = null,
                        InvoiceAmount = PDFDocumentViewModel.InvoiceGridTotalsforPDF.POInvoiceAmtReadOnly.Value,
                    });
                    }

                //Get Short Pay Data..
                PDFDocumentViewModel.ShortPayIndexViewModel = new ShortPayIndexViewModel();
                PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay = eInvoiceModelContext.GetShortPayDetails(invoiceMasterID);

                if (PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay == null)
                    {
                    //PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay = new InvoiceShortPayLetter();
                    }

                if (PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay != null)
                    {
                    //Get CA for InvoiceMasterID, RoleID of CA = 3
                    PDFDocumentViewModel.ShortPayIndexViewModel.SentFrom = eInvoiceModelContext.GetCATTFindingsApprover(PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay.SentFrom);
                    PDFDocumentViewModel.ShortPayIndexViewModel.ShortPay.SentFrom = BuildEmployeeCSV(PDFDocumentViewModel.ShortPayIndexViewModel.SentFrom);
                    }

                //Get CATT Findings Data..
                string LoggedUserCATTorCA = string.Empty;
                PDFDocumentViewModel.CATTFindingsViewModel = new CATTFindingsViewModel();
                PDFDocumentViewModel.CATTFindingsViewModel.DocumentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(invoiceMasterID);
                PDFDocumentViewModel.CATTFindingsViewModel.RoutingDetails = new RoutingDetailsViewModel();
                //Get Routing Header
                PDFDocumentViewModel.CATTFindingsViewModel.RoutingDetails.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeaderTemp(invoiceMasterID);

                //Get InvoiceCATTFindings..
                PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings = eInvoiceModelContext.GetInvoiceCATTFindings(invoiceMasterID);
                if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings == null)
                    {
                    //PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings = new InvoiceCATTFindings();
                    }

                //Get CA for InvoiceMasterID, RoleID of CA=3
                PDFDocumentViewModel.CATTFindingsViewModel.ToCA = eInvoiceModelContext.GetCATTFindingsApprover(PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.AddressedTo);
                //Get CATT for InvoiceMasterID, RoleID of CATT=2
                PDFDocumentViewModel.CATTFindingsViewModel.FromCATT = eInvoiceModelContext.GetCATTFindingsApprover(PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.SentFrom);
                PDFDocumentViewModel.CATTFindingsViewModel.DateSubmit = PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.Date;

                decimal? invoiceAmt = PDFDocumentViewModel.CATTFindingsViewModel.RoutingDetails.InvoiceDetails.InvoiceAmount;

                if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment.HasValue)
                    PDFDocumentViewModel.CATTFindingsViewModel.AssetPayment = invoiceAmt - PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment.Value;
                else
                    {
                    PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CATTRecommendedAdjustment = 0;
                    PDFDocumentViewModel.CATTFindingsViewModel.AssetPayment = invoiceAmt - 0;
                    }
                if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment.HasValue)
                    PDFDocumentViewModel.CATTFindingsViewModel.ApprovedPayment = invoiceAmt - PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment.Value;
                else
                    {
                    PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment = 0;
                    PDFDocumentViewModel.CATTFindingsViewModel.ApprovedPayment = invoiceAmt - 0;
                    }
                PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindingsEmp = eInvoiceModelContext.GetInvoiceCATTFindingsEmp(invoiceMasterID);

                if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindingsEmp != null)
                    {
                    //Manually Adding Totals in Grid Last row.
                    PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindingsEmp.Add(
                        new InvoiceCATTFindingsEmp
                        {
                            EmployeeName = "Total:",
                            RateVariance = PDFDocumentViewModel.InvoiceGridTotalsforPDF.CATTRateVariance.Value,
                            Total = PDFDocumentViewModel.InvoiceGridTotalsforPDF.CATTTotal.Value,
                        });
                    }

                PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCAFindingsEmp = eInvoiceModelContext.GetInvoiceCAFindingsEmp(invoiceMasterID);

                if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCAFindingsEmp != null)
                    {
                    //Manually Adding Totals in Grid Last row.
                    PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCAFindingsEmp.Add(
                        new InvoiceCAFindingsEmp
                        {
                            EmployeeName = "Total:",
                            RateVariance = PDFDocumentViewModel.InvoiceGridTotalsforPDF.CARateVariance.Value,
                            Total = PDFDocumentViewModel.InvoiceGridTotalsforPDF.CATotal.Value,
                        });
                    }

                PDFDocumentViewModel.CATTFindingsViewModel.ToCACSV = BuildEmployeeCSV(PDFDocumentViewModel.CATTFindingsViewModel.ToCA);
                PDFDocumentViewModel.CATTFindingsViewModel.FromCATTCSV = BuildEmployeeCSV(PDFDocumentViewModel.CATTFindingsViewModel.FromCATT);
                LogManager.Debug("Generate PDF: END");

            #endregion GeneratePDF
                }
        }


        public static void SavePDF(int invoiceMasterID, Controller callingController)
        {
            // int invoiceMasterID;
            string documentNo, invoiceNo, vendorNo, vendorName, poNumber, description;
            decimal? invoiceAmount;
            DateTime invoiceDate;

            PDFDocumentViewModel PDFDocumentViewModel;
 
            try
            {
                // Shared call to Generate PDF
                GeneratePDF(invoiceMasterID, out documentNo, out PDFDocumentViewModel);

                LogManager.Debug("SavePDF: START");

                SaveViewAsPDFFile("", "~/Views/eInvoicePDF/_GeneratePDF.cshtml", documentNo + "-eInvoiceApproval.pdf", PDFDocumentViewModel, callingController);

                    // ShortPayPDF - Short Pay letter should be uploaded as PDF to SharePoint only if there is CA Adjustment amount
                    // Return a true flag, if PDF should be uploaded
                    bool uploadShortPayPDF = false;

                    if (PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings != null)
                    {
                        decimal? approvedPaymentAmt = PDFDocumentViewModel.CATTFindingsViewModel.InvoiceCATTFindings.CARecommendedAdjustment;
                        if (approvedPaymentAmt != null && approvedPaymentAmt != Decimal.Zero)
                            uploadShortPayPDF = true;
                    }

                    if (uploadShortPayPDF)
                        SaveShortPayPDF(invoiceMasterID, callingController);

                    LogManager.Debug("SavePDF: END");

                    // 06/29/2015 - Post UAT Changes - In Progress - Build 6
                    LogManager.Debug("Upload to Sharepoint: START");
                    // Get Data to update SharePoint Document Upload Metadata
                    using (var eInvoiceModelContext = new eInvoiceModelContext())
                     {
                        // SM -- TTD - Replace
                        InvoiceMaster invoiceMasterData = eInvoiceModelContext.GetRoutingDetailsHeaderTemp(invoiceMasterID);
                        invoiceNo = invoiceMasterData.InvoiceNo;
                        vendorNo = invoiceMasterData.VendorNo;
                        vendorName = invoiceMasterData.VendorName;
                        poNumber = invoiceMasterData.ContractNo;
                        invoiceDate = invoiceMasterData.InvoiceDate;
                        invoiceAmount = invoiceMasterData.InvoiceAmount;
                        description = invoiceMasterData.Period;
                    };

                    string fileName = documentNo + "-eInvoiceApproval.pdf";
                    string fullFileNamewithPath = HttpContext.Current.Server.MapPath("~") + "\\PDFDocs\\" + fileName;
                    FileStream fileStream = new FileStream(fullFileNamewithPath, FileMode.Open);
                  
                    string pdfFileName = documentNo + "-eInvoiceApproval.pdf";
                    UploadDocToSharePoint(fileStream, pdfFileName, documentNo, invoiceNo, vendorNo, vendorName, poNumber, 
                        invoiceDate, invoiceAmount, description, uploadShortPayPDF);

                    // 08/31/2015 - Post Prod Changes
                    string completePath = HttpContext.Current.Server.MapPath("~") + "\\PDFDocs\\" + fileName;
                    if (System.IO.File.Exists(completePath))
                    {
                        System.IO.File.Delete(completePath);
                    }

                    // 08/31/2015 - Post Prod Changes
                    string shortPayFileName = documentNo + "-eInvoiceShortPayApproval.pdf";
                    string shortPayFilePath = HttpContext.Current.Server.MapPath("~") + "\\PDFDocs\\" + shortPayFileName;
                    if (System.IO.File.Exists(shortPayFilePath))
                    {
                        System.IO.File.Delete(shortPayFilePath);
                    }

                    fileStream.Close();

                    LogManager.Debug("Upload to Sharepoint: END");
              }
            catch (Exception ex)
                {
                LogManager.Error("SavePDF: ERROR " + ex.Message, ex);
                }
            }

        public static void GenerateShortPayPDF(int invoiceMasterID, out string documentNo, out ShortPayIndexViewModel shortPayIndexViewModel)
            {
            documentNo = "";
            shortPayIndexViewModel = new ShortPayIndexViewModel();
            try
                {
                LogManager.Debug("GenerateShortPayPDF: START");
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                    {
                    
                    //invoiceMasterID = eInvoiceModelContext.GetInvoiceMasterID(documentNo);
                    documentNo = eInvoiceModelContext.GetDocumentNoFromInvoiceMasterID(invoiceMasterID);
                    if (invoiceMasterID == 0) { throw new Exception("Document No. is not valid"); }

                    shortPayIndexViewModel.ShortPay = eInvoiceModelContext.GetShortPayDetails(invoiceMasterID);

                    //in case CA is not avilable during PDF Generation..
                    if (shortPayIndexViewModel.ShortPay.SentFrom != null)
                        {
                        //Get CA for InvoiceMasterID, RoleID of CA = 3
                        shortPayIndexViewModel.SentFrom = eInvoiceModelContext.GetCATTFindingsApprover(shortPayIndexViewModel.ShortPay.SentFrom);
                        //  shortPayIndexViewModel.ShortPay.SentFrom = BuildEmployeeCSV(shortPayIndexViewModel.SentFrom);
                        shortPayIndexViewModel.ShortPay.SentFrom = shortPayIndexViewModel.SentFrom.FirstOrDefault().ApproverName;
                        }
                    shortPayIndexViewModel.RoutingDetails = new RoutingDetailsViewModel();
                    shortPayIndexViewModel.RoutingDetails.InvoiceDetails = eInvoiceModelContext.GetRoutingDetailsHeader(invoiceMasterID);

                    shortPayIndexViewModel.ShortPayNotesDefault = eInvoiceModelContext.GetConfigMiscData().Where(p => p.ConfiguredCol == "ShortPayNotesDefault").FirstOrDefault().ConfiguredColText;

                    //in case CA is not avilable during PDF Generation..
                    if (shortPayIndexViewModel.ShortPay.SentFrom != null)
                        {
                        shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("@CAFullName", shortPayIndexViewModel.ShortPay.SentFrom);
                        shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("@emailaddress", shortPayIndexViewModel.SentFrom.FirstOrDefault().WorkEmail);
                        }
                    else
                        {
                        shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("@CAFullName", string.Empty);
                        shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("@emailaddress", string.Empty);
                        }
                    shortPayIndexViewModel.InvoiceGridTotalsforPDF = new InvoiceGridTotalsforPDF();
                    shortPayIndexViewModel.InvoiceGridTotalsforPDF = eInvoiceModelContext.GetInvoiceGridTotalsforPDF(invoiceMasterID);

                    string ApprovedAmt = String.Format("{0:C}", shortPayIndexViewModel.InvoiceGridTotalsforPDF.CATotal.Value);
                    shortPayIndexViewModel.ShortPayNotesDefault = shortPayIndexViewModel.ShortPayNotesDefault.Replace("$CAApprovedAdjustment", ApprovedAmt);

                    shortPayIndexViewModel.InvoiceCAFindingsEmp = new List<InvoiceCAFindingsEmp>();
                    //CA Line Items Grid..
                    shortPayIndexViewModel.InvoiceCAFindingsEmp = eInvoiceModelContext.GetInvoiceCAFindingsEmp(invoiceMasterID);

                    if (shortPayIndexViewModel.InvoiceCAFindingsEmp != null)
                        {
                        //Manually Adding Totals in Grid Last row.
                        shortPayIndexViewModel.InvoiceCAFindingsEmp.Add(
                            new InvoiceCAFindingsEmp
                            {
                                EmployeeName = "Total:",
                                RateVariance = shortPayIndexViewModel.InvoiceGridTotalsforPDF.CARateVariance.Value,
                                Total = shortPayIndexViewModel.InvoiceGridTotalsforPDF.CATotal.Value,
                            });
                        }
                    }
                LogManager.Debug("SaveShortPayPDF: START");
                }
            catch (Exception ex)
                {
                LogManager.Error("GenerateShortPayPDF: ERROR " + ex.Message, ex);
                //ViewBag.ErrorMessage = ex.Message;

                }
            }


        public static void SaveShortPayPDF(int invoiceMasterID, Controller callingController)
            {
            string documentNo = "";
            //if (string.IsNullOrEmpty(documentNo)) { throw new Exception("Document No. cannot be empty"); }

            ShortPayIndexViewModel shortPayIndexViewModel;
            try
                {
                LogManager.Debug("SaveShortPayPDF: START");
                GenerateShortPayPDF(invoiceMasterID, out documentNo, out shortPayIndexViewModel);

                LogManager.Debug("SaveShortPayPDF");
                SaveViewAsPDFFile("", "~/Views/ShortPayPDF/_GeneratePDF.cshtml", documentNo + "-eInvoiceShortPayApproval.pdf", shortPayIndexViewModel, callingController);
                LogManager.Debug("SaveShortPayPDF: END");
                }

            catch (Exception ex)
                {
                LogManager.Error("SaveShortPayPDF: ERROR " + ex.Message, ex);
                //ViewBag.ErrorMessage = ex.Message;
                
                }
            }

        public static bool SaveViewAsPDFFile(string pageTitle, string viewName, string fileName, object model, Controller routingController)
            {
            try
                {
                LogManager.Debug("SaveViewAsPDFFile: START");

                // Render the view html to a string.
                HtmlViewRenderer html = new HtmlViewRenderer();
                string htmlText = html.RenderViewToString(routingController, viewName, model);


                string cssfile = HttpContext.Current.Server.MapPath("~") + "\\Content\\Custom.css";

                System.IO.StreamReader myFile = new System.IO.StreamReader(cssfile);
                string cssText = myFile.ReadToEnd();

                myFile.Close();

                // Let the html be rendered into a PDF document through iTextSharp.
                StandardPdfRenderer spdfr = new StandardPdfRenderer();
                byte[] buffer = spdfr.Render(htmlText, pageTitle, cssText);

                string filepath = HttpContext.Current.Server.MapPath("~") + "\\PDFDocs\\" + fileName;

                System.IO.File.WriteAllBytes(filepath, buffer);

                LogManager.Debug("SaveViewAsPDFFile: END");

                return true;
                }

            catch (Exception ex)
                {
                LogManager.Error("SaveViewAsPDFFile: ERROR " + ex.Message, ex);
                return false;
                }
            }

        private static List<AccountingCostCodesViewModel> GetAccountingCostCodes(IEnumerable<InvoicePODetail> enumerable)
            {
            try
                {

                LogManager.Debug("GetAccountingCostCodes: START");

                List<AccountingCostCodesViewModel> resultList = new List<AccountingCostCodesViewModel>();

                foreach (InvoicePODetail p in enumerable)
                    {
                    AccountingCostCodesViewModel result = new AccountingCostCodesViewModel();
                    result.InvoiceDetailID = p.InvoiceDetailID;
                    result.InvoiceMasterID = p.InvoiceMasterID;
                    result.PONumber = p.PONumber;
                    result.POLine = p.POLine;
                    result.GLAccount = p.GLAccount;
                    result.CostCenter = p.CostCenter;
                    result.WBS = p.WBS;
                    result.Fund = p.Fund;
                    result.FunctionalArea = p.FunctionalArea;
                    result.GrantNumber = p.GrantNumber;
                    result.InternalOrder = p.InternalOrder;
                    result.InvoiceAmount = p.InvoiceAmount;
                    resultList.Add(result);
                    }
                LogManager.Debug("GetAccountingCostCodes: END");
                return resultList;
                }
            catch (Exception ex)
                {
                LogManager.Error("GetAccountingCostCodes: ERROR " + ex.Message, ex);
                return null;
                }
            }

        private static List<ModifyAccountingCostCodesViewModel> GetPOModifyAccountingCostCodes(IEnumerable<InvoicePODetailChanges> enumerable)
            {
            try
                {
                LogManager.Debug("GetPOModifyAccountingCostCodes: START");
                List<ModifyAccountingCostCodesViewModel> resultList = new List<ModifyAccountingCostCodesViewModel>();

                foreach (InvoicePODetailChanges p in enumerable)
                    {
                    ModifyAccountingCostCodesViewModel result = new ModifyAccountingCostCodesViewModel();
                    result.InvoiceDetailChangesID = p.InvoiceDetailChangesID;
                    result.InvoiceMasterID = p.InvoiceMasterID;
                    result.SAPPONumber = p.PONumber;
                    result.SAPPOLine = p.POLine;
                    result.GLAccount = p.GLAccount;
                    result.CostCenter = p.CostCenter;
                    result.WBSNo = p.WBS;
                    result.Fund = p.Fund;
                    result.FunctionalArea = p.FunctionalArea;
                    result.GrantNumber = p.GrantNumber;
                    result.InternalOrder = p.InternalOrder;
                    result.InvoiceAmount = (p.InvoiceAmount == null ? Decimal.Zero : p.InvoiceAmount);
                    result.Notes = p.Notes;
                    resultList.Add(result);
                    }
                LogManager.Debug("GetPOModifyAccountingCostCodes: END");
                return resultList;
                }
            catch (Exception ex)
                {
                LogManager.Error("GetPOModifyAccountingCostCodes: ERROR " + ex.Message, ex);
                return null;
                }
            }

        private static List<AttachmentsViewModel> GetRoutingAttachments(List<InvoiceAttachment> list)
            {
            try
                {
                LogManager.Debug("GetRoutingAttachments: START");
                List<AttachmentsViewModel> resultList = new List<AttachmentsViewModel>();
                foreach (InvoiceAttachment p in list)
                    {
                    AttachmentsViewModel result = new AttachmentsViewModel();
                    result.InvoiceAttachmentID = p.InvoiceAttachmentID;
                    result.InvoiceMasterID = p.InvoiceMasterID;
                    result.FileName = p.FileName;
                    result.FileAttachment = p.FileAttachment;
                    result.UploadedUserID = p.UploadedUserID;
                    result.LoggedUserID = HttpContext.Current.User.Identity.Name.ToString();
                    resultList.Add(result);
                    }
                LogManager.Debug("GetRoutingAttachments: END");
                return resultList;
                }
            catch (Exception ex)
                {
                LogManager.Error("GetRoutingAttachments: ERROR " + ex.Message, ex);
                return null;
                }
            }

        private static List<CommentsViewModel> GetRoutingComments(List<InvoiceComment> list)
            {
            try
                {
                LogManager.Debug("GetRoutingComments: START");
                List<CommentsViewModel> resultList = new List<CommentsViewModel>();
                foreach (InvoiceComment p in list)
                    {
                    CommentsViewModel result = new CommentsViewModel();
                    result.InvoiceCommentID = p.InvoiceCommentID;
                    result.InvoiceMasterID = p.InvoiceMasterID;
                    result.Comment = p.Comment;
                    result.CommentBy = p.CommentBy;
                    // result.CommentDate = p.CommentDate.Date.ToShortDateString();
                    result.CommentDate = p.CommentDate.ToString("MM/dd/yyyy hh:mm tt");
                    resultList.Add(result);
                    }
                LogManager.Debug("GetRoutingComments: END");

                return resultList;
                }
            catch (Exception ex)
                {
                LogManager.Error("GetRoutingComments: ERROR " + ex.Message, ex);
                return null;
                }
            }

        private static List<ApproversViewModel> GetRoutingApprovers(IEnumerable<InvoicePOApprover> invoicePOApprover)
            {
            try
                {
                LogManager.Debug("GetRoutingApprovers: START");
                List<ApproversViewModel> resultList = new List<ApproversViewModel>();

                foreach (InvoicePOApprover p in invoicePOApprover)
                    {
                    ApproversViewModel result = new ApproversViewModel();
                    result.InvoicePOApproverID = p.InvoicePOApproverID;
                    result.InvoiceMasterID = p.InvoiceMasterID;
                    result.RoleName = p.Role;
                    result.PONumberField = p.PONumber;
                    result.POLine = p.POLine;
                    result.Approver = ((p.ApproverUserID == null || p.ApproverUserID == String.Empty) ? String.Empty : FetchExchangeApprover(p.ApproverUserID));
                    result.ApproverSuggestedBySAP = p.ApproverSuggestedbySAP;
                    resultList.Add(result);
                    }
                LogManager.Debug("GetRoutingApprovers: END");
                return resultList;
                }

            catch (Exception ex)
                {
                LogManager.Error("GetRoutingApprovers: ERROR " + ex.Message, ex);
                return null;
                }
            }

        private static string BuildEmployeeCSV(IEnumerable<ExchangeEmployeeProfile> employees)
            {
            try
                {
                LogManager.Debug("BuildEmployeeCSV: START");
                string empcsv = string.Empty;
                foreach (ExchangeEmployeeProfile sepCATT in employees)
                    {
                    empcsv = empcsv + (string.IsNullOrEmpty(empcsv) ? sepCATT.ApproverName : ", " + sepCATT.ApproverName);
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

        public static string FetchExchangeApprover(string approverUserID)
            {
            try
                {
                LogManager.Debug("FetchApprover: START");
                List<ExchangeEmployeeProfile> approversList;
                string concatenatedName = string.Empty;
                using (var SAPSourceModelContext = new SAPSourceModelContext())
                    {
                    approversList = SAPSourceModelContext.FetchExchangeEmployeesList();
                    }
                if (approversList != null && approversList.Count > 0)
                    concatenatedName = (from approver in approversList where approver.UserID.ToLower() == approverUserID.ToLower() select approver.Concatenated).FirstOrDefault();
                LogManager.Debug("FetchApprover: END");
                return concatenatedName;
                }
            catch (Exception ex)
                {
                LogManager.Error("FetchApprover: ERROR " + ex.Message, ex);
                return null;
                }
            }

        #endregion

        }
    }