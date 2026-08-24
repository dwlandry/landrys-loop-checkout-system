using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp.Security;
using System.Windows.Forms;
using DevExpress.ExpressApp.Win;
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

    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppWindowControllertopic.aspx.
    public partial class WinChangeDatabaseController : WindowController
    {
        private readonly SimpleAction _openJobAction;
        private readonly SimpleAction _newJobAction;

        public WinChangeDatabaseController()
        {
            this.TargetWindowType = WindowType.Main;
            _openJobAction = new SimpleAction(this, "OpenJobActionId", "File")
            {
                Caption = "Open Job...",
                ImageName = "Action_Open",
            };
            _openJobAction.Execute += OpenJobAction_Execute;
            
            _newJobAction = new SimpleAction(this, "NewJobActionId", "File")
            {
                Caption = "Create New Job...",
                ImageName = "Action_New",
            };
            _newJobAction.Execute += NewJobAction_Execute;
            
            //InitializeComponent();
        }
        private void NewJobAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            SaveFileDialog fld = new SaveFileDialog()
            {
                Filter = JobDatabase.FileFilter,
                Title = "Select desired location of the Job File.",
                CreatePrompt = true,
                OverwritePrompt=true,
                DefaultExt = JobDatabase.FileExtension,
            };
            if (fld.ShowDialog()==DialogResult.OK)
            {
                OpenJobFile(fld.FileName);
            }
        }
        private void OpenJobAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            OpenFileDialog fld = new OpenFileDialog() { Filter = JobDatabase.FileFilter, RestoreDirectory = true };
            if (fld.ShowDialog() == DialogResult.OK)
            {
                OpenJobFile(fld.FileName);
            }
        }
        private void OpenJobFile(string fileName)
        {
            if (Application is IJobFileSwitcher switcher)
            {
                switcher.SwitchJobFile(fileName);
                return;
            }

            WinChangeDatabaseHelper.DataFilePath = fileName;
            Frame.GetController<LogoffController>().LogoffAction.DoExecute();
        }
        void Application_LoggedOff(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(WinChangeDatabaseHelper.DataFilePath))
            {
                ((IDataFilePathParameter)SecuritySystem.LogonParameters).DataFilePath = WinChangeDatabaseHelper.DataFilePath;
            }
            //AuthenticationStandardLogonParameters authenticationStandardLogonParameters = SecuritySystem.LogonParameters as AuthenticationStandardLogonParameters;
            //if (authenticationStandardLogonParameters != null && !string.IsNullOrEmpty(WinChangeDatabaseStandardAuthentication.AuthenticatedUserName))
            //{
            //    authenticationStandardLogonParameters.UserName = WinChangeDatabaseStandardAuthentication.AuthenticatedUserName;
            //}
        }

        //void Application_LoggedOn(object sender, LogonEventArgs e)
        //{
        //    WinChangeDatabaseHelper.SkipLogonDialog = true; // DLandry:  Switched from false to true.
        //}
        protected override void OnActivated()
        {
            base.OnActivated();
            //Application.LoggedOn += new EventHandler<LogonEventArgs>(Application_LoggedOn);
            Application.LoggedOff += new EventHandler<EventArgs>(Application_LoggedOff);
        }
        
    }
}
