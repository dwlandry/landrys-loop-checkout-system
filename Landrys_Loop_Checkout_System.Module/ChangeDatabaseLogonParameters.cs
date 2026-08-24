using System.IO;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp;
using DevExpress.Xpo;

namespace Landrys_Loop_Checkout_System.Module
{
    public interface IDataFilePathParameter
    {
        string DataFilePath { get; set; }
    }
    public class ChangeDatabaseHelper
    {
        public static void UpdateDatabaseName(XafApplication application, string dataFilePath)
        {
            if (!string.IsNullOrEmpty(dataFilePath))
            {
                application.ConnectionString = JobDatabase.GetConnectionString(dataFilePath);
                application.Title = Path.GetFileName(dataFilePath);
            }
        }
    }

    [DomainComponent]
    public class ChangeDatabaseActiveDirectoryLogonParameters : IDataFilePathParameter
    {
        private string datafilePath;

        public string DataFilePath
        {
            get { return datafilePath; }
            set { datafilePath = value; }
        }
    }
}
