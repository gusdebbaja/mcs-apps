// HomeController.cs
using Microsoft.AspNetCore.Mvc;

namespace WApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}