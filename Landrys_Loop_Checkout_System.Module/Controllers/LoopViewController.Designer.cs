namespace Landrys_Loop_Checkout_System.Module.Controllers
{
    partial class LoopViewController
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SetLoopCheckStatusAction = new DevExpress.ExpressApp.Actions.SingleChoiceAction(this.components);
            // 
            // SetLoopCheckStatusAction
            // 
            this.SetLoopCheckStatusAction.Caption = "Loop Check Status";
            this.SetLoopCheckStatusAction.Category = "Edit";
            this.SetLoopCheckStatusAction.ConfirmationMessage = "Are you sure you want to change the Loop Check Status for the selected items?";
            this.SetLoopCheckStatusAction.Id = "SetLoopCheckStatusAction";
            this.SetLoopCheckStatusAction.ItemType = DevExpress.ExpressApp.Actions.SingleChoiceActionItemType.ItemIsOperation;
            this.SetLoopCheckStatusAction.SelectionDependencyType = DevExpress.ExpressApp.Actions.SelectionDependencyType.RequireMultipleObjects;
            this.SetLoopCheckStatusAction.TargetObjectType = typeof(Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout.Loop);
            this.SetLoopCheckStatusAction.TargetViewType = DevExpress.ExpressApp.ViewType.ListView;
            this.SetLoopCheckStatusAction.ToolTip = null;
            this.SetLoopCheckStatusAction.TypeOfView = typeof(DevExpress.ExpressApp.ListView);
            this.SetLoopCheckStatusAction.Execute += new DevExpress.ExpressApp.Actions.SingleChoiceActionExecuteEventHandler(this.SetLoopCheckStatusAction_Execute);
            // 
            // LoopViewController
            // 
            this.Actions.Add(this.SetLoopCheckStatusAction);
            this.TargetObjectType = typeof(Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout.Loop);
            this.TargetViewType = DevExpress.ExpressApp.ViewType.ListView;
            this.TypeOfView = typeof(DevExpress.ExpressApp.ListView);

        }

        #endregion

        private DevExpress.ExpressApp.Actions.SingleChoiceAction SetLoopCheckStatusAction;
    }
}
