using System;
using System.Linq;
using System.Web.Mvc;
using Kooboo.CMS.Membership.Services;
using Kooboo.CMS.Sites.Extension;
using Kooboo.CMS.Sites.Membership;
using Kooboo.CMS.Sites.Models;
using Kooboo.VirtoCommerce.Model;
using VirtoCommerce.Client;
using VirtoCommerce.Foundation.Customers.Model;

namespace Kooboo.VirtoCommerce.Extensions.Plugins
{
    public class VirtoEditMemberProfilePlugin : EditMemberProfilePlugin
    {
        private readonly UserClient _userClient = DependencyResolver.Current.GetService<UserClient>();

        public VirtoEditMemberProfilePlugin(MembershipUserManager manager) : base(manager)
        {
        }

        protected override bool EditCore(ControllerContext controllerContext, SubmissionSetting submissionSetting, out string redirectUrl)
        {
            if(base.EditCore(controllerContext, submissionSetting, out redirectUrl))
            {
                var model = new VirtoEditMemberProfileModel();
                ModelBindHelper.BindModel(model, "", controllerContext, submissionSetting);

                UpdateVirtoContact(model);

                if (string.IsNullOrEmpty(redirectUrl))
                {
                    redirectUrl = model.RedirectUrl;
                    if (!string.IsNullOrEmpty(redirectUrl))
                    {
                        redirectUrl = MemberPluginHelper.ResolveSiteUrl(controllerContext, redirectUrl);
                    }
                }
                return true;
            }
            return false;
        }

        private void UpdateVirtoContact(VirtoEditMemberProfileModel model)
        {
            var u = _userClient.GetCurrentCustomer(false) ?? _userClient.NewContact();

            u.FullName = string.Format("{0} {1}", model.Profiles["FirstName"], model.Profiles["LastName"]);

            var primaryEmail = u.Emails.FirstOrDefault(e => e.Type == EmailType.Primary.ToString());
            if (primaryEmail != null)
            {
                primaryEmail.Address = model.Email;
            }
            else
            {
                var newEmail = new Email
                {
                    Address = model.Email,
                    MemberId = u.MemberId,
                    Type = EmailType.Primary.ToString()
                };
                u.Emails.Add(newEmail);
            }

            int age;
            if (model.Profiles.ContainsKey("Age") && int.TryParse(model.Profiles["Age"], out age))
            {
                //This should be changed in UI?
                u.BirthDate = DateTime.Now.Subtract(TimeSpan.FromDays(365 * age));
            }

            u.TimeZone = model.TimeZoneId;
            u.DefaultLanguage = model.Culture;

            _userClient.SaveCustomerChanges();
        }
    }
}
