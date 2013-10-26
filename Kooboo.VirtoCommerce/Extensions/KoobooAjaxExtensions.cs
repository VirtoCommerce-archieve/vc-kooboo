using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace Kooboo.VirtoCommerce.Extensions
{
    public static class KoobooAjaxExtensions
    {
        public static MvcHtmlString ActionLinkEx(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes)
        {
            var retValue =  ajaxHelper.ActionLink(linkText, actionName, controllerName, routeValues, ajaxOptions, htmlAttributes);
            return new MvcHtmlString(retValue.ToHtmlString().Replace("~~", "?"));
        }
    }
}
