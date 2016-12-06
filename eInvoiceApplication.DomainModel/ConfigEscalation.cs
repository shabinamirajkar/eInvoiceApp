using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class ConfigEscalation
    {
        [Key]
        public int ConfigEscalationsID { get; set; }

        public string ProcessName { get; set; }
        public string ActivityName { get; set; }
        public int? FirstReminderDays { get; set; }
        public int? SecondReminderDays { get; set; }
        public int? ExpiresAfter { get; set; }
    }
}

