using System.ComponentModel;
using System.IO;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects
{
    [DefaultProperty(nameof(FileName))]
    [ImageName("Action_Open")]
    public class FileLinkObject : BaseObject
    {
        private string _fileName;
        private string _fullName;

        public FileLinkObject(Session session) : base(session) { }

        [Size(260)]
        public string FileName
        {
            get { return _fileName; }
            set { SetPropertyValue(nameof(FileName), ref _fileName, value); }
        }

        [Size(SizeAttribute.Unlimited)]
        [ToolTip("Full path to the file on disk. Use Browse on the toolbar to pick a file.")]
        public string FullName
        {
            get { return _fullName; }
            set
            {
                SetPropertyValue(nameof(FullName), ref _fullName, value);
                if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(FileName))
                {
                    FileName = Path.GetFileName(value);
                }
            }
        }

        [Browsable(false)]
        public bool FileExists
        {
            get { return !string.IsNullOrEmpty(FullName) && File.Exists(FullName); }
            set { }
        }
    }
}
