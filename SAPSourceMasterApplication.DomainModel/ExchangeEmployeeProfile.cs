using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSourceMasterApplication.DomainModel
{
    public class ExchangeEmployeeProfile
    {
        public Nullable<int> EmployeeID { get; set; }
        public string UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string WorkEmail { get; set; }
        public string Department { get; set; }
        public string Division { get; set;}
        public string DistinguishedName { get;set; }
        public string WorkPhone { get; set; }

        public string ApproverUserID
        {
            get { return UserID.ToUpper(); }
        }

        public string ApproverName
        {
            //get { return string.Format("{0} {1}", FirstName, LastName); }
           // get { return string.Format("{0} {1} ({2})", FirstName, LastName, Department); }
            get { return string.Format("{0}", DistinguishedName); }
        }

    
        public string Concatenated
        {
            //get { return string.Format("{0} {1}", FirstName, LastName); }
            get { return string.Format("{0}", DistinguishedName); }
           
        }

     }
}
