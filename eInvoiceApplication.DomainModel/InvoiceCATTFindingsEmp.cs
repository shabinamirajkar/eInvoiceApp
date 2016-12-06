using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eInvoiceApplication.DomainModel
{
    public class InvoiceCATTFindingsEmp
    {
       // EInvoiceModelContext eInvoiceModelContext;

        [Key]
        public int InvoiceCATTFindingsEmpID { get; set; }


        public int InvoiceCATTFindingsID { get; set; }

        //[ForeignKey("InvoiceCATTFindingsID")]
        //public InvoiceCATTFindings InvoiceCATTFindings { get; set; }

        public string EmployeeName { get; set; }

        public string Classification { get; set; }

        public Decimal? InvoiceRate { get; set; }

        public Decimal? ApprovedRate { get; set; }

        public Decimal? RateVariance { get; set; }

        public Decimal? InvoiceHours { get; set; }

        public Decimal? ApprovedHours { get; set; }

        public Decimal? VarianceHours { get; set; }

        public Decimal? Total { get; set; }

        public string Notes { get; set; }
       
    }
}
