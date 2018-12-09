using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo.DB;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base.General;
using Xpand.ExpressApp.Security.Core;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout
{
    [DefaultClassOptions, ImageName("LoopCheckV2"), DefaultProperty("LoopNumber")]
    [DefaultListViewOptions(true, NewItemRowPosition.Bottom)]
    [FullPermission]
    public partial class Loop
    {
        public Loop(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        public string LoopCheckIsComplete
        {
            get
            {
                if (this.LoopCheckStatus != null)
                {
                    if (this.LoopCheckStatus.Description == "Complete - Ready for Startup")
                    {
                        return "Complete";
                    }
                    return "Not Complete";
                }
                return "Not Complete";
            }
        }

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

        public string PIDs
        {
            get
            {
                var list = new List<string>();

                foreach (var item in this.Instruments)
                {
                    if (item.PID != null)
                        if (!list.Contains(item.PID.Number))
                            list.Add(item.PID.Number);
                }

                list.Sort();
                return string.Join("; ", list);
            }
        }

        public string JunctionBoxes
        {
            get
            {
                var list = new List<string>();

                foreach (var item in this.Instruments)
                {
                    if (item.JunctionBox != null)
                        if (!list.Contains(item.JunctionBox.Number))
                            list.Add(item.JunctionBox.Number);
                }

                list.Sort();
                return string.Join("; ", list);
            }
        }

        public string LocationPlans
        {
            get
            {
                var list = new List<string>();

                foreach (var item in this.Instruments)
                {
                    if (item.LocationPlan != null)
                        if (!list.Contains(item.LocationPlan.Number))
                            list.Add(item.LocationPlan.Number);
                }

                list.Sort();
                return string.Join("; ", list);
            }
        }

        public string LoopDrawings
        {
            get
            {
                var list = new List<string>();

                foreach (var item in this.Instruments)
                {
                    if (item.LoopDrawing != null)
                        if (!list.Contains(item.LoopDrawing.Number))
                            list.Add(item.LoopDrawing.Number);
                }

                list.Sort();
                return string.Join("; ", list);
            }
        }

        public string AreaList
        {
            get
            {
                var list = new List<string>();

                foreach (var item in this.Areas)
                {
                    list.Add(item.Name);
                }

                list.Sort();
                return string.Join("; ", list);
            }
        }

        public string InstrumentTags
        {
            get
            {
                var list = new List<string>();

                foreach (var item in this.Instruments)
                {
                    list.Add(item.TagNumber);
                }

                list.Sort();
                return string.Join("; ", list);
            }
        }

        public string ResponsibleCompany
        {
            get
            {
                var list = new List<string>();

                foreach (var item in this.Instruments)
                {
                    if (item.ResponsibleCompany != null)
                        if (!list.Contains(item.ResponsibleCompany.Name))
                            list.Add(item.ResponsibleCompany.Name);
                }

                list.Sort();
                return string.Join("; ", list);
            }
        }


        [ModelDefault("Caption","Notes")]
        public string NoteList
        {
            get
            {
                var list = new List<string>();
                XPCollection<Note> notesCollection = this.Notes;
                SortingCollection sorting = new SortingCollection();
                sorting.Add(new SortProperty("DateAdded", SortingDirection.Ascending));
                notesCollection.Sorting = sorting;
                foreach (var item in notesCollection)
                {
                    list.Add(string.Format("{0}", item.Text));
                }

                list.Sort();
                return string.Join("; ", list);
            }
        }
    }

}
