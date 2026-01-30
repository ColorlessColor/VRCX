using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace VRCX.Core
{
    public static class WinApi
    {
        private static List<(List<string>, string, string)> CpuErrorMessages = new List<(List<string>, string, string)>()
        {
            (["Intel", "Core", "-13"], "VRCX has detected that you're using a 13th or 14th generation Intel CPU.\nThese CPUs are known to have issues which can lead to crashes.\nThis crash was unlikely caused by VRCX itself, therefore limited support can be offered.\nWould you like to open a link with more information?", "https://alderongames.com/intel-crashes"),
            (["Intel", "Core", "-14"], "VRCX has detected that you're using a 13th or 14th generation Intel CPU.\nThese CPUs are known to have issues which can lead to crashes.\nThis crash was unlikely caused by VRCX itself, therefore limited support can be offered.\nWould you like to open a link with more information?", "https://alderongames.com/intel-crashes"),
        };

        internal static string GetCpuName()
        {
            return Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0\")?.GetValue("ProcessorNameString").ToString() ?? null;
        }

        internal static (string, string)? GetCpuErrorMessage()
        {
            string cpuName = GetCpuName();
            if (cpuName == null)
                return null;

            foreach (var errorInfo in CpuErrorMessages)
            {
                if (errorInfo.Item1.All(cpuName.Contains))
                    return (errorInfo.Item2, errorInfo.Item3);
            }

            return null;
        }
    }
}
