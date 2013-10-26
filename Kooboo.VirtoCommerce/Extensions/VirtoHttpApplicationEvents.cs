using System.Linq;
using Kooboo.CMS.Common;
using Kooboo.CMS.Common.Runtime.Dependency;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.Web.Client.Modules;
using VirtoCommerce.Web.Models;
using VirtoCommerce.Web.Models.Binders;

namespace Kooboo.VirtoCommerce.Extensions
{
    using Kooboo.CMS.Sites.Membership;

    [Dependency(typeof(IHttpApplicationEvents), Key ="VirtoHttpApplicationEvents")]
    public class VirtoHttpApplicationEvents : HttpApplicationEvents
    {
        private static readonly IHttpModule[] _modules = new[] { new KoobooStoreHttpModule(), (IHttpModule)new MarketingHttpModule() };
        public override void Init(HttpApplication httpApplication)
        {
            _modules.ForEach((m, i) => m.Init(httpApplication));
            ModelBinders.Binders[typeof(SearchParameters)] = new SearchParametersBinder();
        }

		public override void Application_Start(object sender, System.EventArgs e)
		{
			base.Application_Start(sender, e);
            ViewEngines.Engines.Insert(ViewEngines.Engines.Count, new SiteRazorViewEngine());
		}
    }

    public class KoobooStoreHttpModule : StoreHttpModule
    {
        protected override bool IsRequestAuthenticated(HttpContext context)
        {
            return context.Request.RequestContext.HttpContext.Membership().GetMember().Identity.IsAuthenticated;
        }

        protected override string GetRequestUserName(HttpContext context)
        {
            return context.Request.RequestContext.HttpContext.Membership().GetMember().Identity.Name;
        }

        protected override void OnAuthenticateRequest(object sender, System.EventArgs e)
        {
            //base.OnAuthenticateRequest(sender, e);
            //do nothing here because kooboo membership is not ready
        }

        protected override void OnPostAcquireRequestState(object sender, System.EventArgs e)
        {
            base.OnAuthenticateRequest(sender, e);
            base.OnPostAcquireRequestState(sender, e);
        }
    }
}