using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PetStore.Models;
using PetStore.Models.ViewModels;

namespace PetStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult About()
        {
            return View();
        }

        // GET: /Home/Contact
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        // POST: /Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Return the view with validation errors
                return View(model);
            }

            // --- PROCESSING LOGIC ---
            // For now, we simulate success.

            TempData["SuccessMessage"] = "Thank you! Your message has been sent. We will reply shortly.";
            
            return RedirectToAction("Contact");
        }

    }
}
