using System;
using System.Linq;
using System.Text;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using System.Collections.Generic;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Persistent.Base.General;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects
{
    [Persistent("LoopCheck")]
    public abstract class LoopCheckEvent : BaseObject, IEvent
    {
        private readonly EventImpl appointmentImpl = new EventImpl();
        
        public LoopCheckEvent(Session session)
            : base(session)
        {
            //Resources.ListChanged += new ListChangedEventHandler(Resources_ListChanged);
        }
        
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            appointmentImpl.AfterConstruction();
        }

        [NonPersistent]
        public virtual bool AllDay
        {
            get { return appointmentImpl.AllDay; }
            set
            {
                appointmentImpl.AllDay = value;
                OnChanged("AllDay");
            }
        }

        [NonPersistent, Browsable(false)]
        public object AppointmentId
        {
            get { return Oid; }
        }

        [NonPersistent]
        [Size(SizeAttribute.Unlimited), ObjectValidatorIgnoreIssue(typeof(ObjectValidatorLargeNonDelayedMember))]
        public virtual string Description
        {
            get { return appointmentImpl.Description; }
 
            set
            {
                appointmentImpl.Description = value;
                OnChanged("Description");
            }
        }

        [Indexed]
        public virtual DateTime EndOn
        {
            get { return appointmentImpl.EndOn; }

            set
            {
                appointmentImpl.EndOn = value;
                OnChanged("EndOn");
            }
        }

        [NonPersistent]
        public virtual int Label
        {
            get { return appointmentImpl.Label; }

            set
            {
                appointmentImpl.Label = value;
                OnChanged("Label");
            }
        }

        [NonPersistent]
        public string Location
        {
            get { return appointmentImpl.Location; }

            set
            {
                appointmentImpl.Location = value;
                OnChanged("Location");
            }
        }

        [NonPersistent]
        public string ResourceId
        {
            get
            {
                return null;
            }

            set
            {
            }
        }

        [Indexed]
        public virtual DateTime StartOn
        {
            get { return appointmentImpl.StartOn; }

            set
            {
                DateTime oldValue = appointmentImpl.StartOn;
                appointmentImpl.StartOn = value;
                OnChanged("StartOn",oldValue,appointmentImpl.StartOn);
            }
        }

        [NonPersistent]
        public virtual int Status
        {
            get { return appointmentImpl.Status; }

            set
            {
                appointmentImpl.Status = value;
                OnChanged("Status");
            }
        }

        [Size(250), NonPersistent]
        public virtual string Subject
        {
            get { return appointmentImpl.Subject; }

            set
            {
                appointmentImpl.Subject = value;
                OnChanged("Subject");
            }
        }

        [NonPersistent]
        public virtual int Type
        {
            get { return appointmentImpl.Type; }

            set
            {
                appointmentImpl.Type = value;
                OnChanged("Type");
            }
        }

    }
}
