using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class ConfigRouteRole
    {
        [Key]
        public int ConfigRouteRoleID { get; set; }
        public int RoleID { get; set; }
        [ForeignKey("RoleID")]
        public ConfigRole ConfigRole { get; set; }
        public int RouteToRoleID { get; set; }
    }
}
