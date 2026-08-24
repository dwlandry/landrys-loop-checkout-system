using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using Landrys_Loop_Checkout_System.Module.BusinessObjects;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout
{
    [ImageName("Drawing")]
    [DefaultListViewOptions(true, NewItemRowPosition.Bottom)]
    public partial class Drawing
    {
        public Drawing(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction();}

        private XPCollection<AuditDataItemPersistent> auditTrail;

        public FileLinkObject File
        {
            get { return GetPropertyValue<FileLinkObject>(nameof(File)); }
            set { SetPropertyValue(nameof(File), value); }
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
