using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class StatusReportDropdown
    {
        [Key]
        public int RoleID { get; set; }
        public string RoleAbbreviation { get; set; }
    }
}
