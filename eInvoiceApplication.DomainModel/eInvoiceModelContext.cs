using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Core.Objects;
using SAPSourceMasterApplication.DomainModel;

namespace eInvoiceApplication.DomainModel
{
    public class eInvoiceModelContext : DbContext
    {
        private static readonly log4net.ILog LogManager = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public eInvoiceModelContext()
            : base("eInvoiceModelDbContextConnection")
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<InvoicePOApproverMemoryProfile>().MapToStoredProcedures();
            base.OnModelCreating(modelBuilder);
        }


        public DbSet<InvoicePODetail> InvoicePODetail { get; set; }
        public DbSet<InvoiceComment> InvoiceComment { get; set; }
        public DbSet<InvoiceAttachment> InvoiceAttachment { get; set; }
        public DbSet<InvoiceCATTFindings> InvoiceCATTFindings { get; set; }
        public DbSet<InvoiceCATTFindingsEmp> InvoiceCATTFindingsEmp { get; set; }
        public DbSet<InvoicePOApproverMemoryProfile> InvoicePOApproverMemoryProfile { get; set; }
        public DbSet<InvoicePODetailChanges> InvoicePODetailChanges { get; set; }
        public DbSet<InvoiceShortPayLetter> InvoiceShortPayLetter { get; set; }
        public DbSet<InvoiceRoutingRecord> InvoiceRoutingRecord { get; set; }
        public DbSet<TempErrorMessage> TempErrorMessages { get; set; }
        public DbSet<InvoicePOApprover> InvoicePOApprover { get; set; }

