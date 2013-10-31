using Kooboo.CMS.Common;
using Kooboo.CMS.Membership.Services;
using Kooboo.CMS.Sites.Extension;
using Kooboo.CMS.Sites.Membership;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.View;
using Kooboo.VirtoCommerce.Model;
using System.Collections.Generic;
using System.Web.Mvc;
using Omu.ValueInjecter;
using VirtoCommerce.Foundation.Customers;
using VirtoCommerce.Web.Client.Helpers;

namespace Kooboo.VirtoCommerce.Extensions.Plugins
{
    public class ExternalRegistrationConfirmationPlugin : ISubmissionPlugin, IHttpMethodPagePlugin
    {
        private readonly MembershipUserManager _manager;

        public ExternalRegistrationConfirmationPlugin(MembershipUserManager manager)
        {
            _manager = manager;
        }

        #region ISubmissionPlugin

        public ActionResult Submit(Site site, ControllerContext controllerContext, SubmissionSetting submissionSetting)
        {
            var resultData = new JsonResultData();
            string redirectUrl;
            if (!RegisterCore(controllerContext, submissionSetting, out redirectUrl))
            {
                resultData.AddModelState(controllerContext.Controller.ViewData.ModelState);
                resultData.Success = false;
            }
            else
            {
                resultData.RedirectUrl = redirectUrl;
                resultData.Success = true;
            }
            return new JsonResult() { Data = resultData };

        }

        public Dictionary<string, object> Parameters
        {
            get
            {
                return new Dictionary<string, object> {
                    { "UserName", "{UserName}" },
                    {"RedirectUrl", "~/Member"}
                };
            }
        }
        #endregion

        protected virtual bool RegisterCore(ControllerContext controllerContext, SubmissionSetting submissionSetting, out string redirectUrl)
        {
            redirectUrl = string.Empty;
            var model = new VirtoExternalLoginModel();

            if (!ModelBindHelper.BindModel(model, "", controllerContext, submissionSetting))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(model.RedirectUrl))
            {
                redirectUrl = MemberPluginHelper.ResolveSiteUrl(controllerContext, model.RedirectUrl);
            }
            var user = StoreHelper.UserClient.GetAccountByUserName(model.Username.ToLower());
            var membershipUser = controllerContext.HttpContext.Membership().GetMembershipUser();

            //Such account is already created in virto
            if (membershipUser != null)
            {
                if (user != null)
                {
                    return true;
                }

                var registerMemberModel = new VirtoRegisterMemberModel();
                registerMemberModel.InjectFrom(membershipUser);

                if (membershipUser.ProviderExtraData != null && membershipUser.ProviderExtraData.ContainsKey("fullName"))
                {
                    registerMemberModel.FirstName = membershipUser.ProviderExtraData["fullName"];
                }
                else
                {
                    registerMemberModel.FirstName = model.Username;
                }

                membershipUser.Profiles = membershipUser.Profiles ?? new Dictionary<string, string>();

                if (!membershipUser.Profiles.ContainsKey("FirstName") && !string.IsNullOrEmpty(registerMemberModel.FirstName))
                {
                    membershipUser.Profiles.Add("FirstName",registerMemberModel.FirstName);
                }

                if (!membershipUser.Profiles.ContainsKey("LastName") && !string.IsNullOrEmpty(registerMemberModel.LastName))
                {
                    membershipUser.Profiles.Add("LastName", registerMemberModel.FirstName);
                }

                if (VirtoRegisterMemberPlugin.RegisterVirtoUser(_manager, StoreHelper.UserClient, registerMemberModel))
                {
                    return true;
                }
            }

            return false;
        }

        public ActionResult HttpGet(Page_Context context, PagePositionContext positionContext)
        {
            var membershipUser = context.ControllerContext.HttpContext.Membership().GetMembershipUser();
            if (membershipUser != null)
            {
                var account = StoreHelper.UserClient.GetAccountByUserName(membershipUser.UserName);

                //Account exist, no need to do anything
                if (account != null)
                {
                    var model = new VirtoExternalLoginModel();
                    var redirectUrl = "~/";

                    if (ModelBindHelper.BindModel(model, "", context.ControllerContext, null))
                    {
                        if (!string.IsNullOrEmpty(model.RedirectUrl))
                        {
                            redirectUrl = model.RedirectUrl;
                        }
                    }

                    return new RedirectResult(MemberPluginHelper.ResolveSiteUrl(context.ControllerContext, redirectUrl));

                }
            }

            return null;
        }

        public ActionResult HttpPost(Page_Context context, PagePositionContext positionContext)
        {
            System.Web.Helpers.AntiForgery.Validate();
            string redirectUrl;
            context.ControllerContext.Controller.ViewBag.MembershipSuccess = RegisterCore(context.ControllerContext, null, out redirectUrl);
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                return new RedirectResult(redirectUrl);
            }
            return null;
        }
    }
}
