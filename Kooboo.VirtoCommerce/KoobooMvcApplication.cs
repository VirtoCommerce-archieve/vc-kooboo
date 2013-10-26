
using System;
using System.Linq;
using VirtoCommerce.Web.Client.Helpers;
using VirtoCommerce.Web.Virto.Helpers;

namespace Kooboo.VirtoCommerce
{
    public class MvcApplication : Kooboo.CMS.Web.MvcApplication
    {
        public override string GetVaryByCustomString(System.Web.HttpContext context, string custom)
        {
            var varyString = base.GetVaryByCustomString(context, custom) ?? string.Empty;

            varyString += UserHelper.CustomerSession.Language;//allways vary by language

            if (SettingsHelper.OutputCacheEnabled)
            {
                foreach (var key in custom.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (string.Equals(key, "store", StringComparison.OrdinalIgnoreCase))
                    {
                        varyString += UserHelper.CustomerSession.StoreId;
                    }
                    if (string.Equals(key, "currency", StringComparison.OrdinalIgnoreCase))
                    {
                        varyString += UserHelper.CustomerSession.Currency;
                    }
                    if (string.Equals(key, "cart", StringComparison.OrdinalIgnoreCase))
                    {
                        //This method is called from System.Web.Caching module before customerId set 
                        if (string.IsNullOrEmpty(UserHelper.CustomerSession.CustomerId))
                        {
                            if (UserHelper.CustomerSession.IsRegistered)
                            {
                                var account = UserHelper.UserClient.GetAccountByUserName(context.User.Identity.Name);
                                if (account != null)
                                {
                                    UserHelper.CustomerSession.CustomerId = account.MemberId;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(UserHelper.CustomerSession.CustomerId))
                        {
                            UserHelper.CustomerSession.CustomerId = context.Request.AnonymousID ??
                                                                    Guid.NewGuid().ToString();
                        }

                        var ch = new CartHelper(CartHelper.CartName);

                        if (ch.LineItems.Any())
                        {
                            varyString +=
                                new CartHelper(CartHelper.CartName).LineItems.Select(x => x.LineItemId)
                                                                   .Aggregate((x, y) => x + y);
                        }

                    }
                }
            }

            return varyString;
        }
    }
}
