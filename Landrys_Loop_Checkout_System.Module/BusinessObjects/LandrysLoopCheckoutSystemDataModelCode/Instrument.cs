using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using Landrys_Loop_Checkout_System.Module.BusinessObjects;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout
{
    [DefaultClassOptions, ImageName("gauge"), DefaultProperty("TagNumber")]
    [DefaultListViewOptions(true, NewItemRowPosition.Bottom)]
    public partial class Instrument
    {
        public Instrument(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        private FileLinkObject _DataSheet;
        private XPCollection<AuditDataItemPersistent> auditTrail;

        public FileLinkObject DataSheet
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
