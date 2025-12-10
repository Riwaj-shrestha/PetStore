using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetStore.Models;
using PetStore.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace PetStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // ------------- REGISTER -------------
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // valid
            bool usernameExists = await _context.Users.AnyAsync(u => u.Username == model.Username);
            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Username is already taken.");
                return View(model);
            }

            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                FullName = model.Username, // Default full name as username
                PhoneNumber = "N/A", // Default phone number as N/A
                Role = "Customer",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // login the user
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Username", user.Username);

            return RedirectToAction("Index", "Home");
        }

        // ------------- LOGIN -------------
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var hashed = HashPassword(model.Password);
            if (user.PasswordHash != hashed)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            // success: login and save session
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Username", user.Username);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // ------------- LOGOUT -------------
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserID");
            HttpContext.Session.Remove("Username");
            return RedirectToAction("Index", "Home");
        }
// ------------- PROFILE (GET) -------------
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.Find(userId);
            if (user == null) return NotFound();

            var viewModel = new ProfilePageViewModel
            {
                UserProfile = new UserProfileViewModel
                {
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                },
                ChangePassword = new ChangePasswordViewModel()
            };

            return View(viewModel);
        }

        // ------------- UPDATE PROFILE (POST) -------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bind(Prefix) matches the inputs generated by asp-for="UserProfile.Property"
        public async Task<IActionResult> UpdateProfile([Bind(Prefix = "UserProfile")] UserProfileViewModel userProfile)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            ModelState.Remove("UserProfile.Username");

            if (!ModelState.IsValid)
            {
                
                var viewModel = new ProfilePageViewModel
                {
                    UserProfile = userProfile,
                    ChangePassword = new ChangePasswordViewModel()
                };
                return View("Profile", new ProfilePageViewModel { UserProfile = userProfile, ChangePassword = new ChangePasswordViewModel() });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FullName = userProfile.FullName;
            user.Email = userProfile.Email;
            user.PhoneNumber = userProfile.PhoneNumber;

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // ------------- CHANGE PASSWORD (POST) -------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([Bind(Prefix = "ChangePassword")] ChangePasswordViewModel passwordModel)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                
                var viewModel = new ProfilePageViewModel
                {
                    UserProfile = new UserProfileViewModel
                    {
                        Username = user.Username,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    },
                    ChangePassword = passwordModel
                };
                return View("Profile", viewModel);
            }

            var hashedCurrent = HashPassword(passwordModel.CurrentPassword);
            if (user.PasswordHash != hashedCurrent)
            {
                ModelState.AddModelError("ChangePassword.CurrentPassword", "Incorrect current password.");
                
                var viewModel = new ProfilePageViewModel
                {
                    UserProfile = new UserProfileViewModel
                    {
                        Username = user.Username,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    },
                    ChangePassword = passwordModel
                };
                return View("Profile", viewModel);
            }

            user.PasswordHash = HashPassword(passwordModel.NewPassword);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your password has been changed successfully!";
            return RedirectToAction("Profile");
        }
    }
}
