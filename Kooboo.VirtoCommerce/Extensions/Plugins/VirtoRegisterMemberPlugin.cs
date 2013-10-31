using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Kooboo.CMS.Membership.Services;
using Kooboo.CMS.Sites.Extension;
using Kooboo.CMS.Sites.Membership;
using Kooboo.VirtoCommerce.Model;
using VirtoCommerce.Client;
using VirtoCommerce.Foundation.Customers.Model;
using VirtoCommerce.Foundation.Security.Model;
using VirtoCommerce.Web.Client.Helpers;
using VirtoCommerce.Web.Virto.Helpers;

namespace Kooboo.VirtoCommerce.Extensions.Plugins
{
    public class VirtoRegisterMemberPlugin : RegisterMemberPlugin
    {
        private readonly MembershipUserManager _manager;

        public VirtoRegisterMemberPlugin(MembershipUserManager manager)
            : base(manager)
        {
            _manager = manager;
        }

        protected override bool RegisterCore(ControllerContext controllerContext, CMS.Sites.Models.SubmissionSetting submissionSetting, out string redirectUrl)
        {
            if (base.RegisterCore(controllerContext, submissionSetting, out redirectUrl))
            {
                var registerMemberModel = new VirtoRegisterMemberModel();

                var valid = ModelBindHelper.BindModel(registerMemberModel, "", controllerContext, submissionSetting);

                if (valid && RegisterVirtoUser(_manager, StoreHelper.UserClient, registerMemberModel))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool RegisterVirtoUser(MembershipUserManager manager, UserClient userClient, VirtoRegisterMemberModel model)
        {
            try
            {
                var id = Guid.NewGuid().ToString();

                userClient.CreateAccount(new Account
                    {
                        AccountState = model.IsApproved ? 
                        AccountState.Approved.GetHashCode() : 
                        AccountState.PendingApproval.GetHashCode(),
                        MemberId = id,
                        StoreId = UserHelper.CustomerSession.StoreId,
                        RegisterType = RegisterType.GuestUser.GetHashCode(),
                        UserName = model.UserName
                    });

                var contact = new Contact
                {
                    MemberId = id,
                    FullName = String.Format("{0} {1}", model.FirstName, model.LastName).Trim()
                };

                contact.Emails.Add(new Email { Address = model.Email, MemberId = id, Type = EmailType.Primary.ToString() });
                userClient.CreateContact(contact);

                manager.EditMemberProfile(MemberPluginHelper.GetMembership(), 
                    model.UserName, 
                    model.Email, 
                    model.Culture, 
                    model.TimeZoneId, 
                    model.PasswordQuestion,
                    model.PasswordAnswer, 
                    model.Profiles);         
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
