//using System;
//using System.Linq;
//using System.Text;
//using DevExpress.Xpo;
//using DevExpress.ExpressApp;
//using System.ComponentModel;
//using DevExpress.ExpressApp.DC;
//using DevExpress.Data.Filtering;
//using DevExpress.Persistent.Base;
//using System.Collections.Generic;
//using DevExpress.ExpressApp.Model;
//using DevExpress.Persistent.BaseImpl;
//using DevExpress.Persistent.Validation;
//using Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout;
//using Xpand.ExpressApp.Security.Core;

//namespace Landrys_Loop_Checkout_System.Module.BusinessObjects
//{
//    [FullPermission]
//    [VisibleInReports]
//    public class LoopItem : BaseObject
//    { 
//        public LoopItem(Session session): base(session){}

//        public override void AfterConstruction()
//        {
//            base.AfterConstruction();
//        }

//        // Fields...
//        private bool _LoopFolderReceived;
//        private LoopCheckStatus _Status;
//        private Loop _Loop;

//        public Loop Loop
//        {
//            get { return _Loop; }
//            set { SetPropertyValue("Loop", ref _Loop, value); }
//        }
//        
//        public LoopCheckStatus Status
//        {
//            get { return _Status; }
//            set { SetPropertyValue("Status", ref _Status, value); }
//        }
//        
//        public bool LoopFolderReceived
//        {
//            get { return _LoopFolderReceived; }
//            set { SetPropertyValue("LoopFolderReceived", ref _LoopFolderReceived, value); }
//        }
//    }

//    public enum LoopCheckStatus
//    {
//        NotReadyForCheck,
//        ReadyForCheck,
//        BeingChecked,
//        EngineeringProblems,
//        ContractorWiringMods,
//        CompleteButWaitingOnGraphicsRepair,
//        CompleteButWaitingOnConfigRepair,
//        CompleteButWaitingOnSignature,
//        CompleteReadyForStartUp
//    }
//}
