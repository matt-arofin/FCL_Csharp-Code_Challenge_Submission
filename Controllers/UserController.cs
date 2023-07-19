using Microsoft.AspNetCore.Mvc;

namespace Csharp_Code_ChallengeSubmission.Controllers
{
    public class HomeController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
    }
}

