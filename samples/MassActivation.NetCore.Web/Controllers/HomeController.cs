using System;
using Microsoft.AspNetCore.Mvc;

namespace MassActivation.NetCore.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var cache = Activator.GetService<MassActivation.Services.ICacheService>();
            ViewBag.ApplicationName = Convert.ToString(cache.Get("ApplicationName"));
            ViewBag.ApplicationVersion = Convert.ToString(cache.Get("ApplicationVersion"));
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
