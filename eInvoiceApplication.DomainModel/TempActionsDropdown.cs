using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class TempActionsDropdown
    {
        [Key]
        public int ActionId { get; set; }
        public string Abbreviation { get; set; }
        public string ActionName { get; set; }
        public bool ExcludeFromMemory { get; set; }
    }
}
