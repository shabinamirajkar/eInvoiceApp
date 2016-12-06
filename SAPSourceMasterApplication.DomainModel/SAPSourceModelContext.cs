using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSourceMasterApplication.DomainModel
{
    public class SAPSourceModelContext : DbContext
    {
        public SAPSourceModelContext() : base("SAPSourceModelDbContextConnection")
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }


        public IEnumerable<SAPEmployeeProfile> FetchApproversList(string filtercondition)
        {
            using (var context = new SAPSourceModelContext())
            {
                var result = context.Database.SqlQuery<SAPEmployeeProfile>("uspSAPGetAllEmployees").Distinct().ToList().Where(p => p.ApproverName.ToLower().Contains(filtercondition.ToLower()));
                return result;
            }
        }


        public List<SAPEmployeeProfile> FetchApproversList()
        {
            using (var context = new SAPSourceModelContext())
            {
                var result = context.Database.SqlQuery<SAPEmployeeProfile>("uspSAPGetAllEmployees").ToList();
                return result;
            }
        }

        public List<ExchangeEmployeeProfile> FetchExchangeEmployeesList()
        {
            using (var context = new SAPSourceModelContext())
            {
                var result = context.Database.SqlQuery<ExchangeEmployeeProfile>("uspSAPGetAllExchangeEmployees").ToList();
                return result;
            }
        }

        public List<ExchangeEmployeeProfile> GetSAPExchangeEmployeeFilter(string approverFilter)
        {
            using (var context = new SAPSourceModelContext())
            {
                var result = context.Database.SqlQuery<ExchangeEmployeeProfile>("uspSAPGetAllExchangeEmployees").Distinct().ToList().Where(p => p.Concatenated.ToLower().Contains(approverFilter.ToLower())).ToList();
                return result;
            }
        }

        public string FetchLoggedInUserName(string userId)
        {
            using (var context = new SAPSourceModelContext())
            {
                var clientIdParameter = new SqlParameter("@UserId", userId);
                var result = context.Database.SqlQuery<ExchangeEmployeeProfile>("uspSAPFetchLoggedInUserName @UserId", clientIdParameter).FirstOrDefault();
                return result.FirstName+" "+result.LastName;
            }
        }

        public List<SAPPurchaseOrder> GetPONumbers()
        {
            using (var context = new SAPSourceModelContext())
            {
                var result = GetSAPPurchaseOrders();
                //var poNumbers = (from r in result select r.PONumber).Distinct().ToList();
                return result;
            }
        }

       
        public List<SAPPurchaseOrder> GetPOLines()
        {
            using (var context = new SAPSourceModelContext())
            {
                var result = GetSAPPurchaseOrders();
                var poNumbers = (from r in result select r.PONumber).Distinct().ToList();
                return result;
            }
        }

        public List<SAPPurchaseOrder> GetSAPPurchaseOrders()
        {
            using (var context = new SAPSourceModelContext())
            {
                var result = context.Database.SqlQuery<SAPPurchaseOrder>("uspSAPPurchaseOrder").ToList();
                return result;
            }
        }

        public List<int> GetSAPCostCenterNos()
        {
            using (var context = new SAPSourceModelContext())
            {
                var result = context.Database.SqlQuery<SAPCostCenter>("uspSAPCostCenter").ToList();
                var costCenterNo = (from r in result select r.CostCenterNo).Distinct().ToList();
                return costCenterNo;
            }
        }

        public IEnumerable<SAPWBS> GetSAPWBSFilter(string filtercondition)
        {
            using (var context = new SAPSourceModelContext())
            {
                var result = context.Database.SqlQuery<SAPWBS>("uspSAPWBS").Distinct().ToList().Where(p => p.WBSNo.ToLower().Contains(filtercondition.ToLower()));
                return result;
            }
        }


        public List<SAPWBS> GetSAPWBS()
        {
            using (var context = new SAPSourceModelContext())
            {
                var result = context.Database.SqlQuery<SAPWBS>("uspSAPWBS").Distinct().ToList();
                return result;
            }
        }

        public List<string> GetSAPInternalOrder()
        {
            using (var context = new SAPSourceModelContext())
            {
                var result = context.Database.SqlQuery<SAPInternalOrder>("uspSAPInternalOrder").ToList();
                var internalOrder = (from r in result select r.InternalOrderNo).Distinct().ToList();
                return internalOrder;
            }
        }
    }
}