using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;


namespace eInvoiceK2SAPBroker
{
    public static class eInvoiceLoadDocNoFromSAP
    {
        private static readonly log4net.ILog LogManager = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void LoadDocNoFromSAP(string docNo, string userId, string flagGetStatus = "Y", int InvoiceMasterID = 0, string roleStatus = "AP Review")
        {
            try
            {
                LogManager.Debug("LoadDocNoFromSAP: START");

                eInvoiceSAP eInvoiceSAPSMO = new eInvoiceSAP();
                DataTable dtSAPReturn = eInvoiceSAPSMO.eInvoiceLoadDocNoFromSAP_Bapi_Z_Invoice_To_K2(docNo, flagGetStatus);
                //int InvoiceMasterID=0;
                int rowNo=1;
                bool docNoExists = false;

                // TBD - Amit
                // Check if Doc No already exists in DB
                // If exists, need to update InvoiceMaster data, instead of inserting
                // InvoicePODetail data should be deleted & re-inerted in that scenario


                foreach (DataRow dr in dtSAPReturn.Rows)
                {
                    if ((rowNo == 1) && (!String.IsNullOrEmpty(dr["Vendor_Data_Vendor_No"].ToString())))
                    {
                        //Update Invoice Master, only once
                        InvoiceMaster_SMO invoiceMaster = new InvoiceMaster_SMO();

                        if (InvoiceMasterID != 0)
                        {
                            invoiceMaster.InvoiceMasterID = InvoiceMasterID;
                        }
                        else
                        {
                            invoiceMaster.DocumentNo = docNo;
                        }

                        DataTable dt = invoiceMaster.List();
                        
                        invoiceMaster.InvoiceNo = dr["Vendor_Data_Ac_Doc_No"].ToString();
                        //invoiceMaster.ContractNo = dr["Vendor_Data_Po_Number"].ToString();
                        //The first PO line on the invoice will be used to map the SAP PO data table
                        invoiceMaster.ContractNo = dr["Et_Invitems_Po_Number"].ToString();
                        invoiceMaster.InvoiceAmount = double.Parse(dr["Vendor_Data_Inv_Amount"].ToString());
                        invoiceMaster.VendorNo = dr["Vendor_Data_Vendor_No"].ToString();
                        invoiceMaster.VendorName = dr["Vendor_Data_Name"].ToString().Replace(".", "") + ' ' + dr["Vendor_Data_Name_2"].ToString().Replace(".", "");
                        invoiceMaster.Period = dr["Vendor_Data_Item_text"].ToString().Replace(".", "");
                        invoiceMaster.DocumentType = dr["Vendor_Data_Doc_Type"].ToString();
                        invoiceMaster.PaymentDueBy = Convert.ToDateTime(dr["Vendor_Data_Due_Date"].ToString());
                        invoiceMaster.PostedParkedBy = dr["Vendor_Data_Entered_by"].ToString();
                        // SM - TTD
                        invoiceMaster.InvoiceDate = Convert.ToDateTime(dr["Vendor_Data_Doc_Date"].ToString());
                       // if (!String.IsNullOrEmpty(dr["Vendor_Data_Doc_Status"].ToString()))
                       //   invoiceMaster.Status = dr["Vendor_Data_Doc_Status"].ToString();

                        //Project field on ‘Routing Details’ will be populated using the SAP PO Lines Master data. 
                        //The first PO line on the invoice will be used to map the SAP PO Lines Master data table
                       //invoiceMaster.Project = dr["Et_Invitems_Wbs_Element"].ToString();
                        invoiceMaster.APSubmittedByUserID = userId;
                        invoiceMaster.APSubmittedDate = DateTime.Now;
                        invoiceMaster.NonContractingStatus = false;
                        invoiceMaster.InvoiceType = "-1";

                        // FAP
                        if (InvoiceMasterID != 0)
                        {
                            invoiceMaster.DocumentNo = docNo;
                            invoiceMaster.Update(InvoiceMasterID);
                            docNoExists = true;
                        }
                        //AP
                        else if (dt.Rows.Count > 0 && dt.Rows[0]["Status"].ToString() != "Rejected")
                        {
                            InvoiceMasterID = int.Parse(dt.Rows[0]["InvoiceMasterID"].ToString());
                            invoiceMaster.Update(InvoiceMasterID);
                            docNoExists = true;
                        }
                        else
                        {
                            invoiceMaster.Status = "AP Review";
                            InvoiceMasterID = invoiceMaster.Create(invoiceMaster.DocumentNo, invoiceMaster.InvoiceNo).InvoiceMasterID.Value;
                        }
                    }

                   
                    //excluding last row to prevent the row from being duplicated
                    if (rowNo == dtSAPReturn.Rows.Count) { continue; }
                   

                    //Update ImvoicePODetail --- all PO line items
                    InvoicePODetail_SMO invoicePODetail = new InvoicePODetail_SMO();
                    if (rowNo == 1 && docNoExists)
                        //Delete All Record mapped to InvoiceMasterID
                        invoicePODetail.DeleteByInvoiceMasterID(InvoiceMasterID);
                    string poNo = Convert.ToString(dr["Et_Invitems_Po_Number"]);

                    if ((!String.IsNullOrEmpty(poNo)) ||
                        (!String.IsNullOrEmpty(Convert.ToString(dr["Et_Invitems_Gl_Account"]))) ||
                        (!String.IsNullOrEmpty(Convert.ToString(dr["Et_Invitems_Costcenter"]))) ||
                        (!String.IsNullOrEmpty(Convert.ToString(dr["Et_Invitems_Wbs_Element"]))) ||
                        (!String.IsNullOrEmpty(Convert.ToString(dr["Et_Invitems_Fund"]))) ||
                        (!String.IsNullOrEmpty(Convert.ToString(dr["Et_Invitems_Grant_Nbr"]))) ||
                        (!String.IsNullOrEmpty(Convert.ToString(dr["Et_Invitems_Order_Number"]))) ||
                        (!String.IsNullOrEmpty(Convert.ToString(dr["Et_Invitems_Amount"]))))
                    
                        {
                            if (!String.IsNullOrEmpty(Convert.ToString(dr["Et_Invitems_Po_Number"])))
                                invoicePODetail.PONumber = Convert.ToString(dr["Et_Invitems_Po_Number"]);
                            
                            if (!String.IsNullOrEmpty(Convert.ToString(dr["Et_Invitems_Po_Item"])))
                                invoicePODetail.POLine = Convert.ToInt16(Convert.ToString(dr["Et_Invitems_Po_Item"]));
                            if (!String.IsNullOrEmpty(Convert.ToString(dr["Et_Invitems_Gl_Account"])))
                                invoicePODetail.GLAccount = int.Parse(dr["Et_Invitems_Gl_Account"].ToString());
                            if (!String.IsNullOrEmpty(dr["Et_Invitems_Costcenter"].ToString()))
                                invoicePODetail.CostCenter = int.Parse(dr["Et_Invitems_Costcenter"].ToString());
                            if (!String.IsNullOrEmpty(dr["Et_Invitems_Wbs_Element"].ToString()))
                                invoicePODetail.WBS = Convert.ToString(dr["Et_Invitems_Wbs_Element"]);
                                
                             if (!String.IsNullOrEmpty(dr["Et_Invitems_Fund"].ToString()))
                                invoicePODetail.Fund = int.Parse(Convert.ToString(dr["Et_Invitems_Fund"]));
                             if (!String.IsNullOrEmpty(dr["Et_Invitems_Func_Area"].ToString()))
                                invoicePODetail.FunctionalArea = Convert.ToString(dr["Et_Invitems_Func_Area"]);
                            if (!String.IsNullOrEmpty(Convert.ToString(dr["Et_Invitems_Grant_Nbr"])))
                                invoicePODetail.GrantAmt = Convert.ToString(dr["Et_Invitems_Grant_Nbr"]).TrimStart('0');
                            if (!String.IsNullOrEmpty(Convert.ToString(dr["Et_Invitems_Order_Number"])))
                                invoicePODetail.InternalOrder =  Convert.ToString(dr["Et_Invitems_Order_Number"]);
                            if (!String.IsNullOrEmpty(Convert.ToString(dr["Et_Invitems_Amount"])))
                                invoicePODetail.InvoiceAmount = double.Parse(Convert.ToString(dr["Et_Invitems_Amount"]));
                            invoicePODetail.Create(InvoiceMasterID);
                        }

                    rowNo++;
                }
                LogManager.Debug("LoadDocNoFromSAP: END");

            }
            catch (Exception ex)
            {
                LogManager.Error("LoadDocNoFromSAP: ERROR " + ex.Message, ex);
                //sds
            }
        }

