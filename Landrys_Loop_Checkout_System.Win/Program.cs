using System;
using System.Windows.Forms;
using DevExpress.ExpressApp;

namespace Landrys_Loop_Checkout_System.Win {
    static class Program {
        [STAThread]
        static void Main() {
            DevExpress.ExpressApp.FrameworkSettings.DefaultSettingsCompatibilityMode = DevExpress.ExpressApp.FrameworkSettingsCompatibilityMode.Latest;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                LlcsFileAssociation.EnsureRegistered();
            }
            catch
            {
            }
            // EditModelPermission.AlwaysGranted = System.Diagnostics.Debugger.IsAttached;
            Landrys_Loop_Checkout_SystemWindowsFormsApplication winApplication = Landrys_Loop_Checkout_SystemWindowsFormsApplication.CreateApplication();

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
