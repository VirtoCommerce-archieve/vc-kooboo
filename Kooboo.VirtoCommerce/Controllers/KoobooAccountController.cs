using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Kooboo.CMS.Common;
using VirtoCommerce.Client;
using VirtoCommerce.Foundation.Customers.Model;
using VirtoCommerce.Foundation.Security.Model;
using VirtoCommerce.Web.Client.Services.Security;
using VirtoCommerce.Web.Models;
using VirtoCommerce.Web.Virto.Helpers;

namespace Kooboo.VirtoCommerce.Controllers
{
    public class KoobooAccountController : Controller
    {
        /// <summary>
        /// The _user client
        /// </summary>
        private readonly UserClient _userClient;
         /// <summary>
        /// The _web security
        /// </summary>
        private readonly IUserSecurity _webSecurity;

        public KoobooAccountController(IUserSecurity webSecurity, UserClient userClient)
        {
            _webSecurity = webSecurity;
            _userClient = userClient;
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            JsonResultData resultData = new JsonResultData();

            if (ModelState.IsValid)
            {
                string error;

                if (!Register(model, out error))
                {
                    ModelState.AddModelError("", error);
                    resultData.AddModelState(ModelState);
                    resultData.Success = false;
                }
                else
                {
                    OnPostLogon(model.Email);
                    resultData.Success = true;
                }
            }

            // If we got this far, something failed, redisplay form
            return new JsonResult { Data = resultData };
        }

        /// <summary>
        /// After user has logged in do some actions
        /// </summary>
        private void OnPostLogon(string userName)
        {
            var customerId = _webSecurity.GetUserId(userName);
            var contact = _userClient.GetCustomer(customerId.ToString(CultureInfo.InvariantCulture), false);
            var account = _userClient.GetAccountByUserName(userName);

            if (contact != null)
            {
                var lastVisited = contact.ContactPropertyValues.FirstOrDefault(x => x.Name == ContactPropertyValueName.LastVisit);

                if (lastVisited != null)
                {
                    lastVisited.DateTimeValue = DateTime.UtcNow;
                }
                else
                {
                    lastVisited = new ContactPropertyValue
                    {
                        Name = ContactPropertyValueName.LastVisit,
                        DateTimeValue = DateTime.UtcNow,
                        ValueType = PropertyValueType.DateTime.GetHashCode()
                    };
                    contact.ContactPropertyValues.Add(lastVisited);
                }
                _userClient.SaveCustomerChanges();
            }
        }

        private bool Register(RegisterModel model, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var id = Guid.NewGuid().ToString();

                _webSecurity.CreateUserAndAccount(model.Email, model.Password, new
                {
                    MemberId = id,
                    UserHelper.CustomerSession.StoreId,
                    RegisterType = RegisterType.GuestUser.GetHashCode(),
                    AccountState = AccountState.Approved.GetHashCode(),
                    Discriminator = "Account"
                });

                var contact = new Contact
                {
                    MemberId = id,
                    FullName = String.Format("{0} {1}", model.FirstName, model.LastName)
                };

                contact.Emails.Add(new Email { Address = model.Email, MemberId = id, Type = EmailType.Primary.ToString() });
                foreach (var addr in model.Addresses)
                {
                    contact.Addresses.Add(addr);
                }

                _userClient.CreateContact(contact);

                return _webSecurity.Login(model.Email, model.Password);
            }
            catch (MembershipCreateUserException e)
            {
                errorMessage = ErrorCodeToString(e.StatusCode);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            return false;
        }

        /// <summary>
        /// Converts error code to string.
        /// </summary>
        /// <param name="createStatus">The create status.</param>
        /// <returns>System.String.</returns>
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return
                        "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return
                        "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return
                        "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return
                        "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }
}
