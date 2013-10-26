using System.Web.Http;
using Kooboo.Web.Mvc;
using System.IO;
using System.Web.Mvc;

namespace Kooboo.VirtoCommerce
{
    public class CommerceAreaRegistration : AreaRegistration
    {
        public const string ModuleName = "VirtoCommerce";
        public override string AreaName
        {
            get
            {
                return ModuleName;
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
			WebApiConfig.Register(GlobalConfiguration.Configuration);
            context.MapRoute(
                "Commerce_default",
                "Commerce/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                null, 
                new[] {"VirtoCommerce.Web.Controllers","Kooboo.VirtoCommerce.Controllers" }
            );

            context.MapRoute(
                "Assets", 
                "asset/{*path}",
                new { controller = "Asset", action = "Index", path = UrlParameter.Optional }, 
                null, 
                new[] {"VirtoCommerce.Web.Controllers" });

            /*
            context.MapRoute(
                "Item",
                "p/{url}",
                new { controller = "Transfer", action = "RedirectItem" },
                null,
                new[] { "Kooboo.VirtoCommerce.Controllers" }
            );

            context.MapRoute(
                "Catalog",
                "c/{url}",
                  new { controller = "Transfer", action = "RedirectCatalog" },
                null,
                new[] { "Kooboo.VirtoCommerce.Controllers" }
            );
             * */


            var menuFile = AreaHelpers.CombineAreaFilePhysicalPath(AreaName, "Menu.config");
            if (File.Exists(menuFile))
            {
                Kooboo.Web.Mvc.Menu.MenuFactory.RegisterAreaMenu(AreaName, menuFile);
            }
            var resourceFile = Path.Combine(Settings.BaseDirectory, "Areas", AreaName, "WebResources.config");
            if (File.Exists(resourceFile))
            {
                Kooboo.Web.Mvc.WebResourceLoader.ConfigurationManager.RegisterSection(AreaName, resourceFile);
            }

            Bootstrapper.Initialise();
        }
    }
}
