using System.ComponentModel;
using System.Web.Mvc;
using Kooboo.CMS.Sites.Extension;
using Kooboo.CMS.Sites.View;
using VirtoCommerce.Web.Controllers;

namespace Kooboo.VirtoCommerce.Extensions.Plugins
{
    [Description("WishlistPlugin plugin")]
    public class WishlistPlugin : IHttpMethodPagePlugin
    {
        public ActionResult HttpGet(Page_Context context, PagePositionContext positionContext)
        {
            var accountController = (AccountController)DependencyResolver.Current.GetService(typeof(AccountController));

            if (accountController != null)
            {
                var result = accountController.WishList() as ViewResultBase;

                if (result != null)
                {
                    context.ControllerContext.Controller.ViewBag.WishList = result.Model;
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
