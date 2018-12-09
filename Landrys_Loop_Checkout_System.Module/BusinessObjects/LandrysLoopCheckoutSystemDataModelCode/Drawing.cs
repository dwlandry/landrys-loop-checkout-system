using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp;
using Xpand.ExpressApp.FileAttachment.BusinessObjects;
using DevExpress.Persistent.Base;
using Xpand.ExpressApp.FileAttachment;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout
{
    [ImageName("Drawing")]
    [DefaultListViewOptions(true, NewItemRowPosition.Bottom)]
    [FileAttachment("File")]
    public partial class Drawing
    {
        public Drawing(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction();}

        // Fields...
        //private FileSystemLinkObject _File;
        private XPCollection<AuditDataItemPersistent> auditTrail;

        public FileSystemLinkObject File
        {
            get { return GetPropertyValue<FileSystemLinkObject>("File"); }
            set { SetPropertyValue("File", value); }
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
