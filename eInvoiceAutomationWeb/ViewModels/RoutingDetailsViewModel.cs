using eInvoiceApplication.DomainModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace eInvoiceAutomationWeb.ViewModels
{
    public class RoutingDetailsViewModel
    {
        public RoutingDetailsViewModel()
        {  
        
        }

     
        #region Properties
       // [RegularExpression("/[~`!#%^&*+=[]\\';,./{}|:<>?@]/",ErrorMessage="Please enter valid Document No")]
        public string DocumentNo { get; set; }
        public string SN { get; set; }
        public bool ShowSaveButton { get; set; }
        public bool ShowApproveButton { get; set; }
        public bool ShowRejectButton { get; set; }
        public InvoiceMaster InvoiceDetails { get; set; }
        // TTD - SM Replace
        //public InvoiceMaster InvoiceDetails { get; set; }
        public IEnumerable<InvoicePOApprover> InvoiceApproversList { get; set; }
        public List<InvoiceComment> InvoiceComments { get; set; }
        public List<InvoiceAttachment> InvoiceAttachments { get; set; }
        public List<ConfigRole> ConfigRoles { get; set; }
        public string Comment { get; set; }
        public bool ShowPOWarning { get; set; }
        public string ActionText { get; set; }
        public string DocumentType { get; set; }
        public bool NonContractingStatus { get; set; }
        public string SharedUser { get; set; }
        public string InvoiceType { get; set; }
       
        #endregion

    }


    public class ApproversViewModel
    {
        public int InvoicePOApproverID { get; set; }
        public int InvoiceMasterID { get; set; }
        [DisplayName("PO")]
        public string PONumberField { get; set; }
        public Nullable<int> POLine { get; set; }
        public string RoleName { get; set; }
        public string ApproverUserId { get; set; }
        public string Approver { get; set; }
        public string ApproverSuggestedBySAP { get; set; }

        public string FormattedPONumber {
            get
            {
                if (PONumberField != null)
                    return PONumberField.TrimStart('0');
                else
                    return "";
            }
        }
    }

    public class AttachmentsViewModel
    {
        public int InvoiceAttachmentID { get; set; }
        public int InvoiceMasterID { get; set; }
        public string FileName { get; set; }
        public byte[] FileAttachment { get; set; }
        public string UploadedUserID { get; set; }
        public string LoggedUserID { get; set; }
    }

    public class CommentsViewModel
    {
        public int InvoiceCommentID { get; set; }
        public int InvoiceMasterID { get; set; }
        public string Comment { get; set; }
        public string CommentBy { get; set; }
        public string CommentDate { get; set; }
    }

}
