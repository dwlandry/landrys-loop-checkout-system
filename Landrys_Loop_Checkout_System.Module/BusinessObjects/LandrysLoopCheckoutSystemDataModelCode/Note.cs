using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Base;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout
{
    [ImageName("BO_Note")]
    [DefaultListViewOptions(true, NewItemRowPosition.Bottom)]

    public partial class Note
    {
        public Note(Session session) : base(session) { }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            _AddedBy = SecuritySystem.CurrentUserName;
            _DateAdded = DateTime.Now;
        }

        private DateTime _DateAdded;
        private string _AddedBy;
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

        [ReadOnly(true)]
        public string AddedBy
        {
            get
            {
                return _AddedBy;
            }
            set
            {
                SetPropertyValue("AddedBy", ref _AddedBy, value);
            }
        }
        [ReadOnly(true)]
        public DateTime DateAdded
        {
            get
            {
                return _DateAdded;
            }
            set
            {
                SetPropertyValue("DateAdded", ref _DateAdded, value);
            }
        }
    }

}