        public int GetInvoiceMasterID(string documentNo)
        {
            try
            {
                LogManager.Debug("GetInvoiceMasterID: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@DocumentNo", documentNo);
                    var result = context.Database.SqlQuery<int>("uspRoutingDetailsInvoiceMasterID @DocumentNo", clientIdParameter).FirstOrDefault();
                    LogManager.Debug("GetInvoiceMasterID: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("Home: ERROR " + ex.Message, ex);
                return 0;
            }
        }


        public int GetInvoiceMasterIDFilterOnStatus(string documentNo)
        {
            try
            {
                LogManager.Debug("GetInvoiceMasterIDFilterOnStatus: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@DocumentNo", documentNo);
                    var result = context.Database.SqlQuery<int>("uspRoutingDetailsInvoiceMasterIDFilterOnStatus @DocumentNo", clientIdParameter).FirstOrDefault();
                    LogManager.Debug("GetInvoiceMasterIDFilterOnStatus: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("Home: ERROR " + ex.Message, ex);
                return 0;
            }
        }

        public InvoiceMaster GetRoutingDetailsHeader(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetRoutingDetailsHeader: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterID", invoiceMasterID);
                    var result = context.Database.SqlQuery<InvoiceMaster>("uspInvoiceDetailsHeaderGetData @InvoiceMasterID", clientIdParameter).FirstOrDefault();
                    LogManager.Debug("GetRoutingDetailsHeader: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetRoutingDetailsHeader: ERROR " + ex.Message, ex);
                return null;
            }
        }

        //TTD - SM Replace 
        public InvoiceMaster GetRoutingDetailsHeaderTemp(int invoiceMasterID)
            {
            try
                {
                LogManager.Debug("GetRoutingDetailsHeader: START");
                using (var context = new eInvoiceModelContext())
                    {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterID", invoiceMasterID);
                    var result = context.Database.SqlQuery<InvoiceMaster>("uspInvoiceDetailsHeaderGetData @InvoiceMasterID", clientIdParameter).FirstOrDefault();
                    LogManager.Debug("GetRoutingDetailsHeader: END");
                    return result;
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("GetRoutingDetailsHeader: ERROR " + ex.Message, ex);
                return null;
                }
            }

        public bool POValidateEndValidityDate(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("POValidateEndValidityDate: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterID", invoiceMasterID);
                    var result = context.Database.SqlQuery<DateTime>("uspPOValidateEndValidityDate @InvoiceMasterID", clientIdParameter).FirstOrDefault();
                    var validityDate = (System.DateTime)result;
                    var currentDate = System.DateTime.Now;
                    if (currentDate > validityDate)
                    {
                        LogManager.Debug("POValidateEndValidityDate: END");
                        return true;
                    }
                    else
                    {
                        LogManager.Debug("POValidateEndValidityDate: END");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("POValidateEndValidityDate: ERROR " + ex.Message, ex);
                return false;
            }
        }

        public IEnumerable<InvoicePOApprover> GetDestinationApproversList(int invoiceMasterID, bool loadFromSAP)
        {
            try
            {
                LogManager.Debug("GetDestinationApproversList: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                     {
                          new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value =  invoiceMasterID},
                          new SqlParameter() {ParameterName = "@LoadFromSAP", SqlDbType = SqlDbType.Bit, Value =  loadFromSAP},
                     };
                    var result = context.Database.SqlQuery<InvoicePOApprover>("uspApproversMemoryList @InvoiceMasterID,@LoadFromSAP", sp).ToList<InvoicePOApprover>();
                    LogManager.Debug("GetDestinationApproversList: END");
                    return result;
                }

            }

            catch (Exception ex)
            {
                LogManager.Error("GetDestinationApproversList: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public List<ConfigRole> GetApproverRoleNames(int invoiceMasterID = 0)
        {
            try
            {
                LogManager.Debug("GetApproverRoleNames: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                     {
                          new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value =  invoiceMasterID}
                     };
                    List<ConfigRole> configRoles = new List<ConfigRole>();
                    configRoles = context.Database.SqlQuery<ConfigRole>("uspApproverRoleName @InvoiceMasterID", sp).ToList();
                    LogManager.Debug("GetApproverRoleNames: END");
                    return configRoles;
                }

            }
            catch (Exception ex)
            {
                LogManager.Error("GetApproverRoleNames: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public List<InvoicePOApprover> SaveInvoiceApproversList(List<InvoicePOApprover> approvers)
        {
            try
            {
                LogManager.Debug("SaveInvoiceApproversList: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter parameter = PrepareMemoryProfile(approvers);
                    var result = context.Database.SqlQuery<InvoicePOApprover>("uspRoutingDetailsSaveModifiedApprovers @MemoryProfileTabletype", parameter).ToList<InvoicePOApprover>();
                    LogManager.Debug("SaveInvoiceApproversList: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveInvoiceApproversList: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public void DeleteInvoiceApprovers(List<InvoicePOApprover> approvers)
        {
            try
            {
                LogManager.Debug("DeleteInvoiceApprovers: START");
                using (var invoiceContext = new eInvoiceModelContext())
                {
                    SqlParameter parameter = PrepareMemoryProfile(approvers);
                    var result = invoiceContext.Database.SqlQuery<InvoicePOApprover>("uspRoutingDetailsDeleteApprovers @MemoryProfileTabletype", parameter).ToList<InvoicePOApprover>();
                }
                LogManager.Debug("DeleteInvoiceApprovers: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("DeleteInvoiceApprovers: ERROR " + ex.Message, ex);
            }
        }

        private SqlParameter PrepareMemoryProfile(List<InvoicePOApprover> approvers)
        {
            try
            {
                LogManager.Debug("PrepareMemoryProfile: START");
                string typeName = "dbo.InvoicePOApproverType";
                DataTable table = CreateDataTable(approvers);
                SqlParameter parameter = new SqlParameter("@MemoryProfileTabletype", table);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = typeName;
                LogManager.Debug("PrepareMemoryProfile: END");
                return parameter;
            }
            catch (Exception ex)
            {
                LogManager.Error("PrepareMemoryProfile: ERROR " + ex.Message, ex);
                return null;
            }
        }

        private static DataTable CreateDataTable(List<InvoicePOApprover> approvers)
        {
            try
            {
                LogManager.Debug("CreateDataTable: START");
                string configColumn = "ConfigRole";
                string formattedPOColumn = "FormattedPONumber";
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(InvoicePOApprover));
                DataTable table = new DataTable();
                foreach (PropertyDescriptor prop in properties)
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                foreach (InvoicePOApprover profile in approvers)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                        row[prop.Name] = prop.GetValue(profile) ?? DBNull.Value;
                    table.Rows.Add(row);
                }
                if (table.Columns.Contains(configColumn))
                    table.Columns.Remove(configColumn);
                if (table.Columns.Contains(formattedPOColumn))
                    table.Columns.Remove(formattedPOColumn);

                LogManager.Debug("CreateDataTable: END");
                return table;
            }

            catch (Exception ex)
            {
                LogManager.Error("CreateDataTable: ERROR " + ex.Message, ex);
                return null;
            }
        }


        /// <summary>
        /// Authenticates if the user is a Valid eInvoice user
        /// </summary>
        /// <param name="LoggedinUser">Logged in userName</param>
        /// <returns>true if valid user, else returns False</returns>
        public bool AuthenticateUser(string LoggedinUser)
        {
            try
            {
                LogManager.Debug("AuthenticateUser: START");

                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                      {
                          new SqlParameter() {ParameterName = "@LoggedinUser", SqlDbType = SqlDbType.VarChar, Value =  LoggedinUser},
                      };
                    int result = context.Database.SqlQuery<int>("uspAuthenticateUser @LoggedinUser", sp).FirstOrDefault();
                    if (result == 1)
                    {
                        LogManager.Debug("AuthenticateUser: END");
                        return true;
                    }
                    else
                    {
                        LogManager.Debug("AuthenticateUser: END");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("AuthenticateUser: ERROR " + ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// Gets UserID of the Employee if he is configured for RoleName
        /// </summary>
        /// <param name="roleabbreviation">Name of the Role</param>
        /// <param name="InvoiceMasterID">Pass NULL, if Role Name is a Admin role, else pass InvoiceMasterID for which to determine user Authentication </param>
        /// <returns>UserName if the user is valid for Role Name Passed </returns>
        public List<string> GetConfiguredUser(string roleabbreviation, int InvoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetConfiguredUser: START");
                using (var context = new eInvoiceModelContext())
                {
                    List<String> assignedUsers = new List<string>();
                    SqlParameter[] sp = new SqlParameter[]
               {
                   new SqlParameter() {ParameterName = "@RoleAbbreviation", SqlDbType = SqlDbType.VarChar, Value =  roleabbreviation},
                   new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int,  Value = InvoiceMasterID},
               };

                    var result = context.Database.SqlQuery<AdminRole>("uspAuthenticateUserRole @RoleAbbreviation, @InvoiceMasterID", sp).ToList<AdminRole>();
                    if (result.Count > 0)
                    {
                        foreach (AdminRole role in result)
                            assignedUsers.Add(role.Approver);
                    }
                    LogManager.Debug("GetConfiguredUser: END");
                    return assignedUsers;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetConfiguredUser: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public List<String> GetDestinationApproversforInvoice(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetDestinationApproversforInvoice: START");
                using (var context = new eInvoiceModelContext())
                {
                    List<String> assignedUsers = new List<string>();
                    var clientIdParameter = new SqlParameter("@InvoiceMasterID", invoiceMasterID);
                    assignedUsers = context.Database.SqlQuery<string>("uspGetDestinationApproversForInvoice @InvoiceMasterID", clientIdParameter).ToList<string>();
                    LogManager.Debug("GetDestinationApproversforInvoice: END");
                    return assignedUsers;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetDestinationApproversforInvoice: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public List<string> GetLoggedInUserRole(string userId)
        {
            try
            {
                LogManager.Debug("GetLoggedInUserRole: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@UserId", userId);
                    var result = context.Database.SqlQuery<ConfigRole>("uspGetLoggedInUserRole @UserId", clientIdParameter).ToList<ConfigRole>();
                    var roles = (from r in result select r.RoleAbbreviation).ToList();
                    LogManager.Debug("GetLoggedInUserRole: END");
                    return roles;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetLoggedInUserRole: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public int GetRoleIdForAP(string userId)
        {
            try
            {
                LogManager.Debug("GetRoleIdForAP: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@UserId", userId);
                    var result = context.Database.SqlQuery<ConfigRole>("uspGetLoggedInUserRole @UserId", clientIdParameter).ToList<ConfigRole>();
                    var role = (from r in result where r.RoleAbbreviation == "AP" select r.RoleID).FirstOrDefault();
                    LogManager.Debug("GetRoleIdForAP: END");
                    return role;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetRoleIdForAP: ERROR " + ex.Message, ex);
                return 0;
            }
        }

        public string GetDocumentNo(int procId)
        {
            try
            {
                LogManager.Debug("GetDocumentNo: START");

                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@ProcId", procId);
                    var result = context.Database.SqlQuery<string>("uspRoutingDetailsDocumentNo @ProcId", clientIdParameter).FirstOrDefault();
                    LogManager.Debug("GetDocumentNo: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetDocumentNo: ERROR " + ex.Message, ex);
                return "0";
            }
        }

        public string GetDocumentNoFromInvoiceMasterID(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetDocumentNoFromInvoiceMasterID: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterID", invoiceMasterID);
                    var result = context.Database.SqlQuery<string>("uspRoutingDetailsDocumentNoFromInvoiceMasterID @InvoiceMasterID", clientIdParameter).FirstOrDefault();
                    LogManager.Debug("GetDocumentNoFromInvoiceMasterID: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetDocumentNoFromInvoiceMasterID: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public string GetDocumentTypeFromInvoiceMasterID(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetDocumentTypeFromInvoiceMasterID: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterID", invoiceMasterID);
                    var result = context.Database.SqlQuery<string>("uspRoutingDetailsDocumentTypeFromInvoiceMasterID @InvoiceMasterID", clientIdParameter).FirstOrDefault();
                    LogManager.Debug("GetDocumentTypeFromInvoiceMasterID: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetDocumentTypeFromInvoiceMasterID: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public bool GetNonContractingStatusFromInvoiceMasterID(int invoiceMasterID)
        {
            try
            {
            LogManager.Debug("GetNonContractingStatusFromInvoiceMasterID: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterID", invoiceMasterID);
                    var result = context.Database.SqlQuery<bool>("uspRoutingDetailsNonContractingStatusFromInvoiceMasterID @InvoiceMasterID", clientIdParameter).FirstOrDefault();
                    LogManager.Debug("GetNonContractingStatusFromInvoiceMasterID: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
            LogManager.Error("GetNonContractingStatusFromInvoiceMasterID: ERROR " + ex.Message, ex);
                return false;
            }
        }

        public string GetInvoiceTypeFromInvoiceMasterID(int invoiceMasterID)
            {
            try
                {
                LogManager.Debug("GetInvoiceTypeFromInvoiceMasterID: START");
                using (var context = new eInvoiceModelContext())
                    {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterID", invoiceMasterID);
                    var result = context.Database.SqlQuery<string>("uspRoutingDetailsInvoiceType @InvoiceMasterID", clientIdParameter).FirstOrDefault();
                    LogManager.Debug("GetInvoiceTypeFromInvoiceMasterID: END");
                    return result;
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("GetInvoiceTypeFromInvoiceMasterID: ERROR " + ex.Message, ex);
                return "";
                }
            }

        public string GetStatus(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetStatus: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterID", invoiceMasterID);
                    var result = context.Database.SqlQuery<string>("uspInvoiceStatus @InvoiceMasterID", clientIdParameter).FirstOrDefault();
                    LogManager.Debug("GetStatus: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetStatus: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public string GetApproverRoleId(string status, int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetApproverRoleId: START");

                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                       {
                           new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= invoiceMasterID},
                           new SqlParameter() {ParameterName = "@Status", SqlDbType = SqlDbType.VarChar, Value =  status},
                       };
                    var result = context.Database.SqlQuery<string>("uspApproverRoleId @InvoiceMasterID,@Status", sp).FirstOrDefault();
                    LogManager.Debug("GetApproverRoleId: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetApproverRoleId: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public int GetInvoiceMasterIDFromProcId(int procId)
        {
            try
            {
                LogManager.Debug("GetInvoiceMasterIDFromProcId: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                       {
                           new SqlParameter() {ParameterName = "@ProcID", SqlDbType = SqlDbType.Int, Value= procId},
                       };
                    var result = context.Database.SqlQuery<int>("uspInvoiceMasterIDFromProcID @ProcID", sp).FirstOrDefault();
                    LogManager.Debug("GetInvoiceMasterIDFromProcId: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetInvoiceMasterIDFromProcId: ERROR " + ex.Message, ex);
                return 0;
            }
        }


        public List<InvoiceAttachment> GetInvoiceAttachments(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetInvoiceAttachments: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterID", invoiceMasterID);
                    var result = context.Database.SqlQuery<InvoiceAttachment>("uspRoutingDetailsInvoiceAttachments @InvoiceMasterID", clientIdParameter).ToList<InvoiceAttachment>();
                    LogManager.Debug("GetInvoiceAttachments: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetInvoiceAttachments: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public int SaveInvoiceAttachment(InvoiceAttachment attachment)
        {
            try
            {
                LogManager.Debug("SaveInvoiceAttachment: START");

                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                       {
                         new SqlParameter() {ParameterName = "@FileAttachment", SqlDbType = SqlDbType.VarBinary, Value=  attachment.FileAttachment},
                         new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value =  attachment.InvoiceMasterID},
                         new SqlParameter() {ParameterName = "@FileName", SqlDbType = SqlDbType.VarChar, Value = attachment.FileName},
                         new SqlParameter() {ParameterName = "@UploadedUserID", SqlDbType = SqlDbType.VarChar, Value = attachment.UploadedUserID}
                       };
                    var result = context.Database.SqlQuery<InvoiceAttachment>("uspRoutingDetailsSaveInvoiceAttachment @FileAttachment,@InvoiceMasterID,@FileName,@UploadedUserID", sp).FirstOrDefault();
                    LogManager.Debug("SaveInvoiceAttachment: END");
                    return result.InvoiceAttachmentID;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveInvoiceAttachment: ERROR " + ex.Message, ex);
                return 0;
            }
        }

        public void DeleteInvoiceAttachment(int attachmentID)
        {
            try
            {
                LogManager.Debug("DeleteInvoiceAttachment: START");

                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@AttachmentID", attachmentID);
                    var result = context.Database.SqlQuery<InvoiceAttachment>("uspRoutingDetailsDeleteInvoiceAttachment @AttachmentID", clientIdParameter).ToList<InvoiceAttachment>();
                }
                LogManager.Debug("DeleteInvoiceAttachment: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("DeleteInvoiceAttachment: ERROR " + ex.Message, ex);
            }
        }

        public InvoiceAttachment DisplayAttachment(int attachmentID)
        {
            try
            {
                LogManager.Debug("DisplayAttachment: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter parameter = new SqlParameter("@AttachmentID", attachmentID);
                    var result = context.Database.SqlQuery<InvoiceAttachment>("uspRoutingDetailsDisplayAttachment @AttachmentID", parameter).FirstOrDefault();
                    LogManager.Debug("DisplayAttachment: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("DisplayAttachment: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public int SaveInvoiceComment(InvoiceComment invoiceComment)
        {
            try
            {
                LogManager.Debug("SaveInvoiceComment: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                       {
                         new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value=  invoiceComment.InvoiceMasterID},
                         new SqlParameter() {ParameterName = "@Comment", SqlDbType = SqlDbType.VarChar, Value =  invoiceComment.Comment},
                         new SqlParameter() {ParameterName = "@CommentBy", SqlDbType = SqlDbType.VarChar, Value = invoiceComment.CommentBy},
                         new SqlParameter() {ParameterName = "@CommentDate", SqlDbType = SqlDbType.DateTime, Value = invoiceComment.CommentDate}
                       };
                    var result = context.Database.SqlQuery<InvoiceComment>("uspRoutingDetailsSaveInvoiceComments @InvoiceMasterID,@Comment,@CommentBy,@CommentDate", sp).FirstOrDefault();
                    LogManager.Debug("SaveInvoiceComment: END");
                    return result.InvoiceCommentID;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveInvoiceComment: ERROR " + ex.Message, ex);
                return 0;
            }
        }

        public List<InvoiceComment> GetInvoiceComments(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetInvoiceComments: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterID", invoiceMasterID);
                    var result = context.Database.SqlQuery<InvoiceComment>("uspRoutingDetailsInvoiceComments @InvoiceMasterID", clientIdParameter).ToList<InvoiceComment>();
                    LogManager.Debug("GetInvoiceComments: END");
                    return result;
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("GetInvoiceComments: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public List<TempActionsDropdown> GetApproversListForLoggedRole(int invoiceMasterID, int invoiceType = 0)
        {

            try
            {
                LogManager.Debug("GetApproversListForLoggedRole: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                   {
                    new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= invoiceMasterID},
                    new SqlParameter() {ParameterName = "@InvoiceType", SqlDbType = SqlDbType.Int, Value =  invoiceType},
                   };
                    var result = context.Database.SqlQuery<TempActionsDropdown>("uspRoutingDetailsApproversList @InvoiceMasterID,@InvoiceType", sp).ToList<TempActionsDropdown>();
                    LogManager.Debug("GetApproversListForLoggedRole: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetApproversListForLoggedRole: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public void SaveInvoiceProject(int invoiceMasterID, string project)
        {
            try
            {
                LogManager.Debug("SaveInvoiceProject: START");

                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                   {
                     new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= invoiceMasterID},
                     new SqlParameter() {ParameterName = "@Project", SqlDbType = SqlDbType.VarChar, Value =  project},
                   };
                    var result = context.Database.SqlQuery<InvoiceMaster>("uspRoutingDetailsSaveInvoiceProject @InvoiceMasterID,@Project", sp).FirstOrDefault();
                }
                LogManager.Debug("SaveInvoiceProject: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveInvoiceProject: ERROR " + ex.Message, ex);
            }
        }


        public void SaveInvoicePeriod(int invoiceMasterID, string period)
        {
            try
            {
                LogManager.Debug("SaveInvoicePeriod: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                    {
                        new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= invoiceMasterID},
                        new SqlParameter() {ParameterName = "@Period", SqlDbType = SqlDbType.VarChar, Value =  period},
                    };
                    var result = context.Database.SqlQuery<InvoiceMaster>("uspRoutingDetailsSaveInvoicePeriod @InvoiceMasterID,@Period", sp).FirstOrDefault();
                }
                LogManager.Debug("SaveInvoicePeriod: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveInvoicePeriod: ERROR " + ex.Message, ex);
            }
        }


        public void SaveInvoiceContractNo(int invoiceMasterID, string contractNo)
        {
            try
            {
                LogManager.Debug("SaveInvoiceContractNo: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                    {
                        new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value = invoiceMasterID},
                        new SqlParameter() {ParameterName = "@ContractNo", SqlDbType = SqlDbType.VarChar, Value =  contractNo},
                    };
                    var result = context.Database.SqlQuery<InvoiceMaster>("uspRoutingDetailsSaveInvoiceContractNo @InvoiceMasterID,@ContractNo", sp).FirstOrDefault();
                }
                LogManager.Debug("SaveInvoiceContractNo: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveInvoiceContractNo: ERROR " + ex.Message, ex);
            }
        }


        public void SaveInvoicePaymentDueBy(int invoiceMasterID, DateTime paymentDueBy)
        {
            try
            {
                LogManager.Debug("SaveInvoicePaymentDueBy: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                    {
                         new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= invoiceMasterID},
                         new SqlParameter() {ParameterName = "@PaymentDueBy", SqlDbType = SqlDbType.DateTime, Value =  paymentDueBy},
                    };
                    var result = context.Database.SqlQuery<InvoiceMaster>("uspRoutingDetailsSaveInvoicePaymentDueBy @InvoiceMasterID,@PaymentDueBy", sp).FirstOrDefault();
                }
                LogManager.Debug("SaveInvoicePaymentDueBy: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveInvoicePaymentDueBy: ERROR " + ex.Message, ex);
            }
        }


        public void SaveInvoiceCATTThreshold(int invoiceMasterID, int CATTThreshold)
        {
            try
            {
                LogManager.Debug("SaveInvoiceCATTThreshold: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                   {
                     new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= invoiceMasterID},
                     new SqlParameter() {ParameterName = "@CATTThreshold", SqlDbType = SqlDbType.Int, Value =  CATTThreshold},
                   };
                    var result = context.Database.SqlQuery<InvoiceMaster>("uspRoutingDetailsSaveInvoiceCATTThreshold @InvoiceMasterID,@CATTThreshold", sp).FirstOrDefault();
                }
                LogManager.Debug("SaveInvoiceCATTThreshold: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveInvoiceCATTThreshold: ERROR " + ex.Message, ex);
            }
        }


        public void SaveInvoiceCATTApproval(int invoiceMasterID, bool CATTApproval)
        {
            try
            {
                LogManager.Debug("SaveInvoiceCATTApproval: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                    {
                        new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= invoiceMasterID},
                        new SqlParameter() {ParameterName = "@CATTApprovalRequired", SqlDbType = SqlDbType.Bit, Value =  CATTApproval},
                    };
                    var result = context.Database.SqlQuery<InvoiceMaster>("uspRoutingDetailsSaveInvoiceCATTApproval @InvoiceMasterID,@CATTApprovalRequired", sp).FirstOrDefault();
                }
                LogManager.Debug("SaveInvoiceCATTApproval: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveInvoiceCATTApproval: ERROR " + ex.Message, ex);

            }
        }


        public void SaveInvoiceNonContractingStatus(int invoiceMasterID, bool NonContractingStatus)
        {
            try
            {
            LogManager.Debug("SaveInvoiceNonContractingStatus: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                    {
                        new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= invoiceMasterID},
                        new SqlParameter() {ParameterName = "@NonContractingStatus", SqlDbType = SqlDbType.Bit, Value =  NonContractingStatus},
                    };
                    var result = context.Database.SqlQuery<InvoiceMaster>("uspRoutingDetailsSaveInvoiceNonContractingStatus @InvoiceMasterID,@NonContractingStatus", sp).FirstOrDefault();
                }
                LogManager.Debug("SaveInvoiceNonContractingStatus: END");
            }
            catch (Exception ex)
            {
            LogManager.Error("SaveInvoiceNonContractingStatus: ERROR " + ex.Message, ex);

            }
        }

        public void SaveInvoiceType(int invoiceMasterID, string invoiceType)
            {
            try
                {
                LogManager.Debug("SaveInvoiceType: START");
                using (var context = new eInvoiceModelContext())
                    {
                    SqlParameter[] sp = new SqlParameter[]
                    {
                        new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= invoiceMasterID},
                        new SqlParameter() {ParameterName = "@InvoiceType", SqlDbType = SqlDbType.VarChar, Value =  invoiceType},
                    };
                    var result = context.Database.SqlQuery<InvoiceMaster>("uspRoutingDetailsSaveInvoiceType @InvoiceMasterID,@InvoiceType", sp).FirstOrDefault();
                    }
                LogManager.Debug("SaveInvoiceType: END");
                }
            catch (Exception ex)
                {
                LogManager.Error("SaveInvoiceType: ERROR " + ex.Message, ex);

                }
            }

        public InvoiceMaster SaveInvoiceDetails(int invoiceMasterID, string userId, DateTime submittedDay)
        {
            try
            {
                LogManager.Debug("SaveInvoiceDetails: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                        {
                            new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= invoiceMasterID},
                            //new SqlParameter() {ParameterName = "@Period", SqlDbType = SqlDbType.VarChar, Value =  period},
                            //new SqlParameter() {ParameterName = "@Project", SqlDbType = SqlDbType.VarChar, Value = project},
                            //new SqlParameter() {ParameterName = "@PaymentDueBy", SqlDbType = SqlDbType.DateTime, Value = paymentDueBy},
                            //new SqlParameter() {ParameterName = "@CATTThreshold", SqlDbType = SqlDbType.Int, Value = CATTThreshold},
                            //new SqlParameter() {ParameterName = "@CATTApprovalRequired", SqlDbType = SqlDbType.Bit, Value = CATTApproval},
                            new SqlParameter() {ParameterName = "@SubmittedUserId", SqlDbType = SqlDbType.VarChar, Value = userId},
                            new SqlParameter() {ParameterName = "@SubmittedDate", SqlDbType = SqlDbType.DateTime, Value = submittedDay},
                        // new SqlParameter() {ParameterName = "@SESNumber", SqlDbType = SqlDbType.VarChar, Value = SESNumber},
                        };
                    var result = context.Database.SqlQuery<InvoiceMaster>("uspRoutingDetailsSaveInvoiceDetails @InvoiceMasterID,@SubmittedUserId,@SubmittedDate", sp).FirstOrDefault();
                    LogManager.Debug("SaveInvoiceDetails: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveInvoiceDetails: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public string GetAPSubmittedByUserId(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetAPSubmittedByUserId: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                       {
                           new SqlParameter() { ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value = invoiceMasterID},
                       };
                    var result = context.Database.SqlQuery<string>("uspRoutingDetailsGetAPSubmittedByUserId @InvoiceMasterID", sp).FirstOrDefault();
                    LogManager.Debug("GetAPSubmittedByUserId: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetAPSubmittedByUserId: ERROR " + ex.Message, ex);
                return null;
            }

        }


        public void UpdateSESNumber(int invoiceMasterID, string SESNumber)
        {
            try
            {
                LogManager.Debug("UpdateSESNumber: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                       {
                         new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= invoiceMasterID},
                         new SqlParameter() {ParameterName = "@SESNumber", SqlDbType = SqlDbType.VarChar, Value =  SESNumber},
                       };
                    var result = context.Database.SqlQuery<InvoiceMaster>("uspRoutingDetailsUpdateSESNumber @InvoiceMasterID,@SESNumber", sp).FirstOrDefault();

                }
                LogManager.Debug("UpdateSESNumber: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("UpdateSESNumber: ERROR " + ex.Message, ex);
            }
        }


        public List<TempErrorMessage> ValidateRoutingDetails(int invoiceMasterID, int routeTo, bool isReadOnly, string action)
        {
            try
            {
                LogManager.Debug("ValidateRoutingDetails: START");
                using (var context = new eInvoiceModelContext())
                {
                  SqlParameter[] sp = new SqlParameter[]
                  {
                      new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= invoiceMasterID},
                      new SqlParameter() {ParameterName = "@RouteTo", SqlDbType = SqlDbType.Int, Value =  routeTo},
                      new SqlParameter() {ParameterName = "@IsReadOnly", SqlDbType = SqlDbType.Bit, Value = isReadOnly},
                      new SqlParameter() {ParameterName = "@Action", SqlDbType = SqlDbType.VarChar, Value = action},
                  };
                  var result = context.Database.SqlQuery<TempErrorMessage>("uspRoutingDetailsValidate @InvoiceMasterID,@RouteTo,@IsReadOnly,@Action", sp).ToList<TempErrorMessage>();
                  LogManager.Debug("ValidateRoutingDetails: END");
                  return result;
               }
            }
            catch (Exception ex)
            {
                LogManager.Error("ValidateRoutingDetails: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public void InsertRoutingRecord(int invoiceMasterID, int roleId, string approverUserId)
        {
            try
            {
                LogManager.Debug("InsertRoutingRecord: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                    {
                        new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= invoiceMasterID},
                        new SqlParameter() {ParameterName = "@RoleId", SqlDbType = SqlDbType.Int, Value =  roleId},
                        new SqlParameter() {ParameterName = "@ApproverUserId", SqlDbType = SqlDbType.VarChar, Value = approverUserId},
                    };
                    var result = context.Database.SqlQuery<InvoiceRoutingRecord>("uspInsertRoutingRecordForAP @InvoiceMasterID,@RoleId,@ApproverUserId", sp).ToList<InvoiceRoutingRecord>();
                }
                LogManager.Debug("InsertRoutingRecord: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("InsertRoutingRecord: ERROR " + ex.Message, ex);

            }

        }


        public string GetActionNameForCurrentRole(int invoiceMasterID, string userAction)
        {
            try
            {
                LogManager.Debug("GetActionNameForCurrentRole: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                   {
                       new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= invoiceMasterID},
                       new SqlParameter() {ParameterName = "@UserAction", SqlDbType = SqlDbType.VarChar, Value =  userAction},
                   };
                    var result = context.Database.SqlQuery<string>("uspActionNameForCurrentRole @InvoiceMasterID,@UserAction", sp).FirstOrDefault();
                    LogManager.Debug("GetActionNameForCurrentRole: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetActionNameForCurrentRole: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public void InitializeInvoiceTabs(int invoiceMasterID, string userId)
        {
            try
            {
                LogManager.Debug("InitializeInvoiceTabs: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                     {
                          new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value = invoiceMasterID },
                          new SqlParameter() {ParameterName = "@UserId", SqlDbType = SqlDbType.VarChar, Value = userId }
                     };
                   var result = context.Database.SqlQuery<InvoicePODetailChanges>("uspInitializeInvoiceTabs @InvoiceMasterID,@UserId", sp).ToList<InvoicePODetailChanges>();
                }
                LogManager.Debug("InitializeInvoiceTabs: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("InitializeInvoiceTabs: ERROR " + ex.Message, ex);
            }
        }


        public IEnumerable<InvoicePODetail> GetInvoicePODetails(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetInvoicePODetails: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterId", invoiceMasterID);
                    var result = context.Database.SqlQuery<InvoicePODetail>("uspInvoicePODetails @InvoiceMasterId", clientIdParameter).ToList();
                    LogManager.Debug("GetInvoicePODetails: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetInvoicePODetails: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public IEnumerable<InvoicePODetailChanges> GetInvoicePODetailChanges(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetInvoicePODetailChanges: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterId", invoiceMasterID);
                    var result = context.Database.SqlQuery<InvoicePODetailChanges>("uspInvoicePODetailsChanges @InvoiceMasterId", clientIdParameter).ToList();
                    LogManager.Debug("GetInvoicePODetailChanges: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetInvoicePODetailChanges: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public bool GetInvoicePODetailsModifyFlag(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetInvoicePODetailsModifyFlag: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterId", invoiceMasterID);
                    var result = context.Database.SqlQuery<bool>("uspInvoicePODetailsModifyFlag @InvoiceMasterId", clientIdParameter).FirstOrDefault();
                    LogManager.Debug("GetInvoicePODetailsModifyFlag: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetInvoicePODetailChanges: ERROR " + ex.Message, ex);
                return false;
            }
        }

        public InvoicePODetailChanges GetInvoicePODetailChangeProperties(int invoiceDetailChangesID)
        {
            try
            {
                LogManager.Debug("GetInvoicePODetailChangeProperties: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                    {
                         new SqlParameter() {ParameterName = "@InvoiceDetailChangesID", SqlDbType = SqlDbType.Int, Value = invoiceDetailChangesID }
                    };
                    var result = context.Database.SqlQuery<InvoicePODetailChanges>("uspInvoicePODetailsChangeGetProperties @InvoiceDetailChangesID", sp).FirstOrDefault();
                    LogManager.Debug("GetInvoicePODetailChangeProperties: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetInvoicePODetailChangeProperties: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public IEnumerable<InvoicePODetailChangesLog> GetInvoicePODetailChangesLog(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetInvoicePODetailChangesLog: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterId", invoiceMasterID);
                    var result = context.Database.SqlQuery<InvoicePODetailChangesLog>("uspInvoicePODetailsChangesLog @InvoiceMasterId", clientIdParameter).ToList();
                    LogManager.Debug("GetInvoicePODetailChangesLog: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetInvoicePODetailChangesLog: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public IEnumerable<InvoicePODetailChanges> SaveAccountingCostCodes(List<InvoicePODetailChanges> modifiedCostCodes)
        {
            try
            {
                LogManager.Debug("SaveAccountingCostCodes: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter parameter = PrepareAccountingCostCodes(modifiedCostCodes);
                    var result = context.Database.SqlQuery<InvoicePODetailChanges>("uspPurchaseOrderDetailsSaveModifiedCostCodes @InvoicePODetailChangestype", parameter).ToList<InvoicePODetailChanges>();
                    LogManager.Debug("SaveAccountingCostCodes: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveAccountingCostCodes: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public void DeleteAccountingCostCodes(List<InvoicePODetailChanges> deletedCostCodes)
        {
            try
            {
                LogManager.Debug("DeleteAccountingCostCodes: START");
                using (var invoiceContext = new eInvoiceModelContext())
                {
                    SqlParameter parameter = PrepareAccountingCostCodes(deletedCostCodes);
                    var result = invoiceContext.Database.SqlQuery<InvoicePODetailChanges>("uspPurchaseOrderDetailsDeleteModifiedCostCodes @InvoicePODetailChangestype", parameter).ToList<InvoicePODetailChanges>();
                }
                LogManager.Debug("DeleteAccountingCostCodes: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("DeleteAccountingCostCodes: ERROR " + ex.Message, ex);

            }

        }

        private SqlParameter PrepareAccountingCostCodes(List<InvoicePODetailChanges> modifiedCostCodes)
        {
            try
            {
                LogManager.Debug("PrepareAccountingCostCodes: START");
                string typeName = "dbo.InvoicePODetailChangesType";
                DataTable table = CreateDataTableForCostCodes(modifiedCostCodes);
                SqlParameter parameter = new SqlParameter("@InvoicePODetailChangestype", table);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = typeName;
                LogManager.Debug("PrepareAccountingCostCodes: END");
                return parameter;
            }
            catch (Exception ex)
            {
                LogManager.Error("PrepareAccountingCostCodes: ERROR " + ex.Message, ex);
                return null;
            }
        }


        private static DataTable CreateDataTableForCostCodes(List<InvoicePODetailChanges> modifiedCostCodes)
        {
            try
            {
                LogManager.Debug("CreateDataTableForCostCodes: START");

                string invoiceMasterColumn = "InvoiceMaster";
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(InvoicePODetailChanges));
                DataTable table = new DataTable();
                foreach (PropertyDescriptor prop in properties)
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                foreach (InvoicePODetailChanges changes in modifiedCostCodes)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                        row[prop.Name] = prop.GetValue(changes) ?? DBNull.Value;
                    table.Rows.Add(row);
                }
                if (table.Columns.Contains(invoiceMasterColumn))
                    table.Columns.Remove(invoiceMasterColumn);

                LogManager.Debug("CreateDataTableForCostCodes: END");
                return table;
            }

            catch (Exception ex)
            {
                LogManager.Error("CreateDataTableForCostCodes: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public List<eInvoiceApprovers> GeteInvoiceApprovers(int invoiceMasterId)
        {
            try
            {
                LogManager.Debug("GeteInvoiceApprovers: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterID", invoiceMasterId);
                    var result = context.Database.SqlQuery<eInvoiceApprovers>("uspGeteInvoiceApprovers @InvoiceMasterID", clientIdParameter).ToList();
                    LogManager.Debug("GeteInvoiceApprovers: END");
                    return result;
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("GeteInvoiceApprovers: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public InvoiceShortPayLetter GetShortPayDetails(int invoiceMasterId)
        {
            try
            {
                LogManager.Debug("GetShortPayDetails: START");
                using (var context = new eInvoiceModelContext())
                {
                    var clientIdParameter = new SqlParameter("@InvoiceMasterId", invoiceMasterId);
                    var result = context.Database.SqlQuery<InvoiceShortPayLetter>("uspShortPayLetterDetails @InvoiceMasterId", clientIdParameter).FirstOrDefault();
                    LogManager.Debug("GetShortPayDetails: END");
                    return result;
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("GetShortPayDetails: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public void SaveShortPayDetails(int invoiceMasterId, InvoiceShortPayLetter shortPay)
        {
            try
            {
                LogManager.Debug("SaveShortPayDetails: START");

                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                       {
                             new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value=  invoiceMasterId},
                             new SqlParameter() {ParameterName = "@AddressedTo",IsNullable=true, SqlDbType = SqlDbType.VarChar, Value =  (object)shortPay.AddressedTo ?? DBNull.Value},
                             new SqlParameter() {ParameterName = "@SentFrom",IsNullable=true, SqlDbType = SqlDbType.VarChar, Value =(object) shortPay.SentFrom ?? DBNull.Value},
                             new SqlParameter() {ParameterName = "@Subject",IsNullable=true, SqlDbType = SqlDbType.VarChar, Value = (object)shortPay.Subject ?? DBNull.Value},
                             new SqlParameter() {ParameterName = "@Date", IsNullable=true, SqlDbType = SqlDbType.DateTime, Value = (object) shortPay.Date ?? DBNull.Value },
                             new SqlParameter() {ParameterName = "@CAContactNo",IsNullable=true, SqlDbType = SqlDbType.VarChar, Value = (object)shortPay.CAContactNo ?? DBNull.Value},
                             new SqlParameter() {ParameterName = "@ApprovedPaymentAmount",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value =(object) shortPay.ApprovedPaymentAmount ?? DBNull.Value},
                             new SqlParameter() {ParameterName = "@CANotes",IsNullable=true, SqlDbType = SqlDbType.VarChar, Value =(object) shortPay.CANotes ?? DBNull.Value}
                          };
                    var result = context.Database.SqlQuery<InvoiceShortPayLetter>("uspShortPayLetterSaveDetails @InvoiceMasterID,@AddressedTo,@SentFrom,@Subject,@Date,@CAContactNo,@ApprovedPaymentAmount,@CANotes", sp).FirstOrDefault();
                }

                LogManager.Debug("SaveShortPayDetails: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveShortPayDetails: ERROR " + ex.Message, ex);

            }
        }


        public List<StatusReportDropdown> GetStatusforReport()
        {
            try
            {
                LogManager.Debug("GetStatusforReport: START");
                using (var context = new eInvoiceModelContext())
                {
                    List<StatusReportDropdown> statusReport = new List<StatusReportDropdown>();
                    statusReport = context.Database.SqlQuery<StatusReportDropdown>("uspGetStatusforReport").ToList();
                    LogManager.Debug("GetStatusforReport: END");

                    return statusReport;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetStatusforReport: ERROR " + ex.Message, ex);
                return null;
            }


        }


        public List<InvoiceErrorLog> GetErrorLogReport(DashboardReportSearch reportSearch)
        {
            try
            {
                LogManager.Debug("GetErrorLogReport: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                       {
                         new SqlParameter() {ParameterName = "@DateFrom", SqlDbType = SqlDbType.DateTime, Value=  reportSearch.DateFrom},
                         new SqlParameter() {ParameterName = "@DateTo", SqlDbType = SqlDbType.DateTime, Value =  reportSearch.DateTo},
                       };
                    List<InvoiceErrorLog> result = context.Database.SqlQuery<InvoiceErrorLog>("uspGetErrorLogReport @DateFrom,@DateTo", sp).ToList<InvoiceErrorLog>();

                    LogManager.Debug("GetErrorLogReport: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetErrorLogReport: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public List<DashboardReport> GetDashboardReport(DashboardReportSearch reportSearch)
        {
            try
            {
                LogManager.Debug("GetDashboardReport: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                       {
                         new SqlParameter() {ParameterName = "@DateFrom", SqlDbType = SqlDbType.DateTime, Value=  reportSearch.DateFrom},
                         new SqlParameter() {ParameterName = "@DateTo", SqlDbType = SqlDbType.DateTime, Value =  reportSearch.DateTo},
                         new SqlParameter() {ParameterName = "@Status", SqlDbType = SqlDbType.VarChar, Value = reportSearch.Status},
                         new SqlParameter() {ParameterName = "@Access", SqlDbType = SqlDbType.VarChar, Value = reportSearch.Access},
                         new SqlParameter() {ParameterName = "@SubmittedBy", SqlDbType = SqlDbType.VarChar, Value = reportSearch.SubmittedBy},
                         new SqlParameter() {ParameterName = "@LoggedinUserType", SqlDbType = SqlDbType.VarChar, Value = reportSearch.LoggedinUserType},
                       };
                    List<DashboardReport> result = context.Database.SqlQuery<DashboardReport>("uspGetDashboardReport @DateFrom,@DateTo,@Status,@Access,@SubmittedBy,@LoggedinUserType", sp).ToList<DashboardReport>();

                    LogManager.Debug("GetDashboardReport: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetDashboardReport: ERROR " + ex.Message, ex);
                return null;
            }

        }

        public List<ConfigMiscData> GetConfigMiscData()
        {
            try
            {
                LogManager.Debug("GetConfigMiscData: START");
                using (var context = new eInvoiceModelContext())
                {
                    List<ConfigMiscData> configRoles = new List<ConfigMiscData>();
                    configRoles = context.Database.SqlQuery<ConfigMiscData>("uspGetConfigMiscData").ToList();
                    LogManager.Debug("GetConfigMiscData: END");
                    return configRoles;
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("GetConfigMiscData: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public List<ConfigMiscData> SaveConfigMiscData(List<ConfigMiscData> configMiscDatas)
        {
            try
            {
                LogManager.Debug("SaveConfigMiscData: START");
                List<ConfigMiscData> resultList = new List<ConfigMiscData>();
                using (var context = new eInvoiceModelContext())
                {
                    foreach (ConfigMiscData configMiscData in configMiscDatas)
                    {
                        SqlParameter[] sp = new SqlParameter[]
                            {
                                new SqlParameter() {ParameterName = "@ConfigID", SqlDbType = SqlDbType.Int, Value= configMiscData.ConfigID},
                                new SqlParameter() {ParameterName = "@ConfiguredCol", SqlDbType = SqlDbType.VarChar, Value =  configMiscData.ConfiguredCol},
                                new SqlParameter() {ParameterName = "@ConfiguredColText", SqlDbType = SqlDbType.VarChar, Value = configMiscData.ConfiguredColText}
                            };
                        ConfigMiscData result = context.Database.SqlQuery<ConfigMiscData>("uspSaveConfigMiscData @ConfigID,@ConfiguredCol,@ConfiguredColText", sp).FirstOrDefault();
                        resultList.Add(result);
                    }
                    LogManager.Debug("SaveConfigMiscData: END");
                    return resultList;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveConfigMiscData: ERROR " + ex.Message, ex);
                return null;
            }

        }

        public List<ConfigEscalation> GetConfigEscalation()
        {
            try
            {
                LogManager.Debug("GetConfigEscalation: START");
                using (var context = new eInvoiceModelContext())
                {
                    List<ConfigEscalation> configRoles = new List<ConfigEscalation>();
                    configRoles = context.Database.SqlQuery<ConfigEscalation>("uspGetConfigEscalation").ToList();
                    LogManager.Debug("GetConfigEscalation: END");
                    return configRoles;
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("GetConfigEscalation: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public List<ConfigEscalation> SaveConfigEscalation(List<ConfigEscalation> configEscalations)
        {
            try
            {
                LogManager.Debug("SaveConfigEscalation: START");

                List<ConfigEscalation> resultList = new List<ConfigEscalation>();
                using (var context = new eInvoiceModelContext())
                {
                    foreach (ConfigEscalation configEscalation in configEscalations)
                    {
                        SqlParameter[] sp = new SqlParameter[]
                            {
                                new SqlParameter() {ParameterName = "@ConfigEscalationsID", SqlDbType = SqlDbType.Int, Value= configEscalation.ConfigEscalationsID},
                                new SqlParameter() {ParameterName = "@ActivityName", SqlDbType = SqlDbType.VarChar, Value =  configEscalation.ActivityName},
                                new SqlParameter() {ParameterName = "@FirstReminderDays", SqlDbType = SqlDbType.VarChar, Value = configEscalation.FirstReminderDays}
                            };
                        ConfigEscalation result = context.Database.SqlQuery<ConfigEscalation>("uspSaveConfigEscalation @ConfigEscalationsID,@ActivityName,@FirstReminderDays", sp).FirstOrDefault();
                        resultList.Add(result);
                    }
                    LogManager.Debug("SaveConfigEscalation: END");
                    return resultList;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetApproversLiSaveConfigEscalationstForLoggedRole: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public List<ConfigEscalation> DeleteConfigEscalation(List<ConfigEscalation> configEscalations)
        {
            try
            {
                LogManager.Debug("DeleteConfigEscalation: START");

                List<ConfigEscalation> resultList = new List<ConfigEscalation>();
                using (var context = new eInvoiceModelContext())
                {
                    foreach (ConfigEscalation configEscalation in configEscalations)
                    {
                        SqlParameter[] sp = new SqlParameter[]
                            {
                                 new SqlParameter() {ParameterName = "@ConfigEscalationsID", SqlDbType = SqlDbType.Int, Value=  configEscalation.ConfigEscalationsID}
                            };
                        ConfigEscalation result = context.Database.SqlQuery<ConfigEscalation>("uspDeleteConfigEscalation @ConfigEscalationsID", sp).FirstOrDefault();
                        resultList.Add(result);
                    }

                    LogManager.Debug("DeleteConfigEscalation: END");
                    return resultList;
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("DeleteConfigEscalation: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public List<ConfigRole> GetActivityRoleNames()
        {
            try
            {
                LogManager.Debug("GetActivityRoleNames: START");

                using (var context = new eInvoiceModelContext())
                {
                    List<ConfigRole> configRoles = new List<ConfigRole>();
                    configRoles = context.Database.SqlQuery<ConfigRole>("uspGetActivityRoleName").ToList();
                    LogManager.Debug("GetActivityRoleNames: END");
                    return configRoles;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetActivityRoleNames: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public List<ConfigRole> GetAdminRoleNames()
        {
            try
            {
                LogManager.Debug("GetAdminRoleNames: START");

                using (var context = new eInvoiceModelContext())
                {
                    List<ConfigRole> configRoles = new List<ConfigRole>();
                    configRoles = context.Database.SqlQuery<ConfigRole>("uspGetAdminRoleName").ToList();

                    LogManager.Debug("GetAdminRoleNames: END");
                    return configRoles;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetAdminRoleNames: ERROR " + ex.Message, ex);
                return null;
            }

        }

        public List<AdminRoleForCRUD> GetAdminRole()
        {
            try
            {
                LogManager.Debug("GetAdminRole: START");
                using (var context = new eInvoiceModelContext())
                {
                    List<AdminRoleForCRUD> configRoles = new List<AdminRoleForCRUD>();
                    configRoles = context.Database.SqlQuery<AdminRoleForCRUD>("uspGetAdminRole").ToList();
                    LogManager.Debug("GetAdminRole: END");
                    return configRoles;
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("GetAdminRole: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public List<AdminRoleForCRUD> SaveAdminRole(List<AdminRoleForCRUD> adminRoles)
        {
            try
            {
                LogManager.Debug("SaveAdminRole: START");
                List<AdminRoleForCRUD> resultList = new List<AdminRoleForCRUD>();
                using (var context = new eInvoiceModelContext())
                {
                    foreach (AdminRoleForCRUD adminRole in adminRoles)
                    {
                        if (adminRole.RoleID != 0 && string.IsNullOrEmpty(adminRole.ApproverUserID) == false)
                        {
                            SqlParameter[] sp = new SqlParameter[]
                            {
                                new SqlParameter() {ParameterName = "@AdminRoleID", SqlDbType = SqlDbType.Int, Value= adminRole.AdminRoleID},
                                new SqlParameter() {ParameterName = "@RoleID", SqlDbType = SqlDbType.Int, Value =  adminRole.RoleID},
                                new SqlParameter() {ParameterName = "@Approver", SqlDbType = SqlDbType.VarChar, Value = adminRole.ApproverUserID}
                            };
                            AdminRoleForCRUD result = context.Database.SqlQuery<AdminRoleForCRUD>("uspSaveAdminRole @AdminRoleID,@RoleID,@Approver", sp).FirstOrDefault();
                            resultList.Add(result);
                        }
                    }

                    LogManager.Debug("SaveAdminRole: END");
                    return resultList;
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("SaveAdminRole: ERROR " + ex.Message, ex);
                return null;
            }

        }

        //public List<DashboardReport> DeleteDocumentNo(DashboardReport report)
        //{
        //    try
        //    {
        //        LogManager.Debug("DeleteDocumentNo: START");
        //        List<DashboardReport> resultList = new List<DashboardReport>();
        //        using (var context = new eInvoiceModelContext())
        //        {
        //            //foreach (DashboardReport report in reports)
        //            //{
        //                SqlParameter[] sp = new SqlParameter[]
        //                    {
        //                        new SqlParameter() {ParameterName = "@InvoiceMasterId", SqlDbType = SqlDbType.Int, Value=  report.InvoiceMasterID}
        //                    };
        //                DashboardReport result = context.Database.SqlQuery<DashboardReport>("uspDeleteDocumentNo @InvoiceMasterId", sp).FirstOrDefault();
        //                resultList.Add(result);
        //            //}
        //            LogManager.Debug("DeleteDocumentNo: END");
        //            return resultList;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error("DeleteDocumentNo: ERROR " + ex.Message, ex);
        //        return null;
        //    }
        //}

        public List<DashboardReport> DeleteDocumentNo(List<DashboardReport> reports)
        {
            try
            {
                LogManager.Debug("DeleteDocumentNo: START");
                List<DashboardReport> resultList = new List<DashboardReport>();
                using (var context = new eInvoiceModelContext())
                {
                    foreach (DashboardReport report in reports)
                    {
                        SqlParameter[] sp = new SqlParameter[]
                            {
                                new SqlParameter() {ParameterName = "@InvoiceMasterId", SqlDbType = SqlDbType.Int, Value=  report.InvoiceMasterID}
                            };
                        DashboardReport result = context.Database.SqlQuery<DashboardReport>("uspDeleteDocumentNo @InvoiceMasterId", sp).FirstOrDefault();
                        resultList.Add(result);
                    }
                    LogManager.Debug("DeleteDocumentNo: END");
                    return resultList;
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("DeleteDocumentNo: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public List<AdminRoleForCRUD> DeleteAdminRole(List<AdminRoleForCRUD> adminRoles)
        {
            try
            {
                LogManager.Debug("DeleteAdminRole: START");
                List<AdminRoleForCRUD> resultList = new List<AdminRoleForCRUD>();
                using (var context = new eInvoiceModelContext())
                {
                    foreach (AdminRoleForCRUD adminRole in adminRoles)
                    {
                        SqlParameter[] sp = new SqlParameter[]
                            {
                                new SqlParameter() {ParameterName = "@AdminRoleID", SqlDbType = SqlDbType.Int, Value=  adminRole.AdminRoleID}
                            };
                        AdminRoleForCRUD result = context.Database.SqlQuery<AdminRoleForCRUD>("uspDeleteAdminRole @AdminRoleID", sp).FirstOrDefault();
                        resultList.Add(result);
                    }
                    LogManager.Debug("DeleteAdminRole: END");
                    return resultList;
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("DeleteAdminRole: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public IEnumerable<ExchangeEmployeeProfile> GetCATTFindingsApprover(string approverUserIds)
        {
            try
            {
                LogManager.Debug("GetCATTFindingsApprover: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                     {
                           new SqlParameter() {ParameterName = "@ApproverUserIDs", IsNullable=true, SqlDbType = SqlDbType.VarChar, Value = (object)approverUserIds ?? DBNull.Value}
                     };
                    var result = context.Database.SqlQuery<ExchangeEmployeeProfile>("uspGetCATTFindingsApprover @ApproverUserIDs", sp).ToList();
                    LogManager.Debug("GetCATTFindingsApprover: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetCATTFindingsApprover: ERROR " + ex.Message, ex);
                return null;
            }

        }

        public InvoiceGridTotalsforPDF GetInvoiceGridTotalsforPDF(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetInvoiceGridTotalsforPDF: START");

                using (var context = new eInvoiceModelContext())
                {
                    InvoiceGridTotalsforPDF pdfTotals= new InvoiceGridTotalsforPDF();
                    SqlParameter[] sp = new SqlParameter[]
                    {
                        new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value=invoiceMasterID}
                    };
                    pdfTotals = context.Database.SqlQuery<InvoiceGridTotalsforPDF>("uspGetGridTotalsforPDF @InvoiceMasterID", sp).FirstOrDefault();
                    LogManager.Debug("GetInvoiceGridTotalsforPDF: END");
                    return pdfTotals;
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("GetInvoiceGridTotalsforPDF: ERROR " + ex.Message, ex);
                return null;
            }

        }


        public List<InvoiceCAFindingsEmp> GetInvoiceCAFindingsEmp(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetInvoiceCAFindingsEmp: START");

                using (var context = new eInvoiceModelContext())
                {
                    List<InvoiceCAFindingsEmp> caFindingsEmp = new List<InvoiceCAFindingsEmp>();
                    SqlParameter[] sp = new SqlParameter[]
                    {
                        new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value=invoiceMasterID}
                    };
                    caFindingsEmp = context.Database.SqlQuery<InvoiceCAFindingsEmp>("uspGetInvoiceCAFindingsEmp @InvoiceMasterID", sp).ToList();
                    LogManager.Debug("GetInvoiceCAFindingsEmp: END");
                    return caFindingsEmp;
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("GetInvoiceCAFindingsEmp: ERROR " + ex.Message, ex);
                return null;
            }

        }

        public List<InvoiceCAFindingsEmp> SaveInvoiceCAFindingsEmp(List<InvoiceCAFindingsEmp> cafindings)
        {
            try
            {
                LogManager.Debug("SaveInvoiceCAFindingsEmp: START");
                List<InvoiceCAFindingsEmp> resultList = new List<InvoiceCAFindingsEmp>();
                using (var context = new eInvoiceModelContext())
                {
                    foreach (InvoiceCAFindingsEmp cattfinding in cafindings)
                    {
                        SqlParameter[] sp = new SqlParameter[]
                            {
                                new SqlParameter() {ParameterName = "@InvoiceCAFindingsEmpID", SqlDbType = SqlDbType.Int, Value= cattfinding.InvoiceCAFindingsEmpID},
                                new SqlParameter() {ParameterName = "@InvoiceCATTFindingsID", SqlDbType = SqlDbType.Int, Value= cattfinding.InvoiceCATTFindingsID},
                                new SqlParameter() {ParameterName = "@EmployeeUserID",IsNullable=true, SqlDbType = SqlDbType.VarChar,  Value = (object)cattfinding.EmployeeName ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@Classification",IsNullable=true, SqlDbType = SqlDbType.VarChar, Value = (object) cattfinding.Classification ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@InvoiceRate",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value =  (object)cattfinding.InvoiceRate ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@ApprovedRate",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value =  (object)cattfinding.ApprovedRate ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@RateVariance",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value = (object) cattfinding.RateVariance ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@InvoiceHours",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value = (object) cattfinding.InvoiceHours ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@ApprovedHours",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value =  (object)cattfinding.ApprovedHours ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@VarianceHours",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value =  (object)cattfinding.VarianceHours ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@Total",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value =  (object)cattfinding.Total ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@Notes",IsNullable=true, SqlDbType = SqlDbType.VarChar,  Value = (object)cattfinding.Notes ?? DBNull.Value},
                            };
                        InvoiceCAFindingsEmp result = context.Database.SqlQuery<InvoiceCAFindingsEmp>("uspSaveInvoiceCAFindingsEmp @InvoiceCAFindingsEmpID,@InvoiceCATTFindingsID," +
                        "@EmployeeUserID,@Classification,@InvoiceRate,@ApprovedRate,@RateVariance,@InvoiceHours,@ApprovedHours,@VarianceHours,@Total,@Notes ", sp).FirstOrDefault();
                        resultList.Add(result);
                    }
                    LogManager.Debug("SaveInvoiceCAFindingsEmp: END");
                    return resultList;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveInvoiceCAFindingsEmp: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public List<InvoiceCAFindingsEmp> DeleteInvoiceCAFindingsEmp(List<InvoiceCAFindingsEmp> caFindingEmps)
        {
            try
            {
                LogManager.Debug("DeleteInvoiceCAFindingsEmp: START");
                List<InvoiceCAFindingsEmp> resultList = new List<InvoiceCAFindingsEmp>();
                using (var context = new eInvoiceModelContext())
                {
                    foreach (InvoiceCAFindingsEmp caFindingEmp in caFindingEmps)
                    {
                        SqlParameter[] sp = new SqlParameter[]
                        {
                             new SqlParameter() {ParameterName = "@InvoiceCAFindingsEmpID", SqlDbType = SqlDbType.Int, Value=  caFindingEmp.InvoiceCAFindingsEmpID}
                        };
                        resultList = context.Database.SqlQuery<InvoiceCAFindingsEmp>("uspDeleteInvoiceCAFindingsEmp @InvoiceCAFindingsEmpID", sp).ToList();
                        //  resultList.Add(result);
                    }

                    LogManager.Debug("DeleteInvoiceCAFindingsEmp: END");
                    return resultList;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("DeleteInvoiceCAFindingsEmp: ERROR " + ex.Message, ex);
                return null;
            }
        }


        public List<InvoiceCATTFindingsEmp> GetInvoiceCATTFindingsEmp(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetInvoiceCATTFindingsEmp: START");

                using (var context = new eInvoiceModelContext())
                {
                    List<InvoiceCATTFindingsEmp> cattFindingsEmp = new List<InvoiceCATTFindingsEmp>();
                    SqlParameter[] sp = new SqlParameter[]
                    {
                        new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value=invoiceMasterID}
                    };
                    cattFindingsEmp = context.Database.SqlQuery<InvoiceCATTFindingsEmp>("uspGetInvoiceCATTFindingsEmp @InvoiceMasterID", sp).ToList();
                    LogManager.Debug("GetInvoiceCATTFindingsEmp: END");
                    return cattFindingsEmp;
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("GetInvoiceCATTFindingsEmp: ERROR " + ex.Message, ex);
                return null;
            }

        }

        public List<InvoiceCATTFindingsEmp> SaveInvoiceCATTFindingsEmp(List<InvoiceCATTFindingsEmp> cattfindings)
        {
            try
            {
                LogManager.Debug("SaveInvoiceCATTFindingsEmp: START");
                List<InvoiceCATTFindingsEmp> resultList = new List<InvoiceCATTFindingsEmp>();
                using (var context = new eInvoiceModelContext())
                {
                    foreach (InvoiceCATTFindingsEmp cattfinding in cattfindings)
                    {
                        SqlParameter[] sp = new SqlParameter[]
                            {
                                new SqlParameter() {ParameterName = "@InvoiceCATTFindingsEmpID", SqlDbType = SqlDbType.Int, Value= cattfinding.InvoiceCATTFindingsEmpID},
                                new SqlParameter() {ParameterName = "@InvoiceCATTFindingsID", SqlDbType = SqlDbType.Int, Value= cattfinding.InvoiceCATTFindingsID},
                                new SqlParameter() {ParameterName = "@EmployeeUserID",IsNullable=true, SqlDbType = SqlDbType.VarChar,  Value = (object)cattfinding.EmployeeName ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@Classification",IsNullable=true, SqlDbType = SqlDbType.VarChar, Value = (object) cattfinding.Classification ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@InvoiceRate",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value =  (object)cattfinding.InvoiceRate ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@ApprovedRate",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value =  (object)cattfinding.ApprovedRate ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@RateVariance",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value = (object) cattfinding.RateVariance ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@InvoiceHours",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value = (object) cattfinding.InvoiceHours ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@ApprovedHours",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value =  (object)cattfinding.ApprovedHours ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@VarianceHours",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value =  (object)cattfinding.VarianceHours ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@Total",IsNullable=true, SqlDbType = SqlDbType.Decimal, Value =  (object)cattfinding.Total ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@Notes",IsNullable=true, SqlDbType = SqlDbType.VarChar,  Value = (object)cattfinding.Notes ?? DBNull.Value},
                            };
                        InvoiceCATTFindingsEmp result = context.Database.SqlQuery<InvoiceCATTFindingsEmp>("uspSaveInvoiceCATTFindingsEmp @InvoiceCATTFindingsEmpID,@InvoiceCATTFindingsID," +
                        "@EmployeeUserID,@Classification,@InvoiceRate,@ApprovedRate,@RateVariance,@InvoiceHours,@ApprovedHours,@VarianceHours,@Total,@Notes ", sp).FirstOrDefault();
                        resultList.Add(result);
                    }
                    LogManager.Debug("SaveInvoiceCATTFindingsEmp: END");
                    return resultList;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("SaveInvoiceCATTFindingsEmp: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public List<InvoiceCATTFindingsEmp> DeleteInvoiceCATTFindingsEmp(List<InvoiceCATTFindingsEmp> cattFindingEmps)
        {
            try
            {
                LogManager.Debug("DeleteInvoiceCATTFindingsEmp: START");
                List<InvoiceCATTFindingsEmp> resultList = new List<InvoiceCATTFindingsEmp>();
                using (var context = new eInvoiceModelContext())
                {
                    foreach (InvoiceCATTFindingsEmp cattFindingEmp in cattFindingEmps)
                    {
                        SqlParameter[] sp = new SqlParameter[]
                        {
                             new SqlParameter() {ParameterName = "@InvoiceCATTFindingsEmpID", SqlDbType = SqlDbType.Int, Value=  cattFindingEmp.InvoiceCATTFindingsEmpID}
                        };
                        resultList = context.Database.SqlQuery<InvoiceCATTFindingsEmp>("uspDeleteInvoiceCATTFindingsEmp @InvoiceCATTFindingsEmpID", sp).ToList();
                        //  resultList.Add(result);
                    }

                    LogManager.Debug("DeleteInvoiceCATTFindingsEmp: END");
                    return resultList;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("DeleteInvoiceCATTFindingsEmp: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public InvoiceCATTFindings UpdateInvoiceCATTFindings(InvoiceCATTFindings cattFind)
        {
            try
            {
                LogManager.Debug("UpdateInvoiceCATTFindings: START");

                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                            {
                                new SqlParameter() {ParameterName = "@InvoiceCATTFindingsID", SqlDbType = SqlDbType.Int, Value= cattFind.InvoiceCATTFindingsID},
                                new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value= cattFind.InvoiceMasterID},
                                new SqlParameter() {ParameterName = "@AddressedTo",IsNullable=true, SqlDbType = SqlDbType.VarChar, Value = (object)cattFind.AddressedTo ?? DBNull.Value },
                                new SqlParameter() {ParameterName = "@SentFrom", IsNullable=true, SqlDbType = SqlDbType.VarChar, Value = (object)cattFind.SentFrom ?? DBNull.Value },
                                new SqlParameter() {ParameterName = "@Date", IsNullable=true, SqlDbType = SqlDbType.DateTime, Value = (object) cattFind.Date ?? DBNull.Value },
                                new SqlParameter() {ParameterName = "@CATTRecommendedAdjustment", IsNullable=true, SqlDbType = SqlDbType.Decimal, Value = (object) cattFind.CATTRecommendedAdjustment ?? DBNull.Value},
                                new SqlParameter() {ParameterName = "@CARecommendedAdjustment", IsNullable=true, SqlDbType = SqlDbType.Decimal, Value = (object) cattFind.CARecommendedAdjustment ?? DBNull.Value },
                                new SqlParameter() {ParameterName = "@CATTNotes", IsNullable=true, SqlDbType = SqlDbType.VarChar,  Value = (object) cattFind.CATTNotes ?? DBNull.Value },
                                new SqlParameter() {ParameterName = "@CANotes", IsNullable=true, SqlDbType = SqlDbType.VarChar, Value = (object) cattFind.CANotes ?? DBNull.Value},
                            };
                    InvoiceCATTFindings result = context.Database.SqlQuery<InvoiceCATTFindings>("uspUpdateInvoiceCATTFindings @InvoiceCATTFindingsID,@InvoiceMasterID," +
                    "@AddressedTo,@SentFrom,@Date,@CATTRecommendedAdjustment,@CARecommendedAdjustment,@CATTNotes,@CANotes", sp).FirstOrDefault();
                    LogManager.Debug("UpdateInvoiceCATTFindings: END");
                    return result;
                }
            }

            catch (Exception ex)
            {
                LogManager.Error("UpdateInvoiceCATTFindings: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public InvoiceCATTFindings GetInvoiceCATTFindings(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetInvoiceCATTFindings: START");

                using (var context = new eInvoiceModelContext())
                {
                    InvoiceCATTFindings cattFinding = new InvoiceCATTFindings();
                    SqlParameter[] sp = new SqlParameter[]
                     {
                         new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value=invoiceMasterID}
                     };
                    cattFinding = context.Database.SqlQuery<InvoiceCATTFindings>("uspGetInvoiceCATTFindings @InvoiceMasterID", sp).FirstOrDefault();
                    LogManager.Debug("GetInvoiceCATTFindings: END");
                    return cattFinding;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetInvoiceCATTFindings: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public void UpdateNewDocumentNoChanges(int invoiceMasterID, string userId,bool loadFromSAP)
        {
            try
            {
                LogManager.Debug("UpdateNewDocumentNoChanges: START");

                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                       {
                           new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value = invoiceMasterID },
                           new SqlParameter() {ParameterName = "@UserId", SqlDbType = SqlDbType.VarChar, Value = userId },
                           new SqlParameter() {ParameterName = "@LoadFromSAP", SqlDbType = SqlDbType.Bit, Value = loadFromSAP }
                       };
                    var result = context.Database.SqlQuery<InvoicePODetailChanges>("uspUpdateNewDocumentNoChanges @InvoiceMasterID,@UserId,@LoadFromSAP", sp).ToList<InvoicePODetailChanges>();
                }
                LogManager.Debug("UpdateNewDocumentNoChanges: END");
            }

            catch (Exception ex)
            {
                LogManager.Error("UpdateNewDocumentNoChanges: ERROR " + ex.Message, ex);

            }

        }

        public decimal GetInvoiceAmount(int invoiceMasterID)
        {
            try
            {
                LogManager.Debug("GetInvoiceAmount: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                    {
                         new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value = invoiceMasterID }
                    };
                    var result = context.Database.SqlQuery<decimal>("uspGetInvoiceAmount @InvoiceMasterId", sp).FirstOrDefault();
                    LogManager.Debug("GetInvoiceAmount: END");
                    return Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetInvoiceAmount: ERROR " + ex.Message, ex);
                return Decimal.Zero;
            }
        }


        public decimal GetConfiguredDMAmount()
        {
            try
            {
                LogManager.Debug("GetConfiguredDMAmount: START");
                using (var context = new eInvoiceModelContext())
                {                    
                    var result = context.Database.SqlQuery<string>("uspGetConfiguredDMAmount").FirstOrDefault();
                    LogManager.Debug("GetConfiguredDMAmount: END");
                    return Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetConfiguredDMAmount: ERROR " + ex.Message, ex);
                return Decimal.Zero;
            }
        }


        public List<InvoicePODetail> GetPONumbersFromInvoicePODetail(int invoiceMasterId)
        {
            try
            {
                LogManager.Debug("GetPONumbersFromInvoicePODetail: START");
                using (var context = new eInvoiceModelContext())
                {
                    SqlParameter[] sp = new SqlParameter[]
                    {
                         new SqlParameter() {ParameterName = "@InvoiceMasterID", SqlDbType = SqlDbType.Int, Value = invoiceMasterId }
                    };
                    var result = context.Database.SqlQuery<InvoicePODetail>("uspGetPONumbersFromInvoicePODetail @InvoiceMasterId", sp).ToList<InvoicePODetail>();
                    LogManager.Debug("GetConfiguredDMAmount: END");
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("GetPONumbersFromInvoicePODetail: ERROR " + ex.Message, ex);
                return null;
            }
        }

        public List<InvoiceMaster> GetInvoiceMasterIDForUploadFix()
            {
            try
                {
                LogManager.Debug("GetInvoiceMasterIDForUploadFix: START");
                using (var context = new eInvoiceModelContext())
                    {
                    //var clientIdParameter = new SqlParameter("@InvoiceMasterID");
                    var result = context.Database.SqlQuery<InvoiceMaster>("uspUploadtoSPFixGeteInvoiceMasterIDs").ToList<InvoiceMaster>();
                    LogManager.Debug("GetInvoiceMasterIDForUploadFix: END");
                    return result;
                    }
                }
            catch (Exception ex)
                {
                LogManager.Error("GetInvoiceMasterIDForUploadFix: ERROR " + ex.Message, ex);
                return null;
                }
            }
    }
}

