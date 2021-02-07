using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;

namespace whoami
{
    [SupportedOSPlatform("windows")]
    public class Whoami
    {
        public static string Get()
        {
            return Get(SafeAccessTokenHandle.InvalidHandle);
        }

        public static string Get(SafeAccessTokenHandle userToken)
        {
            var result = CliProcess.Run(
                userToken,
                @"c:\windows\system32\whoami.exe -all");
            if (result.ExitCode != 0)
            {
                return $"ERROR: Failed to execute whoami.exe.\nexit code:\n{result.ExitCode}\nstdout:\n{result.StandardOutput}\nstderr:\n{result.StandardError}";
            }
            return result.StandardOutput;
        }
    }
}
