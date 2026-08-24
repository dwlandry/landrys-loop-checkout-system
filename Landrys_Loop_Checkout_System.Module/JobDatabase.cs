using System;
using System.IO;
using DevExpress.Xpo.DB;

namespace Landrys_Loop_Checkout_System.Module
{
    public static class JobDatabase
    {
        public const string FileFilter = "Landry Loop Check System files (*.llcs)|*.llcs";
        public const string FileExtension = "llcs";

        public static string GetConnectionString(string dataFilePath)
        {
            if (string.IsNullOrWhiteSpace(dataFilePath))
            {
                throw new ArgumentException("A job file path is required.", nameof(dataFilePath));
            }

            string directory = Path.GetDirectoryName(dataFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return SQLiteConnectionProvider.GetConnectionString(dataFilePath);
        }

        public static string AppDataFolder
        {
            get
            {
                string folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "LandrysLoopCheckout");
                Directory.CreateDirectory(folder);
                return folder;
            }
        }

        public static string DefaultJobFilePath
        {
            get
            {
                return Path.Combine(AppDataFolder, "LandrysLoopCheckout.llcs");
            }
        }
    }
}
