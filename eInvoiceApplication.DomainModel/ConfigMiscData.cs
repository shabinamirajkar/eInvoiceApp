using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class ConfigMiscData
    {
        [Key]
        public int ConfigID { get; set; }
      // [ReadOnly(true)] 
        public string ConfiguredCol { get; set; }
        public string ConfiguredColText { get; set; }
    }
}

