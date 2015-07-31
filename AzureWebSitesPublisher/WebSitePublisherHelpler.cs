using Microsoft.Web.Deployment;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
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

    public static class WebSitePublisherHelpler
    {
        public static DeploymentChangeSummary Publish(string publishSettingsPath, string sourcePath, string parametersPath)
        {
            if (String.IsNullOrEmpty(publishSettingsPath)) throw new ArgumentNullException("publishSettingsPath");
            if (String.IsNullOrEmpty(sourcePath)) throw new ArgumentNullException("sourcePath");

            if (!File.Exists(publishSettingsPath))
            {
                throw new Exception(String.Format("{0}: Not found.", publishSettingsPath));
            }
            if (!File.Exists(sourcePath))
            {
                throw new Exception(String.Format("{0}: Not found.", sourcePath));
            }
            else if (Path.GetExtension(sourcePath).Equals(".zip", StringComparison.InvariantCultureIgnoreCase) == false)
            {
                throw new Exception("Extension supports only zip.");
            }

            // PublishSettings
            var document = XElement.Load(publishSettingsPath);
            var profile = document.XPathSelectElement("//publishProfile[@publishMethod='MSDeploy']");
            if (profile == null)
            {
                throw new Exception(String.Format("{0}: Not a valid publishing profile.", publishSettingsPath));
            }
            var publishUrl = profile.SafeGetAttribute("publishUrl");
            var userName = profile.SafeGetAttribute("userName");
            var password = profile.SafeGetAttribute("userPWD");
            var siteName = profile.SafeGetAttribute("msdeploySite");
            var webDeployServer = string.Format(@"https://{0}/msdeploy.axd?site={1}", publishUrl, siteName);

            // Set up deployment
            var destinationOptions = new DeploymentBaseOptions
            {
                ComputerName = webDeployServer,
                UserName = userName,
                Password = password,
                AuthenticationType = "basic",
                IncludeAcls = true,
                TraceLevel = TraceLevel.Info
            };
            destinationOptions.Trace += (sender, e) =>
            {
                Trace.TraceInformation(e.Message);
            };
            var syncOptions = new DeploymentSyncOptions { DoNotDelete = true };  // Please change as you want

            DeploymentChangeSummary result;
            try
            {
                // Start deployment
                using (var deploy = DeploymentManager.CreateObject(DeploymentWellKnownProvider.Package, sourcePath, new DeploymentBaseOptions()))
                {
                    // Apply package parameters
                    foreach (var p in deploy.SyncParameters)
                    {
                        switch (p.Name)
                        {
                            case "IIS Web Application Name":
                                p.Value = siteName;
                                break;
                            default:
                                // SetParameters.xml
                                if (!String.IsNullOrEmpty(parametersPath))
                                {
                                    var parameters = XElement.Load(parametersPath);
                                    var setParameter = parameters.XPathSelectElement(String.Format("//setParameter[@name='{0}']", p.Name));
                                    if (setParameter != null)
                                    {
                                        p.Value = setParameter.SafeGetAttribute("value");
                                    }
                                }
                                break;
                        }
                    }
                    result = deploy.SyncTo(DeploymentWellKnownProvider.Auto, siteName, destinationOptions, syncOptions);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

    }
}
