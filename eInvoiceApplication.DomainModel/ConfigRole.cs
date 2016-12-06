using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
   public class ConfigRole
    {
        [Key]
        public int RoleID {get; set;}
        [Required]
        public string RoleAbbreviation { get; set;}
        [Required]
        public string Role {get; set;}
   }
}
