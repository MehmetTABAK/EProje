using System.Diagnostics;
using EProje.Models;
using Microsoft.AspNetCore.Mvc;

namespace EProje.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Home()
        {
            return View();
        }
    }
}
