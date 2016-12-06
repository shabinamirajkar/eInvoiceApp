using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;
using System.Web;

namespace eInvoiceAutomationWeb.Active_Directory_Access
{
    public class ActiveDirectoryHelper
    {
        private DirectoryEntry _directoryEntry = null;

        private String LDAPPath
        {
            get
            {
                return ConfigurationManager.AppSettings["LDAPPath"];
            }
        }
        private String LDAPUser
        {
            get
            {
                return ConfigurationManager.AppSettings["LDAPUser"];
            }
        }

        private String LDAPPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["LDAPPassword"];
            }
        }

        private DirectoryEntry SearchRoot
        {
            get
            {
                if (_directoryEntry == null)
                {
                    _directoryEntry = new DirectoryEntry(LDAPPath, LDAPUser, LDAPPassword, AuthenticationTypes.Secure);
                }
                return _directoryEntry;
            }
        }


        public List<ADUserDetail> GetUsersByFirstName(string fName)
        {
            //UserProfile user;
            List<ADUserDetail> userlist = new List<ADUserDetail>();
            string filter = "";
              _directoryEntry = null;
            DirectorySearcher directorySearch = new DirectorySearcher(SearchRoot);
            directorySearch.Asynchronous = true;
            directorySearch.CacheResults = true;
            //directorySearch.Filter = "(&(objectClass=user)(SAMAccountName=" + userName + "))";
            filter = string.Format("(givenName={0}*", fName);
            //filter = "(&(objectClass=user)(objectCategory=person)" + filter + ")";
            filter = "(&(objectClass=user)(objectCategory=person)(givenName=" + fName + "*))";


            directorySearch.Filter = filter;

            SearchResultCollection userCollection = directorySearch.FindAll();
            foreach (SearchResult users in userCollection)
            {
                DirectoryEntry userEntry = new DirectoryEntry(users.Path, LDAPUser, LDAPPassword);
                ADUserDetail userInfo = ADUserDetail.GetUser(userEntry);
                userlist.Add(userInfo);
            }
            return userlist;
        }

        public ADUserDetail GetUserByLoginName(String userName)
        {
            try
            {
                _directoryEntry = null;
                DirectorySearcher directorySearch = new DirectorySearcher(SearchRoot);
                directorySearch.Filter = "(&(objectClass=user)(SAMAccountName=" + userName + "))";
                SearchResult results = directorySearch.FindOne();

                if (results != null)
                {
                    DirectoryEntry user = new DirectoryEntry(results.Path, LDAPUser, LDAPPassword);
                    return ADUserDetail.GetUser(user);
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}