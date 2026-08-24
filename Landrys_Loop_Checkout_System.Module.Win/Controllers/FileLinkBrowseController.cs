using System.IO;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Landrys_Loop_Checkout_System.Module.BusinessObjects;

namespace Landrys_Loop_Checkout_System.Module.Win.Controllers
{
    public class FileLinkBrowseController : ObjectViewController<ObjectView, FileLinkObject>
    {
        public FileLinkBrowseController()
        {
            var browse = new SimpleAction(this, "BrowseFileLink", PredefinedCategory.Edit)
            {
                Caption = "Browse...",
                ImageName = "Action_Open"
            };
            browse.Execute += (s, e) =>
            {
                using (var dialog = new System.Windows.Forms.OpenFileDialog())
                {
                    dialog.RestoreDirectory = true;
                    if (!string.IsNullOrEmpty(ViewCurrentObject?.FullName))
                    {
                        dialog.FileName = ViewCurrentObject.FullName;
                    }
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        ViewCurrentObject.FullName = dialog.FileName;
                        ViewCurrentObject.FileName = Path.GetFileName(dialog.FileName);
                        View.ObjectSpace.SetModified(ViewCurrentObject);
                    }
                }
            };

            var open = new SimpleAction(this, "OpenFileLink", PredefinedCategory.Edit)
            {
                Caption = "Open File",
                ImageName = "Action_Opening"
            };
            open.Execute += (s, e) =>
            {
                if (ViewCurrentObject != null && ViewCurrentObject.FileExists)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ViewCurrentObject.FullName,
                        UseShellExecute = true
                    });
                }
            };
        }
    }
}
