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

        // ----------------------------- PASSWORD HASH -----------------------------
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // ----------------------------- REGISTER -----------------------------
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
                return View(model);

            // Check duplicates
            var duplicateUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);

            if (duplicateUser != null)
            {
                if (duplicateUser.Username == model.Username)
                    ModelState.AddModelError("Username", "Username already exists.");

                if (duplicateUser.Email == model.Email)
                    ModelState.AddModelError("Email", "Email already exists.");

                return View(model);
            }

            // Create user
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                FullName = model.Username,
                PhoneNumber = "",
                Role = "Customer",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Auto Login
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role);

            TempData["Success"] = "Registration successful!";
            return RedirectToAction("Index", "Products");
        }

        // ----------------------------- LOGIN -----------------------------
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

            // login by username or email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Username);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // verify password
            var hashed = HashPassword(model.Password);
            if (user.PasswordHash != hashed)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // set session
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role);

            // return to requested page
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // role redirect
            if (user.Role == "Admin")
                return RedirectToAction("Dashboard", "Admin");

            return RedirectToAction("Index", "Products");
        }

        // ----------------------------- LOGOUT -----------------------------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Logged out successfully!";
            return RedirectToAction("Login");
        }

        // ----------------------------- PROFILE PAGE (GET) -----------------------------
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.Find(userId);
            if (user == null) return NotFound();

            var vm = new ProfilePageViewModel
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

            return View(vm);
        }

        // ----------------------------- UPDATE PROFILE -----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind(Prefix = "UserProfile")] UserProfileViewModel profile)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            ModelState.Remove("UserProfile.Username"); // username shouldn't change

            if (!ModelState.IsValid)
            {
                return View("Profile", new ProfilePageViewModel
                {
                    UserProfile = profile,
                    ChangePassword = new ChangePasswordViewModel()
                });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FullName = profile.FullName;
            user.Email = profile.Email;
            user.PhoneNumber = profile.PhoneNumber;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated!";
            return RedirectToAction("Profile");
        }

        // ----------------------------- CHANGE PASSWORD -----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([Bind(Prefix = "ChangePassword")] ChangePasswordViewModel pw)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View("Profile", new ProfilePageViewModel
                {
                    UserProfile = new UserProfileViewModel
                    {
                        Username = user.Username,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    },
                    ChangePassword = pw
                });
            }

            // validate current password
            var hashedCurrent = HashPassword(pw.CurrentPassword);
            if (hashedCurrent != user.PasswordHash)
            {
                ModelState.AddModelError("ChangePassword.CurrentPassword", "Incorrect current password.");
                return View("Profile", new ProfilePageViewModel
                {
                    UserProfile = new UserProfileViewModel
                    {
                        Username = user.Username,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    },
                    ChangePassword = pw
                });
            }

            // update new password
            user.PasswordHash = HashPassword(pw.NewPassword);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password updated successfully!";
            return RedirectToAction("Profile");
        }
    }
}
