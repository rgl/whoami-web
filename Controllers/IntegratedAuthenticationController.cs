using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Principal;
using whoami.Models;

namespace whoami.Controllers
{
    [Authorize]
    public class IntegratedAuthenticationController : Controller
    {
        [SupportedOSPlatform("windows")]
        public IActionResult Index()
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

            // NB WindowsIdentity.RunImpersonated impersonates the user at the
            //    thread level, and as such, we cannot use
            //    System.Diagnostics.Process to spawn processes that run under
            //    the context of the impersonated user. Instead we must use
            //    CreateProcessAsUser (like CliProcess used by Whoami.Get).

            return View(
                new IntegratedAuthenticationModel
                {
                    Name = user.Name,
                    Sid = user.User.ToString(),
                    ImpersonationLevel = user.ImpersonationLevel.ToString(),
                    Groups = groups,
                    Whoami = Whoami.Get(user.AccessToken),
                    EnvironmentVariables = EnvironmentVariables.Get(user.AccessToken),
                });
        }
    }
}
