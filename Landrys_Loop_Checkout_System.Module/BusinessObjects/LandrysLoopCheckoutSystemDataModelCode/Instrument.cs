using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp;
using Xpand.ExpressApp.FileAttachment.BusinessObjects;
using DevExpress.Persistent.BaseImpl;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout
{
    [DefaultClassOptions, ImageName("gauge"), DefaultProperty("TagNumber")]
    [DefaultListViewOptions(true, NewItemRowPosition.Bottom)]
    [FileAttachment("DataSheet")]
    public partial class Instrument
    {
        public Instrument(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        // Fields...
        private FileSystemLinkObject _DataSheet;
        private XPCollection<AuditDataItemPersistent> auditTrail;

        public FileSystemLinkObject DataSheet
        {
            get
            {
                return _DataSheet;
            }
            set
            {
                SetPropertyValue("DataSheet", ref _DataSheet, value);
            }
        }

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
