using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using SourceCode.Workflow.Client;
using System.Data;
using System.Text;


namespace eInvoice.K2Access
{
    public class K2Service
    {
        private static readonly log4net.ILog LogManager = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string _connectionString = string.Empty;

        /// <summary>
        /// Constructor with connection string, which will use to connect database.
        /// </summary>
        /// <param name="connectionString"></param>
        public K2Service(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Constructor without connection string, will use configuration to get connction string to connect database.
        /// </summary>
        /// <param name="connectionString"></param>
        public K2Service()
        {


        }

        private static Connection GetK2Connection()
        {
            return GetSecureK2Connection();
        }

        /// <summary>
        /// Open K2 connection for service and return connection object
        /// </summary>
        /// <returns>Connection</returns>
        private static Connection GetSecureK2Connection()
        {
            Connection connection = null;
            try
            {
                LogManager.Debug("GetSecureK2Connection: START");
                //Open a connection to K2
                connection = new Connection();
                connection.Open(K2Configuration.ServerName);
            }
            catch (Exception ex)
            {
                LogManager.Error("GetSecureK2Connection: ERROR " + ex.Message, ex);
                throw ex;
            }
            LogManager.Debug("GetSecureK2Connection: END");
            return connection;
        }

        /// <summary>
        /// Open K2 connection for loggedin user and return connection object
        /// </summary>
        /// <returns>Connection</returns>
        private static Connection GetK2ConnectionForUser(string userName)
        {
            Connection connection = null;
            try
            {
                LogManager.Debug("GetK2ConnectionForUser: START");
                //Open a connection to K2
                connection = GetK2Connection();
                connection.ImpersonateUser(userName);
            }
            catch (Exception ex)
            {
                LogManager.Error("GetK2ConnectionForUser: ERROR " + ex.Message, ex);
                throw ex;
            }

            LogManager.Debug("GetK2ConnectionForUser: END");
            return connection;
        }

        /// <summary>
        /// Start a new eInvoice Process Instance
        /// </summary>
        /// <param name="RequestID">eInvoicerequestdetailID</param>
        /// <param name="userName">username of the requester</param>
        /// <returns>returns ProcessInstanceID, or 0 if could not start ProcessInstance</returns>
        public static int StarteInvoiceRequest(int InvoiceMasterID, string invoiceNo,int roleID, string userName)
        {
            int K2ProcID = 0;
            Connection cnnProcess = GetK2ConnectionForUser(userName);
          //  Connection cnnProcess = GetSecureK2Connection();
            try
            {
                LogManager.Debug("StarteInvoiceRequest: START");
                // create an instance of the k2 process
                string K2processName = "FI.eInvoice\\eInvoiceApprovalProcess.WF";
                SourceCode.Workflow.Client.ProcessInstance K2procInst = cnnProcess.CreateProcessInstance(K2processName);

                // set data fields..
                K2procInst.DataFields["InvoiceMasterID"].Value = InvoiceMasterID;
                K2procInst.DataFields["RoutetoRoleID"].Value = roleID;

                // set Process Folio
                K2procInst.Folio = "eInvoice - " + invoiceNo + " - " + InvoiceMasterID.ToString();

                // start the process
                cnnProcess.StartProcessInstance(K2procInst, false);

                // get the process inst id
                K2ProcID = K2procInst.ID;

                cnnProcess.Close();
                LogManager.Debug("StarteInvoiceRequest: END");
                return K2ProcID;
            }

            catch (Exception ex)
            {
                LogManager.Error("StarteInvoiceRequest: ERROR " + ex.Message, ex);
                return K2ProcID;
            }
        }

        /// <summary>
        /// Gets worklist of the passed user
        /// </summary>
        /// <param name="userName">username</param>
        /// <returns>K2 worklist Object</returns>
        public static Worklist GetWorklist(string userName)
        {
            Worklist worklist = null;
            Connection connection = GetK2ConnectionForUser(userName);
            try
            {
                LogManager.Debug("GetWorklist: START");
                WorklistCriteria wc = new WorklistCriteria();
                wc.AddFilterField(WCField.ProcessName, WCCompare.Equal, "eInvoice");
                worklist = connection.OpenWorklist(wc);
                LogManager.Debug("GetWorklist: END");
                return worklist;
            }
            catch (Exception ex)
            {
                LogManager.Error("GetWorklist: ERROR " + ex.Message, ex);
                return worklist;
            }

        }

        /// <summary>
        /// Attempts to action the passed request against SrNo 
        /// </summary>
        /// <param name="serialNumber">K2 SrNo request</param>
        /// <param name="action">Action (Approve or Reject)</param>
        /// <param name="userName">username</param>
        /// <returns>success Message or Error Message</returns>
        public static string ActionWorklistItem(string serialNumber, int invoiceMasterID, int routeTo, string action, string userName,
            string sharedUser = "", string managedUser = "")
        {
            string retValue = "Could not find worklist item";
            Connection connection = null;
            try
            {
                LogManager.Debug("ActionWorklistItem: START");
                connection = GetK2ConnectionForUser(userName);
                
                WorklistItem worklistItem = null;
                //worklistItem = connection.OpenWorklistItem(serialNumber);

                worklistItem = OpenWorklistItemShared(serialNumber, userName, sharedUser, managedUser);

                if (worklistItem != null)
                {
                    if (worklistItem.Actions.Count > 0)
                    {
                        worklistItem.ProcessInstance.DataFields["InvoiceMasterID"].Value = invoiceMasterID;
                        worklistItem.ProcessInstance.DataFields["RoutetoRoleID"].Value = routeTo;
                        worklistItem.Actions[action].Execute();
                        retValue = "Successfully actioned worklist item";
                    }
                    else
                        throw new Exception("Worklist Item could not be actioned");
                }
                LogManager.Debug("ActionWorklistItem: END");
                return retValue;
            }

            catch (Exception ex)
            {
                LogManager.Error("ActionWorklistItem: ERROR " + ex.Message, ex);
                retValue = "error: " + ex.Message;
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
            LogManager.Debug("ActionWorklistItem: END");
            return retValue;
        }

        public static WorklistItem OpenWorklistItemShared(string serialNumber, string userName,  string sharedUser = "", string managedUser = "")
            {
            
            WorklistItem worklistItem = null;
            Connection connection = null;
            connection = GetK2ConnectionForUser(userName);

            // Check if the worklist item is for a normal user
            if ((sharedUser == string.Empty) && (managedUser == string.Empty))
                {
                worklistItem = connection.OpenWorklistItem(serialNumber, "ASP");
                }
            // Check if the worklist item is for an Out of Office shared user
            if ((sharedUser != string.Empty) && (managedUser == string.Empty))
                {
                worklistItem = connection.OpenSharedWorklistItem(sharedUser, managedUser, serialNumber);
                }
            // Check if the worklist item is for a normal Manager
            if ((sharedUser == string.Empty) && (managedUser != string.Empty))
                {
                worklistItem = connection.OpenManagedWorklistItem(managedUser, serialNumber);
                }
            // Check if the worklist item is for an Out of Office user's Manager
            if ((sharedUser != string.Empty) && (managedUser != string.Empty))
                {
                worklistItem = connection.OpenSharedWorklistItem(sharedUser, managedUser, serialNumber);
                }

            return worklistItem;
            }

        public static bool VerifyWorklistItemShared(string serialNumber, string userName, string sharedUser = "", string managedUser = "")
            {

            WorklistItem worklistItem = null;
            Connection connection = null;
            connection = GetK2ConnectionForUser(userName);

            // Check if the worklist item is for a normal user
            if ((sharedUser == string.Empty) && (managedUser == string.Empty))
                {
                worklistItem = connection.OpenWorklistItem(serialNumber, "ASP");
                }
            // Check if the worklist item is for an Out of Office shared user
            if ((sharedUser != string.Empty) && (managedUser == string.Empty))
                {
                worklistItem = connection.OpenSharedWorklistItem(sharedUser, managedUser, serialNumber);
                }
            // Check if the worklist item is for a normal Manager
            if ((sharedUser == string.Empty) && (managedUser != string.Empty))
                {
                worklistItem = connection.OpenManagedWorklistItem(managedUser, serialNumber);
                }
            // Check if the worklist item is for an Out of Office user's Manager
            if ((sharedUser != string.Empty) && (managedUser != string.Empty))
                {
                worklistItem = connection.OpenSharedWorklistItem(sharedUser, managedUser, serialNumber);
                }

            if (worklistItem != null && worklistItem.Actions.Count > 0)
                {
                return true;
                }
            LogManager.Debug("VerifyWorklistItem: END");
            return false;
            }
         /// <summary>
        /// Attempts to action the passed request against SrNo 
        /// </summary>
        /// <param name="serialNumber">K2 SrNo request</param>
        /// <param name="action">Action (Approve or Reject)</param>
        /// <param name="userName">username</param>
        /// <returns>success Message or Error Message</returns>
        public static bool VerifyWorklistItem(string serialNumber, string userName)
        {
            string retValue = "Could not find worklist item";
            Connection connection = null;
            try
            {
                LogManager.Debug("VerifyWorklistItem: START");
                connection = GetK2ConnectionForUser(userName);
                //SM 12/09/2015 - need to comment this, not needed Worklist worklist = connection.OpenWorklist();
                WorklistItem worklistItem = null;
                worklistItem = connection.OpenWorklistItem(serialNumber);

                if (worklistItem != null && worklistItem.Actions.Count > 0)
                {
                    return true;
                }
                LogManager.Debug("VerifyWorklistItem: END");
                return false;
            }

            catch (Exception ex)
            {
                LogManager.Error("VerifyWorklistItem: ERROR " + ex.Message, ex);
                retValue = "error: " + ex.Message;
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
            LogManager.Debug("VerifyWorklistItem: END");
            return false;
        }
    }
}