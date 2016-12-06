using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class TempErrorMessage
    {
        [Key]
        public string TabName{ get; set; }
        public string ErrorMsg { get; set; }
    }
}
