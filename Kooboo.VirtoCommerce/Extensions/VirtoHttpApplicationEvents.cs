using Kooboo.CMS.Common;
using Kooboo.CMS.Common.Persistence.Non_Relational;
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Sites.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.Client;
using VirtoCommerce.Foundation.Stores.Model;
using VirtoCommerce.Web.Client.Helpers;
using VirtoCommerce.Web.Client.Modules;
using VirtoCommerce.Web.Models;
using VirtoCommerce.Web.Models.Binders;
using DependencyResolver = System.Web.Mvc.DependencyResolver;

namespace Kooboo.VirtoCommerce.Extensions
{
    using CMS.Sites.Membership;

    [Dependency(typeof(IHttpApplicationEvents), Key = "VirtoHttpApplicationEvents")]
    public class VirtoHttpApplicationEvents : HttpApplicationEvents
    {
        private static readonly IHttpModule[] Modules = new[] { new KoobooStoreHttpModule(), (IHttpModule)new MarketingHttpModule() };
        public override void Init(HttpApplication httpApplication)
        {
            Modules.ForEach((m, i) => m.Init(httpApplication));
            ModelBinders.Binders[typeof(SearchParameters)] = new SearchParametersBinder();
        }

        public override void Application_Start(object sender, EventArgs e)
        {
            base.Application_Start(sender, e);
            ViewEngines.Engines.Insert(ViewEngines.Engines.Count, new SiteRazorViewEngine());
        }
    }

    public class KoobooStoreHttpModule : StoreHttpModule
    {
        protected override string StoreCookie
        {
            get { return "vcf.kooboo.store"; }
        }

        protected override string CurrencyCookie
        {
            get { return "vcf.kooboo.currency"; }
        }

        protected override bool IsRequestAuthenticated(HttpContext context)
        {
            return context.Request.RequestContext.HttpContext.Membership().GetMember().Identity.IsAuthenticated;
        }

        protected override string GetRequestUserName(HttpContext context)
        {
            return context.Request.RequestContext.HttpContext.Membership().GetMember().Identity.Name;
        }

        protected override void OnAuthenticateRequest(object sender, EventArgs e)
        {
            //base.OnAuthenticateRequest(sender, e);
            //do nothing here because kooboo membership is not ready
        }

        protected override void OnAuthenticateRequest(HttpContext context)
        {
            base.OnAuthenticateRequest(context);

            var user = context.Request.RequestContext.HttpContext.Membership().GetMembershipUser();

            if (user != null && user.Profiles != null && 
                (user.Profiles.ContainsKey("FirstName") || user.Profiles.ContainsKey("LastName")))
            {
                CustomerSession.CustomerName = string.Format("{0} {1}",
                                                             user.Profiles.ContainsKey("FirstName")
                                                                 ? user.Profiles["FirstName"]
                                                                 : "",
                                                             user.Profiles.ContainsKey("LastName")
                                                                 ? user.Profiles["LastName"]
                                                                 : "");
            }
        }

        protected override void OnPostAcquireRequestState(object sender, EventArgs e)
        {
            base.OnAuthenticateRequest(sender, e);
            base.OnPostAcquireRequestState(sender, e);
        }

        protected override void OnUnauthorized(HttpContext context)
        {
            var account = StoreHelper.UserClient.GetAccountByUserName(CustomerSession.Username);
            //Logout only if not external login confirmation page
            if (account != null)
            {
                context.Request.RequestContext.HttpContext.Membership().SignOut();
            }
        }

        protected override Store GetStore(HttpContext context)
        {
            // try getting store from the cookie
            var storeid = StoreHelper.GetCookieValue(StoreCookie, false);
            var storeClient = DependencyResolver.Current.GetService<StoreClient>();
            Store store = null;

            // try getting default store from settings
            if (String.IsNullOrEmpty(storeid))
            {
                if (Site.Current == null)
                {
                    //manually load site (TODO: hardcoded VirtoCommerce!!!)
                    Site.Current = (SiteHelper.Parse("VirtoCommerce")).AsActual();
                }

                if (Site.Current != null)
                {
                    storeid = Site.Current.CustomFields["VCStoreId"];

                    if (String.IsNullOrEmpty(storeid))
                    {
                        storeid = ConfigurationManager.AppSettings["DefaultStore"];
                    }
                }
            }

            if (!String.IsNullOrEmpty(storeid))
            {
                store = storeClient.GetStoreById(storeid);

                if (store != null)
                {
                    //Closed store is not available
                    if (store.StoreState == (int)StoreState.Closed)
                    {
                        store = null;
                    }
                }
            }

            return store;
        }
    }
}