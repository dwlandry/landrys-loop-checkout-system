using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Persistent.Base;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout
{
    [DefaultClassOptions, DefaultProperty("Number")]
    public partial class LocationPlan
    {
        public LocationPlan(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
