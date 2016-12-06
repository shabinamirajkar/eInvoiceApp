using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class AdminRole
    {
        [Key]
        public int AdminRoleID { get; set; }

        public int RoleID { get; set; }
        [ForeignKey("RoleID")]
        public ConfigRole ConfigRole { get; set; }
        public string Approver { get; set; }
    }

    public class AdminRoleForCRUD
    {
        public int AdminRoleID { get; set; }

        public int RoleID { get; set; }
      //  [Required(AllowEmptyStrings = false, ErrorMessage = "Field required!")]
        public string RoleAbbreviation { get; set; }

        public string ApproverUserID { get; set; }
       // [Required(AllowEmptyStrings = false, ErrorMessage = "Field required!")]
        public string ApproverName { get; set; }

    }
}

