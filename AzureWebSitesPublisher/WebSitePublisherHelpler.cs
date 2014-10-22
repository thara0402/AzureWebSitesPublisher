using Microsoft.Web.Deployment;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AzureWebSitesPublisher
{
   public static class MethodExtention
    {
        public static string SafeGetAttribute(this XElement node, string attribute, string defaultValue = null)
        {
            var attr = node.Attribute(attribute);
            return attr == null ? defaultValue : attr.Value;
        }
    }

    public enum ContentType
    {
        Pacakge,
        Folder,
        File
    }

    public static class WebSitePublisherHelpler
    {
        public static DeploymentChangeSummary Publish(string publishSettingsPath, string sourcePath)
        {
            if (String.IsNullOrEmpty(publishSettingsPath)) throw new ArgumentNullException("publishSettingsPath");
            if (String.IsNullOrEmpty(sourcePath)) throw new ArgumentNullException("sourcePath");

            var contentType = ContentType.File;

            if (!File.Exists(publishSettingsPath))
            {
                throw new Exception(String.Format("{0}: Not found.", publishSettingsPath));
            }
            if (Directory.Exists(sourcePath))
            {
                contentType = ContentType.Folder;
            }
            else if (!File.Exists(sourcePath))
            {
                throw new Exception(String.Format("{0}: Not found.", sourcePath));
            }
            else if (Path.GetExtension(sourcePath).Equals(".zip", StringComparison.InvariantCultureIgnoreCase))
            {
                contentType = ContentType.Pacakge;
            }

            var document = XElement.Load(publishSettingsPath);
            var profile = document.XPathSelectElement("//publishProfile[@publishMethod='MSDeploy']");
            if (profile == null)
            {
                throw new Exception(String.Format("{0}: Not a valid publishing profile.", publishSettingsPath));
            }

            var publishUrl = profile.SafeGetAttribute("publishUrl");
            var destinationAppUrl = profile.SafeGetAttribute("destinationAppUrl");
            var userName = profile.SafeGetAttribute("userName");
            var password = profile.SafeGetAttribute("userPWD");
            var siteName = profile.SafeGetAttribute("msdeploySite");

            // Database related attributes
            var databaseInfo = new SqlConnectionStringBuilder();
            var databaseSection = profile.XPathSelectElements("./databases/add").FirstOrDefault();
            if (databaseSection != null)
            {
                databaseInfo.ConnectionString = databaseSection.SafeGetAttribute("connectionString");
            }
            var webDeployServer = string.Format(@"https://{0}/msdeploy.axd?site={1}", publishUrl, siteName);

            // Set up deployment
            var sourceProvider = contentType == ContentType.Pacakge ? DeploymentWellKnownProvider.Package : DeploymentWellKnownProvider.ContentPath;
            var destinationProvider = contentType == ContentType.Pacakge ? DeploymentWellKnownProvider.Auto : DeploymentWellKnownProvider.ContentPath;

            var sourceOptions = new DeploymentBaseOptions();
            var destinationOptions = new DeploymentBaseOptions
            {
                ComputerName = webDeployServer,
                UserName = userName,
                Password = password,
                AuthenticationType = "basic",
                IncludeAcls = true,
                TraceLevel = System.Diagnostics.TraceLevel.Info
            };
//            destinationOptions.Trace += (sender, e) => Console.WriteLine(e.Message);

            var destinationPath = siteName;
            if (contentType == ContentType.File)
            {
                var filename = new FileInfo(sourcePath).Name;
                destinationPath += "/" + filename;
            }

            var syncOptions = new DeploymentSyncOptions { DoNotDelete = true };  // Please change as you want

            // Start deployment
            using (var deploy = DeploymentManager.CreateObject(sourceProvider, sourcePath, sourceOptions))
            {
                // Apply package parameters
                foreach (var p in deploy.SyncParameters)
                {
                    switch (p.Name)
                    {
                        case "IIS Web Application Name":
                        case "AppPath":
                            p.Value = siteName;
                            break;
                        case "DbServer":
                            p.Value = databaseInfo.DataSource;
                            break;
                        case "DbName":
                            p.Value = databaseInfo.InitialCatalog;
                            break;
                        case "DbUsername":
                        case "DbAdminUsername":
                            p.Value = databaseInfo.UserID;
                            break;
                        case "DbPassword":
                        case "DbAdminPassword":
                            p.Value = databaseInfo.Password;
                            break;
                    }
                }

                return deploy.SyncTo(destinationProvider, destinationPath, destinationOptions, syncOptions);
            }
        }
    }
}
