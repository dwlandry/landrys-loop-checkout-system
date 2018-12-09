using System;
using System.Configuration;
using System.Windows.Forms;

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Win;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Security.Adapters;
using DevExpress.ExpressApp.Security.Xpo.Adapters;
using DevExpress.Xpo.DB;

namespace Landrys_Loop_Checkout_System.Win {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
#if EASYTEST
            DevExpress.ExpressApp.Win.EasyTest.EasyTestRemotingRegistration.Register();
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            EditModelPermission.AlwaysGranted = System.Diagnostics.Debugger.IsAttached;
            Landrys_Loop_Checkout_SystemWindowsFormsApplication winApplication = Landrys_Loop_Checkout_SystemWindowsFormsApplication.CreateApplication();

            IsGrantedAdapter.Enable(XPOSecurityAdapterHelper.GetXpoCachedRequestSecurityAdapters());
            if (System.Diagnostics.Debugger.IsAttached && winApplication.CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema) {
                winApplication.DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
            }
            winApplication.DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
            try {
                
                winApplication.Setup();
                winApplication.Start();
            }
            catch(Exception e) {
                winApplication.HandleException(e);
            }
            winApplication.Dispose();
        }
    }
}
