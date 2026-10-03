using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaMaquinas.WebApp.Controllers.MVC
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TestController : Controller
    {
        [Route("test")]
        public IActionResult Index()
        {
            return Content("Teste funcionou! O routing está OK.");
        }
    }
}
