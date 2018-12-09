using System;
using System.Linq;
using DevExpress.Xpo;
using System.Collections.Generic;
using Xpand.Persistent.Base.General.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects
{
    [CloneView(CloneViewType.ListView, "LoopCheck_ListView_Calendar")]
    [MapInheritance(MapInheritanceType.ParentTable)]
    [RuleCombinationOfPropertiesIsUnique(DefaultContexts.Save,"Schedule,Item")]
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
        // Fields...
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
                //SetPropertyValue("Subject", value.LoopNumber);
                Subject = value.LoopNumber;
            }
        }
        [RuleRequiredField]
        public Schedule Schedule
        {
            get { return _Schedule; }
            set { SetPropertyValue("Schedule", ref _Schedule, value); }
        }
        //public override string Subject
        //{
        //    get { if (Item != null) return Item.Loop.LoopNumber; return null; }
        //    set { }
        //}
        //public override bool AllDay
        //{
        //    get { return true; }
        //    set { }
        //}
        //public override DateTime StartOn
        //{
        //    get { return base.StartOn; }
        //    set
        //    {
        //        base.StartOn = value;
        //        CheckDate = value;
        //    }
        //}
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
        //public override string Subject
        //{
        //    get { if (_Item != null) return _Item.LoopNumber; return null; }
        //    set { }
        //}
    }


    //public class LoopCheck : LoopCheckEvent
    //{ 
    //    public LoopCheck(Session session) : base(session) { }
    //    
    //    public override void AfterConstruction() {
    //        base.AfterConstruction();
    //    }

    //    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    //    {
    //        base.OnChanged(propertyName, oldValue, newValue);
    //        if (propertyName == "StartOn" && StartOn != CheckDate)
    //            CheckDate = StartOn;
    //    }
    //    // Fields...
    //    private Schedule _Schedule;
    //    private DateTime _CheckDate;
    //    private Loop _Item;

    //    [RuleRequiredField]
    //    public Loop Item
    //    {
    //        get { return _Item; }
    //        set { SetPropertyValue("Item", ref _Item, value);
    //            //Subject = value.LoopNumber;
    //        }
    //    }
    //    [RuleRequiredField]
    //    public Schedule Schedule
    //    {
    //        get { return _Schedule; }
    //        set { SetPropertyValue("Schedule", ref _Schedule, value); }
    //    }
    //    //public override string Subject
    //    //{
    //    //    get { if (Item != null) return Item.Loop.LoopNumber; return null; }
    //    //    set { }
    //    //}
    //    public override bool AllDay
    //    {
    //        get { return true; }
    //        set { }
    //    }
    //    public override DateTime StartOn
    //    {
    //        get { return base.StartOn; }
    //        set
    //        {
    //            base.StartOn = value;
    //            CheckDate = value;
    //        }
    //    }
    //    public DateTime CheckDate
    //    {
    //        get { return _CheckDate; }
    //        set { SetPropertyValue("CheckDate", ref _CheckDate, value);
    //            //StartOn = value;
    //            //EndOn = value;
    //        }
    //    }
    //    public override string Subject
    //    {
    //        get { if (_Item != null) return _Item.LoopNumber; return null; }
    //        set { }
    //    }
    //}
    public class Schedule : BaseObject
    {
        public Schedule(Session session) : base(session) { }
        
        // Fields...
        private string _Name;

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                SetPropertyValue("Name", ref _Name, value);
            }
        }
    }
}
