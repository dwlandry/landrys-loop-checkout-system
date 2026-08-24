using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Win;
using DevExpress.Persistent.Base;
using Landrys_Loop_Checkout_System.Module;
using Landrys_Loop_Checkout_System.Module.Win;

namespace Landrys_Loop_Checkout_System.Module.Win.Controllers
{
    public interface IApplicationFactory
    {
        WinApplication CreateApplication();
    }

    public interface IJobFileSwitcher
    {
        bool SwitchJobFile(string filePath);
    }

    public partial class WinChangeDatabaseController : WindowController
    {
        private readonly SimpleAction _openJobAction;
        private readonly SimpleAction _newJobAction;
        private readonly SingleChoiceAction _recentJobsAction;

        public WinChangeDatabaseController()
        {
            TargetWindowType = WindowType.Main;
            _openJobAction = new SimpleAction(this, "OpenJobActionId", "File")
            {
                Caption = "Open Job...",
                ImageName = "Action_Open",
                ToolTip = "Open an existing job file."
            };
            _openJobAction.Execute += OpenJobAction_Execute;

            _newJobAction = new SimpleAction(this, "NewJobActionId", "File")
            {
                Caption = "Create New Job...",
                ImageName = "Action_New",
                ToolTip = "Create a new job file."
            };
            _newJobAction.Execute += NewJobAction_Execute;

            _recentJobsAction = new SingleChoiceAction(this, "RecentJobsActionId", "File")
            {
                Caption = "Recent Jobs",
                ImageName = "Action_Open",
                ItemType = SingleChoiceActionItemType.ItemIsOperation,
                ToolTip = "Open a recently used job file."
            };
            _recentJobsAction.Execute += RecentJobsAction_Execute;
        }

        private void NewJobAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = JobDatabase.FileFilter;
                dialog.Title = "Create New Job";
                dialog.CreatePrompt = true;
                dialog.OverwritePrompt = true;
                dialog.DefaultExt = JobDatabase.FileExtension;
                dialog.RestoreDirectory = true;
                ApplyInitialDirectory(dialog);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    OpenJobFile(dialog.FileName);
                }
            }
        }

        private void OpenJobAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = JobDatabase.FileFilter;
                dialog.Title = "Open Job";
                dialog.RestoreDirectory = true;
                ApplyInitialDirectory(dialog);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    OpenJobFile(dialog.FileName);
                }
            }
        }

        private void RecentJobsAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            string path = e.SelectedChoiceActionItem?.Data as string;
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (path == RecentJobFiles.ClearCommand)
            {
                RecentJobFiles.Clear();
                RefreshRecentJobs();
                return;
            }

            if (!File.Exists(path))
            {
                MessageBox.Show(
                    "This job file could not be found:" + Environment.NewLine + path,
                    "Recent Jobs",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                RecentJobFiles.Remove(path);
                RefreshRecentJobs();
                return;
            }

            OpenJobFile(path);
        }

        private void OpenJobFile(string fileName)
        {
            if (Application is IJobFileSwitcher switcher)
            {
                switcher.SwitchJobFile(fileName);
            }
            else
            {
                WinChangeDatabaseHelper.DataFilePath = fileName;
                RecentJobFiles.Add(fileName);
                Frame.GetController<LogoffController>()?.LogoffAction.DoExecute();
            }

            RefreshRecentJobs();
        }

        private void RefreshRecentJobs()
        {
            if (_recentJobsAction == null)
            {
                return;
            }

            IReadOnlyList<string> recents = RecentJobFiles.GetExisting();
            _recentJobsAction.BeginUpdate();
            try
            {
                _recentJobsAction.SelectedItem = null;
                _recentJobsAction.Items.Clear();
                for (int i = 0; i < recents.Count; i++)
                {
                    string path = recents[i];
                    var item = new ChoiceActionItem("RecentJob" + i, FormatRecentCaption(i, path, recents), path)
                    {
                        ToolTip = path,
                        ImageName = "Action_Open"
                    };
                    _recentJobsAction.Items.Add(item);
                }

                if (recents.Count > 0)
                {
                    _recentJobsAction.Items.Add(new ChoiceActionItem("ClearRecentJobs", "Clear Recent Jobs", RecentJobFiles.ClearCommand));
                }
            }
            finally
            {
                _recentJobsAction.EndUpdate();
            }

            _recentJobsAction.Active["HasRecentJobs"] = recents.Count > 0;
        }

        private static string FormatRecentCaption(int index, string path, IReadOnlyList<string> recents)
        {
            string fileName = Path.GetFileName(path);
            bool duplicateName = false;
            foreach (string recent in recents)
            {
                if (!string.Equals(recent, path, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(Path.GetFileName(recent), fileName, StringComparison.OrdinalIgnoreCase))
                {
                    duplicateName = true;
                    break;
                }
            }

            string label = duplicateName
                ? fileName + "  (" + Path.GetDirectoryName(path) + ")"
                : fileName;
            if (index < 9)
            {
                return "&" + (index + 1) + "  " + label;
            }
            return (index + 1) + "  " + label;
        }

        private static void ApplyInitialDirectory(FileDialog dialog)
        {
            string directory = RecentJobFiles.GetInitialDirectory(WinChangeDatabaseHelper.CurrentDataFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                dialog.InitialDirectory = directory;
            }
        }

        void Application_LoggedOff(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(WinChangeDatabaseHelper.DataFilePath))
            {
                ((IDataFilePathParameter)SecuritySystem.LogonParameters).DataFilePath = WinChangeDatabaseHelper.DataFilePath;
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            Application.LoggedOff += Application_LoggedOff;
            _openJobAction.Shortcut = "Control+O";
            _newJobAction.Shortcut = "Control+N";
            _recentJobsAction.ShowItemsOnClick = true;
            LogoffController logoff = Frame.GetController<LogoffController>();
            if (logoff != null)
            {
                logoff.LogoffAction.Active["JobFileApp"] = false;
            }
            RefreshRecentJobs();
        }

        protected override void OnDeactivated()
        {
            Application.LoggedOff -= Application_LoggedOff;
            base.OnDeactivated();
        }
    }
}
