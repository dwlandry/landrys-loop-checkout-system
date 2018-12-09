using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Persistent.Base;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout
{
    [DefaultClassOptions, ImageName("BO_Scheduler")]
    public partial class DailySchedule
    {
        public DailySchedule(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        // Fields...
        private Contact _AssignedTo;

        [Association("Contact-DailySchedules")]
        public Contact AssignedTo
        {
            get
            {
                return _AssignedTo;
            }
            set
            {
                SetPropertyValue("AssignedTo", ref _AssignedTo, value);
            }
        }
    }

}
