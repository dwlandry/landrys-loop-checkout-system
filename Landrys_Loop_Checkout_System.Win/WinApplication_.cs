using System;
using System.IO;
using System.Windows.Forms;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Win;
using DevExpress.XtraSplashScreen;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Landrys_Loop_Checkout_System.Module;
using Landrys_Loop_Checkout_System.Module.Win;
using Landrys_Loop_Checkout_System.Module.Win.Controllers;

namespace Landrys_Loop_Checkout_System.Win
{
    public partial class Landrys_Loop_Checkout_SystemWindowsFormsApplication : WinApplication, IApplicationFactory, IJobFileSwitcher
    {
        private bool _switchJobInPlace;

        protected override void OnLoggingOn(LogonEventArgs args)
        {
            base.OnLoggingOn(args);
            if (args.LogonParameters is IDataFilePathParameter fileParameter)
            {
                ChangeDatabaseHelper.UpdateDatabaseName(this, fileParameter.DataFilePath);
                if (!string.IsNullOrEmpty(fileParameter.DataFilePath))
                {
                    WinChangeDatabaseHelper.CurrentDataFilePath = fileParameter.DataFilePath;
                }
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
                WinChangeDatabaseHelper.CurrentDataFilePath = jobFilePath;
            }
            else
            {
                string defaultJob = JobDatabase.DefaultJobFilePath;
                winApplication.ConnectionString = JobDatabase.GetConnectionString(defaultJob);
                winApplication.Title = Path.GetFileName(defaultJob);
                WinChangeDatabaseHelper.CurrentDataFilePath = defaultJob;
            }
            return winApplication;
        }

        public bool SwitchJobFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            string fullPath = Path.GetFullPath(filePath);
            if (SameJobPath(fullPath, WinChangeDatabaseHelper.CurrentDataFilePath))
            {
                return true;
            }

            if (!CloseOpenJobWindows())
            {
                return false;
            }

            try
            {
                SaveModelChanges();
            }
            catch
            {
            }

            string previousPath = WinChangeDatabaseHelper.CurrentDataFilePath;
            WinWindow mainWindow = (ShowViewStrategy as WinShowViewStrategyBase)?.MainWindow
                ?? MainWindow as WinWindow;
            Form form = mainWindow?.Form;
            IOverlaySplashScreenHandle overlay = null;
            Cursor previousCursor = Cursor.Current;
            if (form != null)
            {
                form.UseWaitCursor = true;
                overlay = StartOverlayForm(form);
            }
            Cursor.Current = Cursors.WaitCursor;

            bool savedStartupLogic = ExecuteStartupLogicBeforeClosingLogonWindow;
            _switchJobInPlace = true;
            ExecuteStartupLogicBeforeClosingLogonWindow = false;
            try
            {
                ActivateJobFile(fullPath);
                ShowJobStartupView(mainWindow);
                return true;
            }
            catch (Exception exception)
            {
                if (!string.IsNullOrEmpty(previousPath) && !SameJobPath(previousPath, fullPath))
                {
                    try
                    {
                        ActivateJobFile(previousPath);
                        ShowJobStartupView(mainWindow);
                    }
                    catch
                    {
                    }
                }
                HandleException(exception);
                return false;
            }
            finally
            {
                ExecuteStartupLogicBeforeClosingLogonWindow = savedStartupLogic;
                _switchJobInPlace = false;
                Cursor.Current = previousCursor;
                if (form != null)
                {
                    form.UseWaitCursor = false;
                }
                if (overlay != null)
                {
                    StopOverlayForm(overlay);
                }
            }
        }

        public override void StartSplash()
        {
            if (_switchJobInPlace)
            {
                return;
            }
            base.StartSplash();
        }

        protected override void LoadUserDifferences()
        {
            if (_switchJobInPlace)
            {
                return;
            }
            base.LoadUserDifferences();
        }

        private void ActivateJobFile(string filePath)
        {
            WinChangeDatabaseHelper.DataFilePath = filePath;
            if (Security?.LogonParameters is IDataFilePathParameter fileParameter)
            {
                fileParameter.DataFilePath = filePath;
            }

            if (Security != null && Security.IsAuthenticated)
            {
                Security.Logoff();
            }
            isLoggedOn = false;
            Logon();
            WinChangeDatabaseHelper.CurrentDataFilePath = filePath;
        }

        private bool CloseOpenJobWindows()
        {
            if (ShowViewStrategy is WinShowViewStrategyBase winStrategy)
            {
                foreach (WinWindow inspector in winStrategy.Inspectors.ToArray())
                {
                    if (!inspector.Close())
                    {
                        return false;
                    }
                }

                WinWindow mainWindow = winStrategy.MainWindow;
                if (mainWindow != null && mainWindow.View != null && !mainWindow.SetView(null))
                {
                    return false;
                }
                return true;
            }

            if (MainWindow != null && MainWindow.View != null)
            {
                return MainWindow.SetView(null);
            }
            return true;
        }

        private static void ShowJobStartupView(WinWindow mainWindow)
        {
            Window window = mainWindow;
            if (window == null)
            {
                return;
            }

            ShowNavigationItemController navigation = window.GetController<ShowNavigationItemController>();
            navigation?.RecreateNavigationItems();
            navigation?.ShowStartupNavigationItem();
        }

        private static bool SameJobPath(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }
            return string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), StringComparison.OrdinalIgnoreCase);
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
