using DevExpress.ExpressApp.Model;
using System;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using DevExpress.Xpo.DB.Helpers;
using DevExpress.ExpressApp.Security;
using DevExpress.Xpo.DB;
using System.IO;

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
            if (dataFilePath!=null)
            {
                application.ConnectionString = MSSqlCEConnectionProvider.GetConnectionString(dataFilePath);
                application.Title = Path.GetFileName(dataFilePath);
            }
            //else
            //{
            //    application.ConnectionString = DevExpress.ExpressApp.Xpo.InMemoryDataStoreProvider.ConnectionString;
            //}
                
        }
    }

    //[NonPersistent]
    //public class ChangeDatabaseStandardAuthenticationLogonParameters : AuthenticationStandardLogonParameters, IDataFilePathParameter
    //{
    //    private string datafilePath;

    //    public string DataFilePath
    //    {
    //        get { return datafilePath; }
    //        set { datafilePath = value; }
    //    }
    //}

    [NonPersistent]
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
