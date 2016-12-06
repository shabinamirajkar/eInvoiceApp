using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace eInvoiceAutomationWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //Init Log4Net Configuration..
            log4net.Config.XmlConfigurator.Configure();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            // Register global filter
            //GlobalFilters.Filters.Add(new SessionTimeOutFilter());

#if DEBUG
            {
                BundleTable.EnableOptimizations = true;
            }
#endif
        }
        protected void Session_Start()
        {
            //Setting up value of custom Column, used in Log4Net.
            string userId = User.Identity.Name;
            int index = userId.IndexOf("\\");
            string logginUserName = userId.Substring(index + 1);
            log4net.GlobalContext.Properties["UserID"] = logginUserName;
        }

        protected void Session_End()
        {
            //Response.Redirect("http://localhost/eInvoice/home/SessionTimeOut");
            //GlobalFilters.Filters.Add(new SessionTimeOutFilter());
            //if (Context.Items["SessionTimedOut"] is bool)
            //{
            //     Context.Response.StatusCode = 306;
            //     Context.Response.End();
            //}

        }

    }
}
