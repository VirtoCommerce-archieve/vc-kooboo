using System;
using System.Linq;

namespace Kooboo.VirtoCommerce.Extensions
{
    using System.Globalization;
    using System.Web.Mvc;

    using Kooboo.CMS.Sites.Extension.ModuleArea;
    using Kooboo.CMS.Sites.Models;
    using Kooboo.CMS.Sites.View;
    using Kooboo.CMS.Sites.Web;

    public abstract class SiteViewEngine : BuildManagerViewEngine
    {
        protected SiteViewEngine() { }

        protected SiteViewEngine(IViewPageActivator viewPageActivator) : base(viewPageActivator) { }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            Site site = GetSiteFromContext(controllerContext);
            if(site == null)
            {
                return new ViewEngineResult(new[] { "" });
            }

            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException("Value is required.", "viewName");
            }

            string[] searchedViewLocations;
            string[] searchedMasterLocations;

            var controllerName = controllerContext.RouteData.GetRequiredString("controller");

            var viewPath = GetPath(controllerContext, site, ViewLocationFormats, viewName, controllerName, out searchedViewLocations);
            var masterPath = GetPath(controllerContext, site, MasterLocationFormats, masterName, controllerName, out searchedMasterLocations);

            if (!string.IsNullOrEmpty(viewPath) && (masterPath != string.Empty || string.IsNullOrEmpty(masterName)))
            {
                return new ViewEngineResult(CreateView(controllerContext, viewPath, masterPath), this);
            }
            return new ViewEngineResult(searchedViewLocations.Union(searchedMasterLocations));
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            
            var site = GetSiteFromContext(controllerContext);
            if (site == null)
            {
                return new ViewEngineResult(new[] { "" });
            }

            if (string.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentException("Value is required.", partialViewName);
            }


            string[] searchedLocations;

            var controllerName = controllerContext.RouteData.GetRequiredString("controller");

            var partialPath = GetPath(controllerContext, site, PartialViewLocationFormats, partialViewName, controllerName, out searchedLocations);

            return string.IsNullOrEmpty(partialPath) ? 
                new ViewEngineResult(searchedLocations) : 
                new ViewEngineResult(CreatePartialView(controllerContext, partialPath), this);
        }

        protected virtual string GetPath(
            ControllerContext controllerContext,
            Site site,
            string[] locations,
            string viewName,
            string controllerName,
            out string[] searchedLocations)
        {
            searchedLocations = new string[locations.Length];

            var cacheKey = CreateCacheKey("FrontWrapper", site.Name, viewName, controllerName);
            string path = ViewLocationCache.GetViewLocation(controllerContext.HttpContext, cacheKey);
            if (path != null)
            {
                return path;
            }

            var baseVirtualPath = site.VirtualPath;
            for (var i = 0; i < locations.Length; i++)
            {
                path = string.Format(
                    CultureInfo.InvariantCulture,
                    locations[i],
                    new object[] { viewName, controllerName, baseVirtualPath });
                if (VirtualPathProvider.FileExists(path))
                {
                    searchedLocations = new string[0];
                    ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, path);
                    return path;
                }
                searchedLocations[i] = path;
            }

            return null;
        }

        private string CreateCacheKey(string prefix, string moduleName, string name, string controllerName)
        {
            return string.Format(CultureInfo.InvariantCulture, ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}", new object[] { GetType().AssemblyQualifiedName, prefix, moduleName, name, controllerName });
        }

        private Site GetSiteFromContext(ControllerContext controllerContext)
        {
            if (controllerContext.RequestContext.HttpContext is FrontHttpContextWrapper)
            {
                var context = controllerContext.RequestContext.HttpContext as FrontHttpContextWrapper;
                return context.RequestWrapper.Site;
            }

            if ((controllerContext.RequestContext is ModuleRequestContext 
                || controllerContext.RequestContext.HttpContext is ModuleHttpContext) 
                && Page_Context.Current.PageRequestContext != null 
                && Page_Context.Current.PageRequestContext.Site != null)
            {
                return Page_Context.Current.PageRequestContext.Site;
            }

            return null;
        }
    }

    public class SiteRazorViewEngine : SiteViewEngine
    {
        public SiteRazorViewEngine() : this(null) { }
        public SiteRazorViewEngine(IViewPageActivator viewPageActivator)
            : base(viewPageActivator)
        {
            ViewLocationFormats = new[] {
                "{2}/Templates/Views/Proxy.{1}.{0}/template.cshtml",
                "{2}/Templates/Views/Proxy.{0}/template.cshtml",
                "{2}/Templates/Views/Proxy.Shared.{0}/template.cshtml"
            };

            MasterLocationFormats = new[] {
                "{2}/Templates/Views/{1}{0}/template.cshtml"
            };

            PartialViewLocationFormats = new[] {
                "{2}/Templates/Views/Proxy.{1}.{0}/template.cshtml",
                "{2}/Templates/Views/Proxy.Shared.{0}/template.cshtml"
            };
            FileExtensions = new[] { "cshtml" };
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            return new RazorView(controllerContext, partialPath, null, false, FileExtensions, ViewPageActivator);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            return new RazorView(controllerContext, viewPath, masterPath, true, FileExtensions, ViewPageActivator);
        }
    }
}
