﻿using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout
{
    [ImageName("box")]
    [DefaultClassOptions, DefaultProperty("Number")]
    [DefaultListViewOptions(true, NewItemRowPosition.Bottom)]
    public partial class JunctionBox
    {
        public JunctionBox(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        private XPCollection<AuditDataItemPersistent> auditTrail;
        public XPCollection<AuditDataItemPersistent> AuditTrail
        {
            get
            {
                if (auditTrail == null)
                {
                    auditTrail = AuditedObjectWeakReference.GetAuditTrail(Session, this);
                }
                return auditTrail;
            }
        }
    }

}
