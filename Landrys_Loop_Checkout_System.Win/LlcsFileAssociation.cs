using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Landrys_Loop_Checkout_System.Win
{
    public static class LlcsFileAssociation
    {
        public const string ProgId = "LandrysLoopCheckout.Job";
        public const string Extension = ".llcs";
        public const string FileDescription = "Landry Loop Checkout Job";

        public static void EnsureRegistered()
        {
            string exePath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            {
                return;
            }

            string command = "\"" + exePath + "\" \"%1\"";
            string icon = "\"" + exePath + "\",0";

            using (RegistryKey classes = Registry.CurrentUser.CreateSubKey(@"Software\Classes"))
            {
                using (RegistryKey ext = classes.CreateSubKey(Extension))
                {
                    ext.SetValue(null, ProgId);
                }

                using (RegistryKey progId = classes.CreateSubKey(ProgId))
                {
                    progId.SetValue(null, FileDescription);
                    using (RegistryKey iconKey = progId.CreateSubKey("DefaultIcon"))
                    {
                        iconKey.SetValue(null, icon);
                    }
                    using (RegistryKey commandKey = progId.CreateSubKey(@"shell\open\command"))
                    {
                        commandKey.SetValue(null, command);
                    }
                }
            }

            NativeMethods.SHChangeNotify(NativeMethods.SHCNE_ASSOCCHANGED, NativeMethods.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        private static class NativeMethods
        {
            public const uint SHCNE_ASSOCCHANGED = 0x08000000;
            public const uint SHCNF_IDLIST = 0x0000;

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
        }
    }
}
