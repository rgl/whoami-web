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
                .ExecuteBufferedAsync();
            return result.StandardOutput;
        }
    }
}
