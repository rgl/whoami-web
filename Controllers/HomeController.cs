using Microsoft.AspNetCore.Mvc;
using System.Runtime.Versioning;
using whoami.Models;

namespace whoami.Controllers
{
    public class HomeController : Controller
    {
        [SupportedOSPlatform("windows")]
        public IActionResult Index()
        {
            return View(
                new HomeViewModel
                {
                    Whoami = Whoami.Get(),
                    EnvironmentVariables = EnvironmentVariables.Get(),
                });
        }
    }
}
