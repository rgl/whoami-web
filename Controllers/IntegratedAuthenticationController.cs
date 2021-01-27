using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
            return View(
                new IntegratedAuthenticationModel
                {
                    Whoami = await Whoami.Get(),
                });
        }
    }
}