        // Validate Doc No with SAP to see if there is a block on the Invoice
        // If Vendor_Data_PMNT_Blktx = “Free for Payment”, we allow user to continue.
        // Else display message saying: "Please release the hold in SAP before you Approve this Invoice"
        public static string ValidateDocNoWithSAP(string docNo, out string docStatus,  string flagGetStatus = "Y")
        {
            docStatus = string.Empty;
            try
            {
                LogManager.Debug("ValidateDocNoWithSAP: START");
               
                eInvoiceSAP eInvoiceSAPSMO = new eInvoiceSAP();
                DataTable dtSAPReturn = eInvoiceSAPSMO.eInvoiceLoadDocNoFromSAP_Bapi_Z_Invoice_To_K2(docNo, flagGetStatus);

                string blockText = string.Empty;

                if (dtSAPReturn.Rows.Count > 0)
                {
                    blockText = dtSAPReturn.Rows[0]["Vendor_Data_PMNT_Blktx"].ToString();
                    docStatus = dtSAPReturn.Rows[0]["Vendor_Data_Doc_Status"].ToString();
                }

                LogManager.Debug("ValidateDocNoWithSAP: END");
                return blockText;
                
            }
            catch (Exception ex)
            {
                LogManager.Error("ValidateDocNoWithSAP: ERROR " + ex.Message, ex);
                return string.Empty;
            }
        }
    }
}
