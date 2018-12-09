using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout
{
    [DefaultClassOptions, ImageName("BO_List"), DefaultProperty("Name")]
    [DefaultListViewOptions(true, NewItemRowPosition.Bottom)]
    public partial class Area
    {
        public Area(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
