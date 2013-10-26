using System;
using System.Linq;

namespace Kooboo.VirtoCommerce.Security
{
    using global::VirtoCommerce.Foundation.Security.Model;
    using global::VirtoCommerce.Foundation.Security.Repositories;
    using global::VirtoCommerce.Web.Client.Services.Security;
    using Kooboo.CMS.Account.Models;
    using Kooboo.CMS.Account.Services;

    public class KBUserSecurity : IUserSecurity
    {
        private readonly UserManager _manager;
        private readonly ISecurityRepository _securityRepository;

        public KBUserSecurity(ISecurityRepository securityRepository)
        {
            _manager = ServiceFactory.UserManager;
            _securityRepository = securityRepository;
        }

        public bool ChangePassword(string userName, string currentPassword, string newPassword)
        {
            return _manager.ChangePassword(userName, currentPassword, newPassword);
        }

        public string CreateAccount(string userName, string password, bool requireConfirmationToken = false)
        {
            var user = new User();

            user.UserName = userName;
            user.IsAdministrator = false;
            user.IsLockedOut = false;
            user.Password = password;
            _manager.Add(user);

            return userName;
        }

        public string CreateUserAndAccount(string userName, string password, object propertyValues = null, bool requireConfirmationToken = false)
        {
            var user = new User();

            user.UserName = userName;
            user.IsAdministrator = false;
            user.IsLockedOut = false;
            user.Password = password;
            _manager.Add(user);

            return userName;
        }

        public bool DeleteUser(string userName)
        {
            _manager.Delete(userName);
            return true;
        }

        public string GetUserId(string userName)
        {
            var user = _manager.Get(userName);
            return user.UUID;
        }

        public bool Login(string userName, string password, bool persistCookie = false)
        {
            //Try to get account and check account state
            //If account is not found or state is not Approved - user cannot login
            var account = _securityRepository.Accounts.FirstOrDefault(
                a => a.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));

            if (account == null
                || !account.AccountState.GetHashCode().Equals(AccountState.Approved.GetHashCode()))
            {
                return false;
            }
            
            return _manager.ValidateUser(userName, password);
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

        public bool ResetPassword(string userName, string newPassword)
        {
            return _manager.ChangePassword(userName, newPassword);
        }
    }
}
