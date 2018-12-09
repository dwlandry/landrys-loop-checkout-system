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
using DevExpress.Persistent.Base.General;
using DevExpress.ExpressApp.Scheduler.Win;
using DevExpress.XtraScheduler;

namespace Landrys_Loop_Checkout_System.Module.Win.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class SchedulerController : ViewController
    {
        public SchedulerController()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            if (View.ObjectTypeInfo.Implements<IEvent>())
                View.ControlsCreated += new EventHandler(View_ControlsCreated);
        }

        private void View_ControlsCreated(object sender, EventArgs e)
        {
            ListView view = (ListView)View;
            SchedulerListEditor listEditor = (SchedulerListEditor)view.Editor;
            SchedulerControl scheduler = listEditor.SchedulerControl;
            scheduler.Views.DayView.AllDayAreaScrollBarVisible = true;
            scheduler.Views.TimelineView.AppointmentDisplayOptions.AppointmentAutoHeight = true;
            scheduler.Views.TimelineView.TimeIndicatorDisplayOptions.Visibility = TimeIndicatorVisibility.Never;
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            if (View.ObjectTypeInfo.Implements<IEvent>())
            {
                View.ControlsCreated -= new EventHandler(View_ControlsCreated);
            }
            base.OnDeactivated();
        }
    }
}
