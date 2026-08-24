using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;

namespace Landrys_Loop_Checkout_System.Module.Win
{
    public class WinChangeDatabaseHelper
    {
        public static string DataFilePath;
        public static string CurrentDataFilePath;
        public static bool AuthenticatedUserLogonFailed = false;
    }

    public class WinChangeDatabaseActiveDirectoryAuthentication : AuthenticationActiveDirectory<PermissionPolicyUser, ChangeDatabaseActiveDirectoryLogonParameters>
    {
        public WinChangeDatabaseActiveDirectoryAuthentication()
        {
            CreateUserAutomatically = true;
            CustomCreateUser += OnCustomCreateUser;
        }

        public override bool IsLogoffEnabled => true;
        public override bool AskLogonParametersViaUI => false;

        public override object Authenticate(DevExpress.ExpressApp.IObjectSpace objectSpace)
        {
            WinChangeDatabaseHelper.AuthenticatedUserLogonFailed = false;
            try
            {
                return base.Authenticate(objectSpace);
            }
            catch
            {
                WinChangeDatabaseHelper.AuthenticatedUserLogonFailed = true;
                throw;
            }
        }

        private void OnCustomCreateUser(object sender, CustomCreateUserEventArgs e)
        {
            var user = e.User as PermissionPolicyUser ?? e.ObjectSpace.CreateObject<PermissionPolicyUser>();
            if (string.IsNullOrEmpty(user.UserName))
            {
                user.UserName = e.UserName;
            }

            var adminRole = e.ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Administrators");
            if (adminRole == null)
            {
                adminRole = e.ObjectSpace.CreateObject<PermissionPolicyRole>();
                adminRole.Name = "Administrators";
                adminRole.IsAdministrative = true;
            }
            bool hasAdmin = false;
            foreach (PermissionPolicyRole role in user.Roles)
            {
                if (role == adminRole)
                {
                    hasAdmin = true;
                    break;
                }
            }
            if (!hasAdmin)
            {
                user.Roles.Add(adminRole);
            }

            e.User = user;
            e.Handled = true;
        }
    }
}
