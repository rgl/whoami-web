using CliWrap;
using CliWrap.Buffered;
using System.Threading.Tasks;

namespace whoami
{
    public class Whoami
    {
        public static async Task<string> Get()
        {
            var result = await Cli.Wrap(@"C:\Windows\system32\whoami.exe")
                .WithArguments("-all")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();
            if (result.ExitCode != 0) {
                return $"ERROR: Failed to execute whoami.exe.\nexit code:\n{result.ExitCode}\nstdout:\n{result.StandardOutput}\nstderr:\n{result.StandardError}";
            }
            return result.StandardOutput;
        }
    }
}
