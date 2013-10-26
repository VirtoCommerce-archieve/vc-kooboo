using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.Foundation.AppConfig;
using VirtoCommerce.Foundation.Assets;
using VirtoCommerce.Foundation.Catalogs;
using VirtoCommerce.Foundation.Customers;
using VirtoCommerce.Foundation.Data.Azure;
using VirtoCommerce.Foundation.Frameworks.Workflow;
using VirtoCommerce.Foundation.Importing;
using VirtoCommerce.Foundation.Inventories;
using VirtoCommerce.Foundation.Marketing;
using VirtoCommerce.Foundation.Marketing.DynamicContent;
using VirtoCommerce.Foundation.Orders;
using VirtoCommerce.Foundation.Reviews;
using VirtoCommerce.Foundation.Search;
using VirtoCommerce.Foundation.Security;
using VirtoCommerce.Foundation.Stores;
using VirtoCommerce.Web;

namespace Kooboo.VirtoCommerce
{
    using Kooboo.VirtoCommerce.Security;

    using Microsoft.Practices.Unity;

    using global::VirtoCommerce.Web.Client.Services.Security;

    public static class Bootstrapper
    {
        public static void Initialise()
        {
            SetupWebConfiguration();
            var oldResolver = DependencyResolver.Current;
            UnityWebActivator.Start();
            LocalInitializer();
            var dependencyResolver = new UnityKoobooResolver(DependencyResolver.Current, oldResolver);
            DependencyResolver.SetResolver(dependencyResolver);
            
        }

        private static void LocalInitializer()
        {
            var container = UnityConfig.GetConfiguredContainer();
            container.RegisterType<IUserSecurity, KBUserSecurity>();
        }

        public static void SetupWebConfiguration()
        {
            var mainCfg = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            var moduleConfigs = new Dictionary<string,ConfigurationSection>
                {{"AppConfig",new AppConfigConfiguration()},
                {"Asset",new AssetConfiguration()},
                {"Catalog",new CatalogConfiguration()},
                {"Customer",new CustomerConfiguration()},
                {"Import",new ImportConfiguration()},
                {"Inventory",new InventoryConfiguration()},
                {"Order",new OrderConfiguration()},
                {"Review",new ReviewConfiguration()},
                {"Search",new SearchConfiguration()},
                {"Store",new StoreConfiguration()},
                {"Security",new SecurityConfiguration()},
                {"Workflow",new WorkflowConfiguration()},
                {"Marketing",new MarketingConfiguration()},
                {"DynamicContent",new DynamicContentConfiguration()},
                {"AzureRepository",new AzureConfiguration()}};
            var mainSectionGroup = mainCfg.SectionGroups["VirtoCommerce"];
            var newsectionGroup = mainSectionGroup ?? new ConfigurationSectionGroup();
            if (mainSectionGroup == null)
            {
                mainCfg.SectionGroups.Add("VirtoCommerce",newsectionGroup);
            }
            var refreshSections = new List<string>();
            foreach (var configurationSection in moduleConfigs)
            {
                var existingSection = newsectionGroup.Sections[configurationSection.Key];
                if (existingSection != null && existingSection.ElementInformation.IsPresent)
                    continue;
                var cfgDoc = new ConfigXmlDocument();
                var configPath = HttpContext.Current.Server.MapPath(string.Format(@"Areas\{0}\Configuration\{1}.config", CommerceAreaRegistration.ModuleName, configurationSection.Key));
                cfgDoc.Load(configPath);
                if (cfgDoc.DocumentElement != null)
                {
                    if (existingSection == null)
                    {

                        configurationSection.Value.SectionInformation.SetRawXml(cfgDoc.DocumentElement.OuterXml);
                        newsectionGroup.Sections.Add(configurationSection.Key,
                        configurationSection.Value);
                    }
                    else
                    {

                        existingSection.SectionInformation.SetRawXml(cfgDoc.DocumentElement.OuterXml);
                    }
                }
                refreshSections.Add(configurationSection.Key);
            }
            if (mainCfg.AppSettings.Settings["DefaultStore"] == null)
            {
                mainCfg.AppSettings.Settings.Add("DefaultStore", "SampleStore");
            }
            mainCfg.Save();
            foreach (var sectionName in refreshSections)
            {
                ConfigurationManager.RefreshSection(sectionName);
            }
        }
    }
}
