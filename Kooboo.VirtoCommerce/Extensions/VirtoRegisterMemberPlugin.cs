using System;
using System.Web.Mvc;
using Kooboo.CMS.Membership.Services;
using Kooboo.CMS.Sites.Extension;
using Kooboo.CMS.Sites.Membership;
using Kooboo.VirtoCommerce.Model;
using VirtoCommerce.Client;
using VirtoCommerce.Foundation.Customers.Model;
using VirtoCommerce.Foundation.Security.Model;
using VirtoCommerce.Web.Virto.Helpers;

namespace Kooboo.VirtoCommerce.Extensions
{
    public class VirtoRegisterMemberPlugin : RegisterMemberPlugin
    {
        private readonly UserClient _userClient = DependencyResolver.Current.GetService<UserClient>();

        public VirtoRegisterMemberPlugin(MembershipUserManager manager)
            : base(manager)
        {
        }

        protected override bool RegisterCore(ControllerContext controllerContext, CMS.Sites.Models.SubmissionSetting submissionSetting, out string redirectUrl)
        {
            if (base.RegisterCore(controllerContext, submissionSetting, out redirectUrl))
            {
                var registerMemberModel = new VirtoRegisterMemberModel();

                var valid = ModelBindHelper.BindModel(registerMemberModel, "", controllerContext, submissionSetting);

                if (valid && RegisterVirtoUser(registerMemberModel))
                {
                    return true;
                }
            }
            return false;
        }

        private bool RegisterVirtoUser(VirtoRegisterMemberModel model)
        {
            try
            {
                var id = Guid.NewGuid().ToString();

                _userClient.CreateAccount(new Account
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
                    FullName = String.Format("{0} {1}", model.FirstName, model.LastName)
                };

                contact.Emails.Add(new Email { Address = model.Email, MemberId = id, Type = EmailType.Primary.ToString() });
                _userClient.CreateContact(contact);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
