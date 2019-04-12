using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;


namespace NSmartProxyWinform
{
    static class AutoStartup
    {
        // Don't use Application.ExecutablePath
        // see https://stackoverflow.com/questions/12945805/odd-c-sharp-path-issue
        private static readonly string ExecutablePath = Assembly.GetEntryAssembly().Location;

        private static string Key = "Shadowsocks_" + Application.StartupPath.GetHashCode();

        public static bool Set(bool enabled)
        {
            RegistryKey runKey = null;
            try
            {
                runKey = OpenRegKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (runKey == null)
                {
                    Program.Logger.Error(@"Cannot find HKCU\Software\Microsoft\Windows\CurrentVersion\Run");
                    return false;
                }
                if (enabled)
                {
                    runKey.SetValue(Key, ExecutablePath);
                }
                else
                {
                    runKey.DeleteValue(Key);
                }
                // When autostartup setting change, change RegisterForRestart state to avoid start 2 times
                RegisterForRestart(!enabled);
                return true;
            }
            catch (Exception e)
            {
                Program.Logger.Error(e);
                return false;
            }
            finally
            {
                if (runKey != null)
                {
                    try
                    {
                        runKey.Close();
                        runKey.Dispose();
                    }
                    catch (Exception e)
                    { Program.Logger.Error(e); }
                }
            }
        }

        public static bool Check()
        {
            RegistryKey runKey = null;
            try
            {
                runKey = OpenRegKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (runKey == null)
                {
                    Program.Logger.Error(@"Cannot find HKCU\Software\Microsoft\Windows\CurrentVersion\Run");
                    return false;
                }
                string[] runList = runKey.GetValueNames();
                foreach (string item in runList)
                {
                    if (item.Equals(Key, StringComparison.OrdinalIgnoreCase))
                        return true;
                    else if (item.Equals("Shadowsocks", StringComparison.OrdinalIgnoreCase)) // Compatibility with older versions
                    {
                        string value = Convert.ToString(runKey.GetValue(item));
                        if (ExecutablePath.Equals(value, StringComparison.OrdinalIgnoreCase))
                        {
                            runKey.DeleteValue(item);
                            runKey.SetValue(Key, ExecutablePath);
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Program.Logger.Error(e);
                return false;
            }
            finally
            {
                if (runKey != null)
                {
                    try
                    {
                        runKey.Close();
                        runKey.Dispose();
                    }
                    catch (Exception e)
                    { Program.Logger.Error(e); }
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int RegisterApplicationRestart([MarshalAs(UnmanagedType.LPWStr)] string commandLineArgs, int Flags);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int UnregisterApplicationRestart();

        [Flags]
        enum ApplicationRestartFlags
        {
            RESTART_ALWAYS = 0,
            RESTART_NO_CRASH = 1,
            RESTART_NO_HANG = 2,
            RESTART_NO_PATCH = 4,
            RESTART_NO_REBOOT = 8,
        }

        // register restart after system reboot/update
        public static void RegisterForRestart(bool register)
        {
            // requested register and not autostartup
            if (register && !Check())
            {
                // escape command line parameter
                string[] args = new List<string>(Program.Args)
                    .Select(p => p.Replace("\"", "\\\""))                   // escape " to \"
                    .Select(p => p.IndexOf(" ") >= 0 ? "\"" + p + "\"" : p) // encapsule with "
                    .ToArray();
                string cmdline = string.Join(" ", args);
                // first parameter is process command line parameter
                // needn't include the name of the executable in the command line
                RegisterApplicationRestart(cmdline, (int)(ApplicationRestartFlags.RESTART_NO_CRASH | ApplicationRestartFlags.RESTART_NO_HANG));
                Program.Logger.Debug("Register restart after system reboot, command line:" + cmdline);
            }
            // requested unregister, which has no side effect
            else if (!register)
            {
                UnregisterApplicationRestart();
                Program.Logger.Debug("Unregister restart after system reboot");
            }
        }


        public static RegistryKey OpenRegKey(string name, bool writable, RegistryHive hive = RegistryHive.CurrentUser)
        {
            // we are building x86 binary for both x86 and x64, which will
            // cause problem when opening registry key
            // detect operating system instead of CPU
            if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name));
            try
            {
                RegistryKey userKey = RegistryKey.OpenBaseKey(hive,
                        Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32)
                    .OpenSubKey(name, writable);
                return userKey;
            }
            catch (ArgumentException ae)
            {
                MessageBox.Show("OpenRegKey: " + ae.ToString());
                return null;
            }
            catch (Exception e)
            {
                Program.Logger.Error(e);
                return null;
            }
        }
    }
}
