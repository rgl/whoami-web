using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using whoami.Models;

namespace whoami.Controllers
{
    [Authorize]
    public class IntegratedAuthenticationController : Controller
    {
        private readonly ILogger<IntegratedAuthenticationController> _logger;

        public IntegratedAuthenticationController(ILogger<IntegratedAuthenticationController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = (WindowsIdentity)User.Identity;

            var groups = user.Groups.Select(g =>
                    new IntegratedAuthenticationModel.Group
                    {
                        Name = g.Translate(typeof(NTAccount)).ToString(),
                        Sid = g.Value,
                    }
                )
                .OrderBy(g => g.Name)
                .ToArray();

            var whoami = await WindowsIdentity.RunImpersonatedAsync(user.AccessToken, Whoami.Get);

            return View(
                new IntegratedAuthenticationModel
                {
                    Name = user.Name,
                    Sid = user.User.ToString(),
                    ImpersonationLevel = user.ImpersonationLevel.ToString(),
                    Groups = groups,
                    Whoami = whoami,
                });
        }
    }
}
