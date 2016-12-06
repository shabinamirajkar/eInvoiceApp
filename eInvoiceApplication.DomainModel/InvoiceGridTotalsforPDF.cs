using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class InvoiceGridTotalsforPDF
    {
        public decimal? CARateVariance { get; set; }
        public decimal? CATotal { get; set; }
        public decimal? CATTRateVariance { get; set; }
        public decimal? CATTTotal { get; set; }
        public decimal? POInvoiceAmt { get; set; }
        public decimal? POInvoiceAmtReadOnly { get; set; }
    }
}
