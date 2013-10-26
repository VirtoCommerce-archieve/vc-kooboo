using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Kooboo.CMS.Sites.Extension.ModuleArea;

namespace Kooboo.VirtoCommerce
{

    public class ProxyHtmlHelper<TModel> : HtmlHelper<TModel>
    {
        private readonly ModuleHtmlHelper<TModel> _moduleHtmlHelper;

        public ProxyHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer) :
            this(viewContext, viewDataContainer, RouteTable.Routes)
        {
        }

        public ProxyHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection) : 
            base(viewContext, viewDataContainer, routeCollection)
        {
            var moduleRequestContext = viewContext.RequestContext as ModuleRequestContext;
            if (moduleRequestContext != null)
            {
                _moduleHtmlHelper = new ModuleHtmlHelper<TModel>(viewContext, viewDataContainer,
                                                                 moduleRequestContext.ModuleContext.RouteTable);
            }
        }

        public MvcHtmlString Action(string actionName)
        {
            return Action(actionName, null, null);
        }

        public MvcHtmlString Action(string actionName, object routeValues)
        {
            return Action(actionName, null, new RouteValueDictionary(routeValues));
        }

        public MvcHtmlString Action(string actionName, RouteValueDictionary routeValues)
        {
            return Action(actionName, null, routeValues);
        }

        public MvcHtmlString Action(string actionName, string controllerName)
        {
            return Action(actionName, controllerName, null);
        }

        public MvcHtmlString Action(string actionName, string controllerName, object routeValues)
        {
            return Action(actionName, controllerName, new RouteValueDictionary(routeValues));
        }

        public MvcHtmlString Action(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            return _moduleHtmlHelper == null ? ChildActionExtensions.Action(this, actionName, controllerName, routeValues) : 
                _moduleHtmlHelper.Action(actionName, controllerName, routeValues);
        }

        public void RenderAction(string actionName)
        {
            RenderAction(actionName, null, null);
        }

        public void RenderAction(string actionName, object routeValues)
        {
            RenderAction(actionName, null, new RouteValueDictionary(routeValues));
        }

        public void RenderAction(string actionName, RouteValueDictionary routeValues)
        {
            RenderAction(actionName, null, routeValues);
        }

        public void RenderAction(string actionName, string controllerName)
        {
            RenderAction(actionName, controllerName, null);
        }

        public void RenderAction(string actionName, string controllerName, object routeValues)
        {
            RenderAction(actionName, controllerName, new RouteValueDictionary(routeValues));
        }

        public void RenderAction(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            if (_moduleHtmlHelper == null)
            {
                ChildActionExtensions.RenderAction(this, actionName, controllerName, routeValues);
            }
            else
            {
                _moduleHtmlHelper.RenderAction(actionName, controllerName, routeValues);
            }
        }

        public MvcForm BeginForm()
        {
// ReSharper disable Mvc.ActionNotResolved
            return _moduleHtmlHelper == null ? FormExtensions.BeginForm(this) : _moduleHtmlHelper.BeginForm();
// ReSharper restore Mvc.ActionNotResolved
        }


        public MvcForm BeginForm(object routeValues)
        {
            return BeginForm(null, null, new RouteValueDictionary(routeValues), FormMethod.Post, new RouteValueDictionary());
        }

        public MvcForm BeginForm(RouteValueDictionary routeValues)
        {
            return BeginForm(null, null, routeValues, FormMethod.Post, new RouteValueDictionary());
        }

        public MvcForm BeginForm(string actionName, string controllerName)
        {
            return BeginForm(actionName, controllerName, new RouteValueDictionary(), FormMethod.Post, new RouteValueDictionary());
        }

        public MvcForm BeginForm(string actionName, string controllerName, object routeValues)
        {
            return BeginForm(actionName, controllerName, new RouteValueDictionary(routeValues), FormMethod.Post, new RouteValueDictionary());
        }

        public MvcForm BeginForm(string actionName, string controllerName, FormMethod method)
        {
            return BeginForm(actionName, controllerName, new RouteValueDictionary(), method, new RouteValueDictionary());
        }

        public MvcForm BeginForm(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            return BeginForm(actionName, controllerName, routeValues, FormMethod.Post, new RouteValueDictionary());
        }

        public MvcForm BeginForm(string actionName, string controllerName, object routeValues, FormMethod method)
        {
            return BeginForm(actionName, controllerName, new RouteValueDictionary(routeValues), method, new RouteValueDictionary());
        }

        public MvcForm BeginForm(string actionName, string controllerName, FormMethod method, IDictionary<string, object> htmlAttributes)
        {
            return BeginForm(actionName, controllerName, new RouteValueDictionary(), method, htmlAttributes);
        }

        public MvcForm BeginForm(string actionName, string controllerName, FormMethod method, object htmlAttributes)
        {
            return BeginForm(actionName, controllerName, new RouteValueDictionary(), method, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public MvcForm BeginForm(string actionName, string controllerName, RouteValueDictionary routeValues, FormMethod method)
        {
            return BeginForm(actionName, controllerName, routeValues, method, new RouteValueDictionary());
        }

        public MvcForm BeginForm(string actionName, string controllerName, object routeValues, FormMethod method, object htmlAttributes)
        {
            return BeginForm(actionName, controllerName, new RouteValueDictionary(routeValues), method, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public MvcForm BeginForm(string actionName, string controllerName, RouteValueDictionary routeValues, FormMethod method, IDictionary<string, object> htmlAttributes)
        {
            return _moduleHtmlHelper == null ? 
                FormExtensions.BeginForm(this, actionName, controllerName, routeValues, method, htmlAttributes) : 
                _moduleHtmlHelper.BeginForm(actionName, controllerName, routeValues, method, htmlAttributes);
        }
    }

    public abstract class KoobooBasePage<TModel> : WebViewPage<TModel>
    {
        public HtmlHelper<TModel> MvcHtml { get; set; }
        public AjaxHelper<TModel> MvcAjax { get; set; }
        public UrlHelper MvcUrl { get; set; }

        public new ProxyHtmlHelper<TModel> Html { get; set; }
        public new AjaxHelper<TModel> Ajax { get; set; }
        public new UrlHelper Url { get; set; }

        public override void InitHelpers()
        {
            base.InitHelpers();

            MvcAjax = base.Ajax;
            MvcHtml = base.Html;
            MvcUrl = base.Url;

            Html = new ProxyHtmlHelper<TModel>(ViewContext, this);
            Ajax = IsModuleContext(MvcAjax.ViewContext.RequestContext) ? MvcAjax.ModuleAjax() : base.Ajax;
            Url = IsModuleContext(MvcUrl.RequestContext) ? MvcUrl.ModuleUrl() : base.Url;
        }

        private bool IsModuleContext(RequestContext requestContext)
        {
            return requestContext is ModuleRequestContext;
        }
    }
}
