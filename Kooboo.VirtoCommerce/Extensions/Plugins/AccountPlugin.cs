using System.ComponentModel;
using System.Web.Mvc;
using Kooboo.CMS.Sites.Extension;
using Kooboo.CMS.Sites.View;
using VirtoCommerce.Web.Controllers;

namespace Kooboo.VirtoCommerce.Extensions.Plugins
{
    [Description("Account plugin")]
    public class AccountPlugin : IHttpMethodPagePlugin
    {
        public ActionResult HttpGet(Page_Context context, PagePositionContext positionContext)
        {
            var accountController = (AccountController)DependencyResolver.Current.GetService(typeof(AccountController));

            if (accountController != null)
            {
                var result = accountController.Index() as ViewResultBase;

                if (result != null)
                {
                    context.ControllerContext.Controller.ViewBag.CustomerModel = result.Model;
                }

                result = accountController.RecentOrders() as ViewResultBase;

                if (result != null)
                {
                    context.ControllerContext.Controller.ViewBag.RecentOrders = result.Model;
                }

                result = accountController.AccountInfo() as ViewResultBase;

                if (result != null)
                {
                    context.ControllerContext.Controller.ViewBag.AccountInfo = result.Model;
                }
            }

            return null;
        }

        public ActionResult HttpPost(Page_Context context, PagePositionContext positionContext)
        {
            return null;
        }
    }
}
