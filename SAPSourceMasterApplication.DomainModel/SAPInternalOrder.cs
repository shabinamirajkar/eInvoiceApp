using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSourceMasterApplication.DomainModel
{
    public class SAPInternalOrder
    {
        [Key]
        public string InternalOrderNo { get; set; }
        public string Description { get; set; }
        public string ResponsibleCostCenter { get; set; }
        public string PersonResponsible { get; set; }
    }
}
