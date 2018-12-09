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

namespace Landrys_Loop_Checkout_System.Module.Win.Controllers
{
    public interface IApplicationFactory
    {
        WinApplication CreateApplication();
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
                Filter = "Landry Loop Check System files (*.llcs)|*.llcs",
                Title = "Select desired location of the Job File.",
                CreatePrompt = true,
                OverwritePrompt=true,
                DefaultExt ="llcs",
                

            };
            if (fld.ShowDialog()==DialogResult.OK)
            {
                string fileName = fld.FileName;
                WinChangeDatabaseHelper.DataFilePath = fileName;
                Frame.GetController<LogoffController>().LogoffAction.DoExecute();
            }
            
            
        }
        private void OpenJobAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            OpenFileDialog fld = new OpenFileDialog() { Filter = "Landry Loop Check System files (*.llcs)|*.llcs", RestoreDirectory = true };
            if (fld.ShowDialog() == DialogResult.OK)
            {
                string fileName = fld.FileName;
                WinChangeDatabaseHelper.DataFilePath = fileName;
                //WinChangeDatabaseHelper.SkipLogonDialog = true;
                //WinChangeDatabaseStandardAuthentication.AuthenticatedUserName = SecuritySystem.CurrentUserName;
                Frame.GetController<LogoffController>().LogoffAction.DoExecute();
                
                //((IDataFilePathParameter)SecuritySystem.LogonParameters).DataFilePath = WinChangeDatabaseHelper.DataFilePath;
            }
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
