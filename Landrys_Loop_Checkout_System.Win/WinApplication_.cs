using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Win;
using DevExpress.Xpo.DB;
using Landrys_Loop_Checkout_System.Module;
using Landrys_Loop_Checkout_System.Module.Win;
using Landrys_Loop_Checkout_System.Module.Win.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Landrys_Loop_Checkout_System.Win
{
    public partial class Landrys_Loop_Checkout_SystemWindowsFormsApplication : WinApplication, IApplicationFactory
    {
        //protected override void ReadLastLogonParametersCore(DevExpress.ExpressApp.Utils.SettingsStorage storage, object logonObject)
        //{
        //    AuthenticationStandardLogonParameters standardLogonParameters = logonObject as AuthenticationStandardLogonParameters;
        //    if ((standardLogonParameters != null) && string.IsNullOrEmpty(standardLogonParameters.UserName))
        //    {
        //        base.ReadLastLogonParametersCore(storage, logonObject);
        //    }
        //}
        protected override void OnLoggingOn(LogonEventArgs args)
        {
            base.OnLoggingOn(args);
            ChangeDatabaseHelper.UpdateDatabaseName(this, ((IDataFilePathParameter)args.LogonParameters).DataFilePath);
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
            // Refer to the https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112680.aspx help article for more details on how to provide a custom splash form.
            //winApplication.SplashScreen = new DevExpress.ExpressApp.Win.Utils.DXSplashScreen("YourSplashImage.png");

            //((SecurityBase)winApplication.Security).Authentication = new WinChangeDatabaseStandardAuthentication();

            WinChangeDatabaseActiveDirectoryAuthentication activeDirectoryAuthentication = new WinChangeDatabaseActiveDirectoryAuthentication();
            activeDirectoryAuthentication.CreateUserAutomatically = true;

            ((SecurityStrategyComplex)winApplication.Security).Authentication = activeDirectoryAuthentication;

            String[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Length > 1)
            {
                string connectionString = arguments[1].ToString();
                winApplication.ConnectionString = MSSqlCEConnectionProvider.GetConnectionString(connectionString);
                winApplication.Title = Path.GetFileName(connectionString);
            }
            else
            {
                //winApplication.ConnectionString = MSSqlCEConnectionProvider.GetConnectionString(@"C:\Users\dlandry\OneDrive\Visual Studio 2015\Projects\Landrys Loop Checkout System\Datafile\AutoCreatedFile.llcs");
                winApplication.ConnectionString= "Integrated Security = SSPI; Pooling = false; Data Source = (localdb)\\mssqllocaldb; Initial Catalog = Landrys_Loop_Checkout_System";
                //winApplication.ConnectionString = DevExpress.ExpressApp.Xpo.InMemoryDataStoreProvider.ConnectionString;
                winApplication.Title = "In-Memory Data Provider - WORK WILL NOT BE SAVED.";
                
            }
            return winApplication;
        }
    }
}
