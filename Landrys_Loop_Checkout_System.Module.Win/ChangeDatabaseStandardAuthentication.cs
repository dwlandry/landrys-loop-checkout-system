using System;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Security;

namespace Landrys_Loop_Checkout_System.Module.Win
{
    public class WinChangeDatabaseHelper
    {
        //private static bool skipLogonDialog = false;
        public static string DataFilePath;//DatabaseName;
        public static bool AuthenticatedUserLogonFailed = false;
        //public static bool SkipLogonDialog
        //{
        //    get { return skipLogonDialog; }
        //    set { skipLogonDialog = value; }
        //}
    }

    //public class WinChangeDatabaseStandardAuthentication : AuthenticationStandard<User, ChangeDatabaseStandardAuthenticationLogonParameters>
    //{
    //    public static string AuthenticatedUserName;

    //    public override bool AskLogonParametersViaUI
    //    {
    //        get
    //        {
    //            if (WinChangeDatabaseHelper.SkipLogonDialog)
    //            {
    //                return false;
    //            }
    //            return base.AskLogonParametersViaUI; // DLandry:  Should never want to return this - never want to display the LogonDialog.
    //        }
    //    }
    //    public override object Authenticate(DevExpress.ExpressApp.IObjectSpace objectSpace)
    //    {
    //        WinChangeDatabaseHelper.AuthenticatedUserLogonFailed = false;
    //        if (string.IsNullOrEmpty(AuthenticatedUserName))
    //        {
    //            return base.Authenticate(objectSpace);
    //        }
    //        else
    //        {
    //            ChangeDatabaseStandardAuthenticationLogonParameters logonParameters = (ChangeDatabaseStandardAuthenticationLogonParameters)LogonParameters;
    //            object result = objectSpace.FindObject(UserType, new BinaryOperator("UserName", logonParameters.UserName));
    //            if (result == null)
    //            {
    //                WinChangeDatabaseHelper.AuthenticatedUserLogonFailed = true;
    //                WinChangeDatabaseHelper.SkipLogonDialog = true; // DLandry: Changed from false to true.
    //                throw new AuthenticationException(logonParameters.UserName, SecurityExceptionLocalizer.GetExceptionMessage(SecurityExceptionId.RetypeTheInformation));
    //            }
    //            AuthenticatedUserName = "";
    //            return result;
    //        }
    //    }
    //}

    public class WinChangeDatabaseActiveDirectoryAuthentication : AuthenticationActiveDirectory<User, ChangeDatabaseActiveDirectoryLogonParameters>
    {
        public override bool IsLogoffEnabled => true;
        public override bool AskLogonParametersViaUI
        {
            get
            {
                //if (WinChangeDatabaseHelper.SkipLogonDialog)
                //{
                    return false;
                //}
                //return true;
            }
        }

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
                //WinChangeDatabaseHelper.SkipLogonDialog = true; // DLandry:  Changed from false to true.
                throw;
            }
        }
    }
}
