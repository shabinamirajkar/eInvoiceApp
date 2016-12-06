using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSourceMasterApplication.DomainModel
{
   public class SAPEmployeeProfile
    {
        [Key]
        public int EmployeeID { get; set; }
        public string UserID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleInitial { get; set; }
        public string Gender { get; set; }
        public DateTime HireDate { get; set; }
        public string ActivityType { get; set; }
        public string PersonnalArea { get; set; }
        public string PersonnalAreaDescription { get; set; }
        public string ExemptStatus { get; set; }
        public int EmployeeSubgroup { get; set; }
        public int SupervisorID { get; set; }
        public string SupervisorTitle { get; set; }
        public string SupervisorFirstName { get; set; }
        public string SupervisorLastName { get; set; }
        public int CostCenter { get; set; }
        public int Job { get; set; }
        public string JobDescription { get; set; }
        public int Position { get; set; }
        public string PositionDescription { get; set; }
        public int OrgUnit { get; set; }
        public string OrgUnitDescription { get; set; }
        public string WorkScheduleRule { get; set; }
        public string WorkPhone { get; set; }
        public string WorkCellPhone { get; set; }
        public string WorkEmail { get; set; }
        public string WorkLocation { get; set; }
        public bool Supervisor { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        public string ApproverUserID
        {
            get { return UserID.ToUpper(); }
        }

        public string ApproverName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }

        public string Concatenated
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }
    }
}
