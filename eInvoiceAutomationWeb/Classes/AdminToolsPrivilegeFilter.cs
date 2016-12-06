using eInvoice.K2Access;
using eInvoiceApplication.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace eInvoiceAutomationWeb
{
    // MVC Action Filter class which gets executed before an Action executes
    public class AdminToolsPrivilegeFilter : ActionFilterAttribute
    {
        private static readonly log4net.ILog LogManager = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                LogManager.Debug("OnActionExecuting: START");

                string logginUserName = String.Empty;
                base.OnActionExecuting(filterContext);
                using (var eInvoiceModelContext = new eInvoiceModelContext())
                {
                    string userId = HttpContext.Current.User.Identity.Name;
                    // Removing Domain name from the logged in user name
                    if (!String.IsNullOrEmpty(userId))
                    {
                        int index = userId.IndexOf("\\");
                        logginUserName = userId.Substring(index + 1);

                        //  var url = HttpContext.Current.Request.Url.ToString();

                        if (filterContext.ActionDescriptor.ActionName == "RolesAdmin" ||
                            filterContext.ActionDescriptor.ActionName == "ConfigMiscData" ||
                            filterContext.ActionDescriptor.ActionName == "ConfigEscalation" ||
                            filterContext.ActionDescriptor.ActionName == "ErrorLogReport")
                        {
                            //Only Super Admin can have Access this Action..
                            var result = eInvoiceModelContext.GetConfiguredUser("SADMN", 0);
                            var authenticated = (from configuredName in result where configuredName.ToLower() == logginUserName.ToLower() select configuredName);
                            if (authenticated == null || authenticated.Count() == 0)
                            {
                                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Home" }, { "action", "AccessDenied" } });
                            }
                        }
                        else if (filterContext.ActionDescriptor.ActionName == "SavedInvoice")
                        {
                            //Only AP can have Access this Action..
                            var result = eInvoiceModelContext.GetConfiguredUser("AP", 0);
                            var authenticated = (from configuredName in result where configuredName.ToLower() == logginUserName.ToLower() select configuredName);
                            if (authenticated == null || authenticated.Count() == 0)
                            {
                                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Home" }, { "action", "AccessDenied" } });
                            }
                        }
                        else if (filterContext.ActionDescriptor.ActionName == "Report")
                        {
                            //Only a valid user of eInvoice system can access Reports..
                            bool ValidUser = eInvoiceModelContext.AuthenticateUser(logginUserName);
                            if (!ValidUser)
                            {
                                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Home" }, { "action", "AccessDenied" } });
                            }
                        }
                        //Security for worklist Tab
                        else if (filterContext.ActionDescriptor.ActionName == "Index")
                        {
                            //Only a valid user of eInvoice system can access Reports..
                            bool ValidUser = eInvoiceModelContext.AuthenticateUser(logginUserName);
                            if (!ValidUser)
                            {
                                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Home" }, { "action", "AccessDenied" } });
                            }
                        }
                        //Security for Dashboard Tab
                        else if (filterContext.ActionDescriptor.ActionName == "Dashboard")
                        {
                            //Only a valid user of eInvoice system can access Reports..
                            bool ValidUser = eInvoiceModelContext.AuthenticateUser(logginUserName);
                            if (!ValidUser)
                            {
                                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Home" }, { "action", "AccessDenied" } });
                            }
                        }
                    }
                }
                LogManager.Debug("OnActionExecuting: END");
            }
            catch (Exception ex)
            {
                LogManager.Error("OnActionExecuting: ERROR " + ex.Message, ex);
            }
        }
    }
}