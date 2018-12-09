using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using System.ComponentModel;
using DevExpress.Utils.Serializing;
using DevExpress.XtraGrid;
using System.IO;
using DevExpress.Utils;
using DevExpress.ExpressApp.Win.Editors;

namespace Landrys_Loop_Checkout_System.Module.Win.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class GridListEditorViewController : ViewController<ListView>, IModelExtender
    {
        GridFormatRulesStore formatRulesStore = null;
        GridListEditor gridListEditor = null;
        public GridListEditorViewController(){ InitializeComponent(); }
        protected override void OnActivated()
        {
            base.OnActivated();
            gridListEditor = View.Editor as GridListEditor;
            if (gridListEditor != null)
            {
                View.ControlsCreated += View_ControlsCreated;
                View.ModelSaved += View_ModelSaved;
            }
        }
        void View_ControlsCreated(object sender, EventArgs e){ InitializeFormatRules(); }
        private void InitializeFormatRules()
        {
            gridListEditor.GridView.OptionsMenu.ShowConditionalFormattingItem = true;
            //Dennis: Uncomment this loop for versions older than v15.1.7 (https://www.devexpress.com/Support/Center/Question/Details/T291023).
            //foreach(XafGridColumnWrapper item in gridListEditor.Columns) {
            //	item.Column.Name = item.Id;
            //}
            formatRulesStore = new GridFormatRulesStore() { FormatRules = gridListEditor.GridView.FormatRules };
            formatRulesStore.Restore(((IModelListViewGridFormatRuleSettings)View.Model).GridFormattingSettings);
        }
        void View_ModelSaved(object sender, EventArgs e)
        {
            ((IModelListViewGridFormatRuleSettings)View.Model).GridFormattingSettings = formatRulesStore.Save();
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
        protected override void OnDeactivated()
        {
            if (gridListEditor != null)
            {
                View.ModelSaved -= View_ModelSaved;
                View.ControlsCreated -= View_ControlsCreated;
                gridListEditor = null;
            }
            base.OnDeactivated();
        }

        public void ExtendModelInterfaces(ModelInterfaceExtenders extenders)
        {
            extenders.Add<IModelListView, IModelListViewGridFormatRuleSettings>();
        }
    }
    public interface IModelListViewGridFormatRuleSettings
    {
        [Browsable(false)]
        string GridFormattingSettings { get; set; }
    }

    //Dennis: This is a modified version of the solution given at https://www.devexpress.com/Support/Center/Question/Details/T289562 that can save/load settings into/from a string.
    internal class GridFormatRulesStore : IXtraSerializable
    {
        GridFormatRuleCollection formatRules;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Browsable(true),
         XtraSerializableProperty(XtraSerializationVisibility.Collection, true, false, true, 1000, XtraSerializationFlags.DefaultValue)]
        public virtual GridFormatRuleCollection FormatRules
        {
            get { return formatRules; }
            set { formatRules = value; }
        }
        public string Save()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                SaveCore(new XmlXtraSerializer(), stream);
                return Convert.ToBase64String(stream.GetBuffer());
            }
        }
        public void Restore(string settings)
        {
            if (!string.IsNullOrEmpty(settings))
            {
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(settings)))
                {
                    RestoreCore(new XmlXtraSerializer(), stream);
                }
            }
        }
        void SaveCore(XtraSerializer serializer, object path)
        {
            Stream stream = path as Stream;
            if (stream != null)
                serializer.SerializeObject(this, stream, GetType().Name);
            else
                serializer.SerializeObject(this, path.ToString(), GetType().Name);
        }
        void RestoreCore(XtraSerializer serializer, object path)
        {
            Stream stream = path as Stream;
            if (stream != null)
                serializer.DeserializeObject(this, stream, GetType().Name);
            else
                serializer.DeserializeObject(this, path.ToString(), GetType().Name);
        }
        void XtraClearFormatRules(XtraItemEventArgs e) { FormatRules.Clear(); }
        object XtraCreateFormatRulesItem(XtraItemEventArgs e)
        {
            var rule = new GridFormatRule();
            formatRules.Add(rule);
            return rule;
        }
        #region IXtraSerializable
        public void OnEndDeserializing(string restoredVersion) { }
        public void OnEndSerializing() { }
        public void OnStartDeserializing(LayoutAllowEventArgs e) { }
        public void OnStartSerializing() { }
        #endregion
    }
}
