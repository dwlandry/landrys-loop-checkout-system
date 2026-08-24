using System;
using System.IO;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Win;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Landrys_Loop_Checkout_System.Module;
using Landrys_Loop_Checkout_System.Module.Win;
using Landrys_Loop_Checkout_System.Module.Win.Controllers;

namespace Landrys_Loop_Checkout_System.Win
{
    public partial class Landrys_Loop_Checkout_SystemWindowsFormsApplication : WinApplication, IApplicationFactory
    {
        protected override void OnLoggingOn(LogonEventArgs args)
        {
            base.OnLoggingOn(args);
            if (args.LogonParameters is IDataFilePathParameter fileParameter)
            {
                ChangeDatabaseHelper.UpdateDatabaseName(this, fileParameter.DataFilePath);
            }
        }
        protected override bool OnLogonFailed(object logonParameters, Exception e)
        {
            return base.OnLogonFailed(logonParameters, e);
        }
        WinApplication IApplicationFactory.CreateApplication()
        {
            return CreateApplication();
        }
        public static Landrys_Loop_Checkout_SystemWindowsFormsApplication CreateApplication()
        {
            Landrys_Loop_Checkout_SystemWindowsFormsApplication winApplication = new Landrys_Loop_Checkout_SystemWindowsFormsApplication();

            WinChangeDatabaseActiveDirectoryAuthentication activeDirectoryAuthentication = new WinChangeDatabaseActiveDirectoryAuthentication();
            activeDirectoryAuthentication.CreateUserAutomatically = true;
            SecurityStrategyComplex security = (SecurityStrategyComplex)winApplication.Security;
            security.Authentication = activeDirectoryAuthentication;
            security.NewUserRoleName = "Administrators";

            string[] arguments = Environment.GetCommandLineArgs();
            string jobFilePath = FindJobFileArgument(arguments);
            if (!string.IsNullOrEmpty(jobFilePath))
            {
                winApplication.ConnectionString = JobDatabase.GetConnectionString(jobFilePath);
                winApplication.Title = Path.GetFileName(jobFilePath);
            }
            else
            {
                string defaultJob = JobDatabase.DefaultJobFilePath;
                winApplication.ConnectionString = JobDatabase.GetConnectionString(defaultJob);
                winApplication.Title = Path.GetFileName(defaultJob);
            }
            return winApplication;
        }

        private static string FindJobFileArgument(string[] arguments)
        {
            for (int i = 1; i < arguments.Length; i++)
            {
                string argument = arguments[i];
                if (argument.StartsWith("-", StringComparison.Ordinal) || argument.StartsWith("/", StringComparison.Ordinal))
                {
                    continue;
                }
                if (argument.EndsWith("." + JobDatabase.FileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetFullPath(argument);
                }
            }
            return null;
        }
    }
}
