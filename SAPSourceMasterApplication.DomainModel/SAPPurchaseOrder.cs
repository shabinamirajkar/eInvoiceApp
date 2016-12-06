using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSourceMasterApplication.DomainModel
{
    public class SAPPurchaseOrder
    {
        public string PONumber { get; set; }
        public int POLine { get; set; }
        public string PurchasingGroup { get; set; }
        public string Project { get; set; }
        public string Recipient { get; set; }
        public string Requisitioner { get; set; }
        public string AccountAssignCat { get; set; }
        public int? GLAccount { get; set; }
        public DateTime ValidityEndDate { get; set; }

        public String FormattedPONumber
        {
            get { return PONumber.TrimStart('0'); }
        }
    }
}
