using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.DirectoryServices;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAL
{
    class Program
    {
        static void Main(string[] args)
        {
            string alias = "mrkar";
            Collection<ActiveDirectoryModel> userData = SearchAndFetchGalUsersList(alias);
            string[] managerDets = userData[0].Manager.Split(',');

            string managerName = managerDets[0].Substring(3, managerDets[0].Length - 3);
        }

        public class ActiveDirectoryModel
        {
            public string UserId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime HiredDate { get; set; }
            public string EmailId { get; set; }
            public string UserDisplayName { get; set; }
            public string UserAccountControl { get; set; }
            public long LastLogOn { get; set; }
            public string JobTitle { get; set; }
            public string ParticipantType { get; set; }
            public string Location { get; set; }
            public string Manager { get; set; }
        }

        public static Collection<ActiveDirectoryModel> SearchAndFetchGalUsersList(string searchText)
        {
            SearchResultCollection results;
            Collection<ActiveDirectoryModel> result = new Collection<ActiveDirectoryModel>();

            int sizeLimit = 1;
            string[] properties = { "c", "co", "countryCode", "samaccountname", "displayname", "givenname", "initials", "sn", "l", "st", "title", "dc", "mail", "mobile", "MemberOf", "msRTCSIP-PrimaryUserAddress", "manager" };

            if (string.IsNullOrEmpty(searchText) || searchText == "*")
            {
                return new Collection<ActiveDirectoryModel>(result);
            }


            string filterString3 = string.Format(CultureInfo.InvariantCulture, "(samaccountname={0})", searchText);

            string DirectoryPath = ConfigurationManager.AppSettings["LDAPAddress"].ToString();
            string UserName = ConfigurationManager.AppSettings["LDAPUserID"].ToString();
            string Password = ConfigurationManager.AppSettings["LDAPPassword"].ToString();

            using (DirectoryEntry searchRoot = new DirectoryEntry(DirectoryPath, UserName, Password, AuthenticationTypes.Secure))
            {
                using (DirectorySearcher search = new DirectorySearcher(searchRoot))
                {
                    search.Filter = string.Format(CultureInfo.CurrentCulture, "(&(objectClass=user){0})", filterString3);

                    search.SearchScope = SearchScope.Subtree;
                    search.PropertiesToLoad.AddRange(properties);
                    search.Sort.PropertyName = "displayname";
                    search.Sort.Direction = System.DirectoryServices.SortDirection.Ascending;
                    if (!sizeLimit.Equals(0))
                    {
                        search.SizeLimit = sizeLimit;
                    }
                    results = search.FindAll();
                }
            }


            if (results != null && results.Count != 0)
            {
                foreach (SearchResult searchResult in results)
                {
                    AddGalUserDetails(result, searchResult);
                }
            }
            return new Collection<ActiveDirectoryModel>(result);
        }

        public static void AddGalUserDetails(Collection<ActiveDirectoryModel> result, SearchResult searchResult)
        {
            ActiveDirectoryModel userADDetails = new ActiveDirectoryModel();

            if (result != null)
            {
                if (searchResult != null)
                {
                    if (searchResult.Properties.Contains("samaccountname") && searchResult.Properties["samaccountname"][0] != null)
                    {
                        userADDetails.UserId = searchResult.Properties["samaccountname"][0].ToString();
                    }
                    if (searchResult.Properties.Contains("givenName") && searchResult.Properties["givenName"][0] != null)
                    {
                        userADDetails.FirstName = searchResult.Properties["givenName"][0].ToString();
                    }
                    if (searchResult.Properties.Contains("sn") && searchResult.Properties["sn"][0] != null)
                    {
                        userADDetails.LastName = searchResult.Properties["sn"][0].ToString();
                    }
                    if (searchResult.Properties.Contains("mail") && searchResult.Properties["mail"][0] != null)
                    {
                        userADDetails.EmailId = searchResult.Properties["mail"][0].ToString();
                    }
                    if (searchResult.Properties.Contains("displayname") && searchResult.Properties["displayname"][0] != null)
                    {
                        userADDetails.UserDisplayName = searchResult.Properties["displayname"][0].ToString();
                    }
                    if (searchResult.Properties.Contains("userAccountControl") && searchResult.Properties["userAccountControl"][0] != null)
                    {
                        userADDetails.UserAccountControl = searchResult.Properties["userAccountControl"][0].ToString();
                    }
                    if (searchResult.Properties.Contains("lastLogonTimeStamp") && searchResult.Properties["lastLogonTimeStamp"][0] != null)
                    {
                        userADDetails.LastLogOn = Convert.ToInt64(searchResult.Properties["lastLogonTimeStamp"][0].ToString(), CultureInfo.InvariantCulture);
                    }
                    if (searchResult.Properties.Contains("title") && searchResult.Properties["title"][0] != null)
                    {
                        userADDetails.JobTitle = searchResult.Properties["title"][0].ToString();
                    }
                    if (searchResult.Properties.Contains("manager") && searchResult.Properties["manager"][0] != null)
                    {
                        userADDetails.Manager = searchResult.Properties["manager"][0].ToString();
                    }
                }
                result.Add(userADDetails);
            }

        }
    }
}
