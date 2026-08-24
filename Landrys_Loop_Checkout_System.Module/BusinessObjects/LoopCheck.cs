using System;
using DevExpress.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects
{
    [MapInheritance(MapInheritanceType.ParentTable)]
    [RuleCombinationOfPropertiesIsUnique(DefaultContexts.Save, "Schedule,Item")]
    public class LoopCheck : Event
    {
        public LoopCheck(Session session)
            : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            base.Subject = Item.LoopNumber;
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            AllDay = true;
        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (propertyName == "StartOn" && StartOn != CheckDate)
                CheckDate = StartOn;
        }

        private Schedule _Schedule;
        private DateTime _CheckDate;
        private Loop _Item;

        [RuleRequiredField]
        public Loop Item
        {
            get { return _Item; }
            set
            {
                SetPropertyValue("Item", ref _Item, value);
                Subject = value.LoopNumber;
            }
        }

        [RuleRequiredField]
        public Schedule Schedule
        {
            get { return _Schedule; }
            set { SetPropertyValue("Schedule", ref _Schedule, value); }
        }

        public DateTime CheckDate
        {
            get { return _CheckDate; }
            set
            {
                SetPropertyValue("CheckDate", ref _CheckDate, value);
                base.StartOn = value;
                base.EndOn = value;
            }
        }
    }

    public class Schedule : BaseObject
    {
        public Schedule(Session session) : base(session) { }

        private string _Name;

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Name
        {
            get { return _Name; }
            set { SetPropertyValue("Name", ref _Name, value); }
        }
    }
}
