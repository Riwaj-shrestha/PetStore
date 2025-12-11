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

        // Helper method to hash passwords using SHA256
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // ============================================
        // REGISTER
        // ============================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);

                if (existingUser != null)
                {
                    if (existingUser.Username == model.Username)
                    {
                        ModelState.AddModelError("Username", "Username already exists.");
                    }
                    if (existingUser.Email == model.Email)
                    {
                        ModelState.AddModelError("Email", "Email already exists.");
                    }
                    return View(model);
                }

                // Create new user (without FullName and PhoneNumber since they're not in your ViewModel)
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = HashPassword(model.Password),
                    FullName = model.Username, // Default to username
                    PhoneNumber = "", // Empty for now
                    Role = "Customer", // Default role
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Automatically log in the user after registration
                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserRole", user.Role);

                TempData["Success"] = "Registration successful! Welcome to Pet Store!";
                return RedirectToAction("Index", "Products");
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

        // ============================================
        // LOGIN (WITH ROLE-BASED REDIRECT)
        // ============================================

        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect based on role
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId.HasValue)
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Products");
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find user by username OR email (flexible login)
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Username);

                if (user != null)
                {
                    // Verify password using SHA256
                    var hashedPassword = HashPassword(model.Password);

                    if (user.PasswordHash == hashedPassword)
                    {
                        // Set session variables
                        HttpContext.Session.SetInt32("UserID", user.UserID);
                        HttpContext.Session.SetString("Username", user.Username);
                        HttpContext.Session.SetString("UserRole", user.Role); 

                        // ============================================
                        // ROLE-BASED REDIRECT
                        // ============================================
                        if (user.Role == "Admin")
                        {
                            // Admin users go to admin dashboard
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        else
                        {
                            // Regular customers go to products page
                            return RedirectToAction("Index", "Products");
                        }
                    }
                }

                // If we reach here, login failed
                ModelState.AddModelError("", "Invalid username or password.");
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

        // ============================================
        // LOGOUT
        // ============================================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
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
