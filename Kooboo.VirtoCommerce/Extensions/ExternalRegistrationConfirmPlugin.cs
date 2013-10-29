using System.Web;
using Kooboo.CMS.Common;
using Kooboo.CMS.Common.DataViolation;
using Kooboo.CMS.Membership.Models;
using Kooboo.CMS.Membership.Services;
using Kooboo.CMS.Sites.Extension;
using Kooboo.CMS.Sites.Membership;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.View;
using Kooboo.Globalization;
using Kooboo.VirtoCommerce.Model;
using Kooboo.Web.Url;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Omu.ValueInjecter;
using VirtoCommerce.Client;
using DependencyResolver = System.Web.Mvc.DependencyResolver;

namespace Kooboo.VirtoCommerce.Extensions
{
    public class ExternalRegistrationConfirmPlugin : ISubmissionPlugin
    {
        private readonly MembershipUserManager _manager;
        private readonly UserClient _userClient = DependencyResolver.Current.GetService<UserClient>();

        public ExternalRegistrationConfirmPlugin(MembershipUserManager manager)
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
            var user = _userClient.GetAccountByUserName(model.Username.ToLower());
            var membershipUser = controllerContext.HttpContext.Membership().GetMembershipUser();

            //Such account is already created in virto
            if (membershipUser != null)
            {

                membershipUser.Profiles = membershipUser.Profiles ?? new Dictionary<string, string>();

                if (user != null)
                {

                    if (!membershipUser.Profiles.ContainsKey("IsVirtoCommerce"))
                    {
                        membershipUser.Profiles.Add("IsVirtoCommerce", "True");
                        _manager.Update(membershipUser, membershipUser);
                    }
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

                if (VirtoRegisterMemberPlugin.RegisterVirtoUser(_manager, _userClient, registerMemberModel))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
