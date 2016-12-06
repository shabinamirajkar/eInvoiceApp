using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSourceMasterApplication.DomainModel
{
    public class SAPCostCenter
    {
        [Key]
        public int CostCenterNo { get; set; }
        public string Description { get; set; }
        public string PersonResponsible { get; set; }
    }
}
