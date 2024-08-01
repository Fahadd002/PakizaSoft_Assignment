using Microsoft.AspNetCore.Mvc;

namespace PakizaSoft_Assignment_Fahad.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
