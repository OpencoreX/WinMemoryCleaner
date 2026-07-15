using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace WinMemoryCleaner
{
    internal static class StartupService
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string RegistryValueName = Constants.App.Name;

        public static bool IsRegistryEnabled()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunKey, false))
                    return key != null && key.GetValue(RegistryValueName) != null;
            }
            catch { return false; }
        }

        public static bool IsTaskEnabled()
        {
            try
            {
                var startInfo = new ProcessStartInfo("schtasks")
                {
                    Arguments = string.Format(CultureInfo.InvariantCulture, @"/Query /TN ""{0}""", Constants.App.Title),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    return process.ExitCode == Constants.Windows.SystemErrorCode.ErrorSuccess;
                }
            }
            catch { return false; }
        }

        public static bool IsEnabled()
        {
            return IsRegistryEnabled() || IsTaskEnabled();
        }

        public static void SetRegistryEnabled(bool enable, string executablePath)
        {
            if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
                return;
            using (var key = Registry.CurrentUser.OpenSubKey(RunKey, true))
            {
                if (key == null) return;
                if (enable)
                    key.SetValue(RegistryValueName, '"' + executablePath + '"');
                else if (key.GetValue(RegistryValueName) != null)
                    key.DeleteValue(RegistryValueName, false);
            }
        }
    }
}