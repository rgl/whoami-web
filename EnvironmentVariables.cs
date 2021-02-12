using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;

namespace whoami
{
    [SupportedOSPlatform("windows")]
    public class EnvironmentVariables
    {
        public static string Get()
        {
            return Get(SafeAccessTokenHandle.InvalidHandle);
        }

        public static string Get(SafeAccessTokenHandle userToken)
        {
            var result = CliProcess.Run(
                userToken,
                @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe 
                    -NoLogo
                    -NoProfile
                    -ExecutionPolicy Bypass
                    -Command ""dir env: | Sort-Object Name,Value | Format-Table -AutoSize -Wrap | Out-String -Width 160""");
            if (result.ExitCode != 0)
            {
                return $"ERROR: Failed to execute powershell.exe to get the environment variables.\nexit code:\n{result.ExitCode}\nstdout:\n{result.StandardOutput}\nstderr:\n{result.StandardError}";
            }
            return result.StandardOutput;
        }
    }
}
