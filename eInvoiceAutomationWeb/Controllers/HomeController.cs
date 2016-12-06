using eInvoiceApplication.DomainModel;
using Microsoft.SharePoint.Client;
using SAPSourceMasterApplication.DomainModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace eInvoiceAutomationWeb.Controllers
{
      
    public class HomeController : Controller
    {
        private static readonly log4net.ILog LogManager = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActionResult Home()
        {
            try
            {
                LogManager.Debug("Home: START");
                FetchUserName();
                LogManager.Debug("Home: END");
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error("Home: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        public ActionResult SessionTimeOut()
            {
            try
                {
                LogManager.Debug("SessionTimeOut: START");
                FetchUserName();
                LogManager.Debug("SessionTimeOut: END");
                return View();
                }
            catch (Exception ex)
                {
                LogManager.Error("SessionTimeOut: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }

        public ActionResult Test()
            {
            try
                {
                LogManager.Debug("Test: START");
                FetchUserName();

                string SPUrl = System.Configuration.ConfigurationManager.AppSettings["SharePointSiteURL"].ToString();
                string SPFolder = System.Configuration.ConfigurationManager.AppSettings["SharePointFolder"].ToString();
                string docLibName = System.Configuration.ConfigurationManager.AppSettings["SharePointDocLibrary"].ToString();

                using (ClientContext clientContext = new ClientContext(SPUrl))
                    {
                    try
                        {
                        
                        Web web = clientContext.Web;

                        List oList = clientContext.Web.Lists.GetByTitle("eInvoiceDocsList");

                        ListItem lstItem;
                        var qry = new CamlQuery();
                        qry.ViewXml = "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name='DocumentNumber' /><Value Type='Text'>" + "DDDD" + "</Value></Eq></Where></Query></View>";
                        //qry.ViewXml = string.Format("<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"FileRef\"/><Value Type=\"Url\">{0}</Value></Eq></Where></Query></View>", fileUrl);
                        ListItemCollection items = oList.GetItems(qry);
                        clientContext.Load(items);
                        clientContext.ExecuteQuery();
                        if (items.Count > 0)
                            {
                            lstItem = items[0];
                            }
                        else
                            {
                            ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                            lstItem = oList.AddItem(itemCreateInfo);
                            }
                        lstItem["DocumentNumber"] = "XXXX";
                        lstItem["InvoiceNumber"] = "IIII";
                        FieldUrlValue url = new FieldUrlValue();
                        url.Url = SPUrl + "/" + "VVVV" + "/" + "IIII";
                        url.Description = "View eInvoiceApprovalDocs";
                        lstItem["ApprovalDocs"] = url;
                        lstItem.Update();

                        clientContext.ExecuteQuery(); 
                       //List list = clientContext.Web.Lists.GetByTitle(docLibName);

                        //List oList = clientContext.Web.Lists.GetByTitle("eInvoiceDocsList");

                        //ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                        //ListItem ListItem = oList.AddItem(itemCreateInfo);
                        //lstItem["DocumentNumber"] = 'YYY';
                        //lstItem["InvoiceNumber"] = 'III';
                        //FieldUrlValue url = new FieldUrlValue();
                        //url.Url = SPUrl + "/" + vendorNo + "/" + invoiceNo;
                        //url.Description = "View eInvoiceApprovalDocs";
                        //lstItem["ApprovalDocs"] = url;
                        //lstItem.Update();

                        clientContext.ExecuteQuery(); 


                        ListCollection listCollection = clientContext.Web.Lists;

                        clientContext.Load(listCollection, lists => lists.Include(list => list.Title).Where(list => list.Title == "XXXXX"));

                        clientContext.ExecuteQuery();

                        if (listCollection.Count > 0)
                            {
                            Response.Write("List all ready exist...");
                            }
                        else
                            {
                            Response.Write("List not exist");
                            }

                        if (0 == 0)
                            {
                            string lName;

                            lName = "eInvoiceVendorLib";
                            ListCreationInformation lci = new ListCreationInformation();

                            ListTemplateCollection ltc = clientContext.Site.GetCustomListTemplates(clientContext.Web);
                            clientContext.Load(ltc,
                                    listX => listX
                                        .Include(i => i.IsCustomTemplate, i => i.FeatureId, i => i.Name, i => i.ListTemplateTypeKind)
                                        .Where(l => l.Name == lName));
                            clientContext.ExecuteQuery();

                            lci.TemplateType = ltc[0].ListTemplateTypeKind;
                            lci.TemplateFeatureId = ltc[0].FeatureId;
                            lci.Description = docLibName + " Document Library";
                            lci.Title = docLibName;

                            List newLib = clientContext.Web.Lists.Add(lci);
                            //clientContext.Load(newLib);
                            newLib.Update();
                            clientContext.ExecuteQuery();
                            }

                        clientContext.Load(clientContext.Site);


                        }
                    catch (Exception ex)
                        {
                        LogManager.Debug("DeleteDocFromSharePoint: Error" + ex.Message);
                        
                        }
                    }
                //string SPUrl = System.Configuration.ConfigurationManager.AppSettings["SharePointSiteURL"].ToString();
                //string SPFolder = System.Configuration.ConfigurationManager.AppSettings["SharePointFolder"].ToString();
                //string docLibName = System.Configuration.ConfigurationManager.AppSettings["SharePointDocLibrary"].ToString();
                //string pdfFileName = "eInvoiceApproval-5100099542.pdf";
                //string documentNo = "15100113996";

                //SPCreateFolderwithCheckExists(SPUrl, docLibName, SPFolder + "/" + documentNo);
                //using (ClientContext clientContext = new ClientContext(SPUrl))
                //    {
                //    try
                //        {
                //        LogManager.Debug("Test Upload to SP: Start");
                //        List documentLibrary = clientContext.Web.Lists.GetByTitle("Documents");


                         

                //   //string fileName = "C:\\Users\\Pabathi_N\\K2\\MVC\\eInvoiceAutomationPublish\\PDFDocs\\eInvoice-5100099542.pdf";
                //   FileCreationInformation newUploadFile = new FileCreationInformation();

                //   //newUploadFile.Overwrite = true;
                //   //newUploadFile.Url = SPUrl + "/" + docLibName + "/" + SPFolder + "/" + documentNo + "/" + pdfFileName;


                //   var folder = clientContext.Web.GetFolderByServerRelativeUrl(SPUrl + " / " + docLibName + " / " + SPFolder + " / " + documentNo);

                  
                //   ListItemCreationInformation newItem = new ListItemCreationInformation();
                //   //newItem.UnderlyingObjectType =  FileSystemObjectType.Folder;

                //   ////newItem.FolderUrl = SPUrl + "/lists/" + listName;
                //   //newItem.FolderUrl = SPUrl + "/" + docLibName + "/" + SPFolder + "/" + documentNo;
                //   //ListItem item = documentLibrary.AddItem(newItem);

                //   //item.Update();

                  


                //  // documentLibrary.RootFolder.Folders.Add(SPUrl + "/" + docLibName + "/" + SPFolder + "/" + documentNo);
                //   documentLibrary.RootFolder.Folders.Add(SPFolder + "/" + documentNo);
                //   documentLibrary.Update();
                //   clientContext.ExecuteQuery();

                //        string fileName = "C:\\Users\\Pabathi_N\\K2\\MVC\\eInvoiceAutomationPublish\\PDFDocs\\eInvoice-5100099542.pdf";
                //        //FileCreationInformation newUploadFile = new FileCreationInformation();
                //        FileStream fileStream = new FileStream(fileName, FileMode.Open);
                //        newUploadFile.Overwrite = true;
                //        newUploadFile.Url = SPUrl + "/" + docLibName + "/" + SPFolder + "/" + pdfFileName;
                //        byte[] fileContents = new byte[fileStream.Length];
                //        newUploadFile.Content = fileContents; // "Add File Stream Array";
                //        Microsoft.SharePoint.Client.File uploadFile = documentLibrary.RootFolder.Files.Add(newUploadFile);
                //        //Set Metadata for uploaded document
                //        uploadFile.ListItemAllFields["Title"] = "ePDF Doc1";
                //        uploadFile.ListItemAllFields["DocumentNumber"] = "5100099542";
                //        uploadFile.ListItemAllFields["InvoiceNumber"] = "IA5100099542";
                //        uploadFile.ListItemAllFields.Update();
                //        clientContext.ExecuteQuery();
                //        fileStream.Close();
                //        LogManager.Debug("Test Upload to SP: End");
                //        }
                //    catch (Exception ex)
                //        {
                //        LogManager.Debug("Test Upload to SP: Error" + ex.Message);
                //        throw ex;
                //        }
                //    }

                LogManager.Debug("Test: END");
                //return PartialView("Home");
                return PartialView("TestUpload");
                }
            catch (Exception ex)
                {
                LogManager.Error("Test: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }


        [HttpPost]
        public JsonResult TestUpload(string id)
            {
            try
                {
                
                }
            catch (Exception)
                {
                
                return Json("Upload failed");
                }

            return Json("File uploaded successfully");
            }

        public static void SPCreateFolderwithCheckExists(string SPUrl, string docLibName, string folderName)
            {
            ClientContext clientContext = new ClientContext(SPUrl);
            Web web = clientContext.Web;
            List list = clientContext.Web.Lists.GetByTitle(docLibName);



            if (list == null)
                {
                string lName = "eInvoiceVendorLib";
                ListCreationInformation lci = new ListCreationInformation();

                ListTemplateCollection ltc = clientContext.Site.GetCustomListTemplates(clientContext.Web);
                clientContext.Load(ltc,
                        listX => listX
                            .Include(i => i.IsCustomTemplate, i => i.FeatureId, i => i.Name, i => i.ListTemplateTypeKind)
                            .Where(l => l.Name == lName));
                clientContext.ExecuteQuery();

                lci.TemplateType = ltc[0].ListTemplateTypeKind;
                lci.TemplateFeatureId = ltc[0].FeatureId;
                lci.Description = docLibName + " Document Library";
                lci.Title = docLibName;

                List newLib = clientContext.Web.Lists.Add(lci);
                //clientContext.Load(newLib);
                newLib.Update();
                clientContext.ExecuteQuery();
                }

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
                newItemInfo.LeafName = folderName;
                ListItem newListItem = list.AddItem(newItemInfo);

                newListItem["ContentTypeId"] = folderContentType.Id.ToString();
                newListItem["Title"] = folderName;
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

        //public ActionResult TestUpload()
        //{
        //    return View("TestUpload");
        //}

        //[HttpPost]
        //public ActionResult UploadFile()
        //{
        //    using (var context = new eInvoiceModelContext())
        //    {
        //        foreach (string file in Request.Files)
        //        {
        //            HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
        //            if (hpf.ContentLength == 0)
        //                continue;
        //            using (var memoryStream = new MemoryStream())
        //            {
        //                hpf.InputStream.CopyTo(memoryStream);
        //                var uploadAttachment = new InvoiceAttachment
        //                {
        //                    FileAttachment = memoryStream.ToArray(),
        //                    InvoiceMasterID = 404,
        //                    FileName = Path.GetFileName(hpf.FileName),
        //                    UploadedUserID = HttpContext.User.Identity.Name
        //                };
        //                var result = context.SaveInvoiceAttachment(uploadAttachment);
        //                uploadAttachment.InvoiceAttachmentID = result;
        //            }
        //        }
        //    }
        //    return Json(new { data = "" }, "text/html");
        //}

        //[HttpPost]
        //public ActionResult UploadFile(HttpPostedFileBase file)
        //{
        //    if (file.ContentLength > 0)
        //    {
        //        using (var context = new eInvoiceModelContext())
        //        {
        //           using (var memoryStream = new MemoryStream())
        //           {
        //               file.InputStream.CopyTo(memoryStream);
        //               var uploadAttachment = new InvoiceAttachment
        //               {
        //                    FileAttachment = memoryStream.ToArray(),
        //                    InvoiceMasterID = 404,
        //                    FileName = Path.GetFileName(file.FileName),
        //                    UploadedUserID = HttpContext.User.Identity.Name
        //                };
        //               var result = context.SaveInvoiceAttachment(uploadAttachment);
        //               uploadAttachment.InvoiceAttachmentID = result;
        //          }
        //      }
        //   }
        //    return View("TestUpload");
           
        //}

       

        public ActionResult Result(string result)
        {
            try
            {
               LogManager.Debug("Result: START");
               ViewBag.Result = result;
               LogManager.Debug("Result: END");
               return View();
            }
            catch (Exception ex)
            {
                LogManager.Error("Result: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

                     
        [AuthorizationPrivilegeFilter]
        public ActionResult eInvoice()
        {
            try
            {
                LogManager.Debug("eInvoice: START");
                FetchUserName();
                TempData["DocumentNo"] = "0";
                ViewBag.ShowAllTabs = false;
                // 06/29/2015 - Post UAT Changes - Build 6
                // ViewBag.ReadOnly = false;
                TempData["ReadOnly"] = false;
                ViewBag.ShowPODetails = false;
                Session["InvoiceMasterID"] = null;
                LogManager.Debug("eInvoice: END");
                //return View();
                return View("eInvoiceApprovalForm");
            }
            catch (Exception ex)
            {
                LogManager.Error("eInvoice: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        public ActionResult SavedeInvoice(string documentNo, string status, int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("SavedeInvoice: START");
                FetchUserName();
                TempData["DocumentNo"] = documentNo;
                ViewBag.Status = status;
                ViewBag.ShowAllTabs = false;
                // 06/29/2015 - Post UAT Changes - Build 6
                // ViewBag.ReadOnly = false;
                TempData["ReadOnly"] = false;
                ViewBag.ShowPODetails = true;
                ViewBag.InvoiceMasterID = invoiceMasterID;
                LogManager.Debug("SavedeInvoice: END");
                //return View();
                return View("eInvoiceApprovalForm");
            }
            catch (Exception ex)
            {
                LogManager.Error("SavedeInvoice: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        public ActionResult ViewReport(string documentNo, string status, int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("ViewReport: START");
                FetchUserName();
                TempData["DocumentNo"] = documentNo;
                // 06/29/2015 - Post UAT Changes - Build 6 
                // ViewBag.ReadOnly = true;
                TempData["ReadOnly"] = true;
                ViewBag.status = status;
                ViewBag.ShowPODetails = true;
                ViewBag.InvoiceMasterID = invoiceMasterID;
                LogManager.Debug("ViewReport: END");
                //return View();
                return View("eInvoiceApprovalForm");
            }
            catch (Exception ex)
            {
                LogManager.Error("ViewReport: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        public ActionResult Worklist()
        {
            try
            {
                LogManager.Debug("Worklist: START");
                FetchUserName();
                TempData["DocumentNo"] = "0";
                ViewBag.ShowAllTabs = false;
                // 06/29/2015 - Post UAT Changes - Build 6 
                // ViewBag.ReadOnly = false;
                TempData["ReadOnly"] = false;
                ViewBag.ShowPODetails = false;
                LogManager.Debug("Worklist: END");
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error("Worklist: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        [AuthorizationPrivilegeFilter]
        public ActionResult ReadOnlyeInvoice(string documentNo,string status,string SN, string SharedUser = "")
        {
            try
            {
                LogManager.Debug("ReadOnlyeInvoice: START");
                FetchUserName();
                TempData["DocumentNo"] = documentNo;
                ViewBag.Status = status;
                ViewBag.SN = SN;
                ViewBag.SharedUser = SharedUser;
                // 06/29/2015 - Post UAT Changes - Build 6 
                // ViewBag.ReadOnly = false;
                TempData["ReadOnly"] = false;
                ViewBag.ShowPODetails = true;
                LogManager.Debug("ReadOnlyeInvoice: END");
                //return View();
                return View("eInvoiceApprovalForm");
            }
            catch (Exception ex)
            {
                LogManager.Error("ReadOnlyeInvoice: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        public ActionResult AdminTools()
        {
            try
            {
                LogManager.Debug("AdminTools: START");
                LogManager.Debug("AdminTools: END");
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error("AdminTools: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }


        public ActionResult AccessDenied()
        {
            try
            {
                LogManager.Debug("AccessDenied: START");
                LogManager.Debug("AccessDenied: END");
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error("AccessDenied: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
            }
        }

        public ActionResult ActionedOrCompleted()
            {
            try
                {
                LogManager.Debug("ActionedOrCompleted: START");
                LogManager.Debug("ActionedOrCompleted: END");
                return View();
                }
            catch (Exception ex)
                {
                LogManager.Error("ActionedOrCompleted: ERROR " + ex.Message, ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("Error");
                }
            }

        // SM -- Test Upload File Start
        public ActionResult Async()
            {
            return View();
            }

        public ActionResult Save(IEnumerable<HttpPostedFileBase> files)
            {
            // The Name of the Upload component is "files"
            if (files != null)
                {
                foreach (var file in files)
                    {
                    // Some browsers send file names with full path.
                    // We are only interested in the file name.
                    var fileName = Path.GetFileName(file.FileName);
                    var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);

                    // The files are not actually saved in this demo
                    // file.SaveAs(physicalPath);
                    }
                }

            // Return an empty string to signify success
            return Content("");
            }

        public ActionResult Remove(string[] fileNames)
            {
            // The parameter of the Remove action must be called "fileNames"

            if (fileNames != null)
                {
                foreach (var fullName in fileNames)
                    {
                    var fileName = Path.GetFileName(fullName);
                    var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);

                    // TODO: Verify user permissions

                    if (System.IO.File.Exists(physicalPath))
                        {
                        // The files are not actually removed in this demo
                        // System.IO.File.Delete(physicalPath);
                        }
                    }
                }

            // Return an empty string to signify success
            return Content("");
            }

        // SM -- Test Upload File End

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
