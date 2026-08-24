using System;
using System.Windows.Forms;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout;
using Landrys_Loop_Checkout_System.Module.Import;
using Landrys_Loop_Checkout_System.Module.Win.Forms;

namespace Landrys_Loop_Checkout_System.Module.Win.Controllers
{
    public class ImportIoListController : WindowController
    {
        public ImportIoListController()
        {
            TargetWindowType = WindowType.Main;
            var importAction = new SimpleAction(this, "ImportIoListAction", "File")
            {
                Caption = "Import I/O List...",
                ImageName = "Action_Export_ToXLSX",
                ToolTip = "Import instruments and loops from an Excel I/O list."
            };
            importAction.Execute += ImportAction_Execute;
        }

        private void ImportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Excel workbooks (*.xlsx)|*.xlsx";
                dialog.Title = "Select an I/O list";
                dialog.RestoreDirectory = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                IoListTable table;
                try
                {
                    table = IoListWorkbookReader.Read(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not read the workbook:\n" + ex.Message, "Import I/O List",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (table.Rows.Count == 0)
                {
                    MessageBox.Show("The worksheet has headers but no data rows.", "Import I/O List",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                IoListColumnMap map = IoListColumnMap.Merge(IoListColumnMap.LoadSaved(), table.Columns);
                using (var form = new ImportIoListForm(table, map))
                {
                    if (form.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                    map = form.Map;
                }

                try
                {
                    IoListImportResult result;
                    using (IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(Instrument)))
                    {
                        result = IoListImporter.Import(objectSpace, table, map);
                    }
                    map.Save();
                    if (Application.MainWindow?.View?.ObjectSpace != null)
                    {
                        Application.MainWindow.View.ObjectSpace.Refresh();
                    }
                    MessageBox.Show(result.ToString(), "Import I/O List", MessageBoxButtons.OK,
                        result.RowsFailed > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Import failed:\n" + ex.Message, "Import I/O List",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
