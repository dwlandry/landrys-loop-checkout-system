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
using Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using System.Collections;

namespace Landrys_Loop_Checkout_System.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class LoopViewController : ViewController
    {
        //DLandry note:  See Example https://www.devexpress.com/Support/Center/Question/Details/Q305425
        private ChoiceActionItem setStatusItem;
        private const string refreshItemId = "refresh";

        public LoopViewController()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            PopulateLoopCheckStatusCollection();
        }
        private void PopulateLoopCheckStatusCollection()
        {
            SetLoopCheckStatusAction.Items.Clear();
            SetLoopCheckStatusAction.Items.Add(new ChoiceActionItem(refreshItemId, "Refresh Status List", null));
            List<SortProperty> sortProperties = new List<SortProperty>();
            sortProperties.Add(new SortProperty("Description", SortingDirection.Ascending));
            foreach (LoopCheckStatus loopCheckStatus in View.ObjectSpace.CreateCollection(typeof(LoopCheckStatus),null,sortProperties))
            {
                string itemCaption = CaptionHelper.GetMemberCaption(typeof(LoopCheckStatus), "Status") + " :" + loopCheckStatus.Description;
                SetLoopCheckStatusAction.Items.Add(new ChoiceActionItem(loopCheckStatus.Description, itemCaption, loopCheckStatus));
            }
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

        private void SetLoopCheckStatusAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            if (e.SelectedChoiceActionItem.Id == refreshItemId)
            {
                PopulateLoopCheckStatusCollection();
            }
            else
            {
                IObjectSpace objectSpace = View is ListView ? Application.CreateObjectSpace() : View.ObjectSpace;
                //IObjectSpace objectSpace = Application.CreateObjectSpace();
                LoopCheckStatus loopCheckStatus = objectSpace.GetObject(e.SelectedChoiceActionItem.Data as LoopCheckStatus);

                ArrayList objectsToProcess = new ArrayList(e.SelectedObjects);
                foreach (var obj in objectsToProcess)
                {
                    Loop objInNewObjectSpace = (Loop)objectSpace.GetObject(obj);
                    objInNewObjectSpace.LoopCheckStatus = loopCheckStatus; // (LoopCheckStatus)e.SelectedChoiceActionItem.Data;
                }

                objectSpace.CommitChanges();
                View.ObjectSpace.Refresh();
            }
        }
    }
}
