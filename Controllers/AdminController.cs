/// Author - Swedha - 8995269

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetStore.Models;
using PetStore.Filters;

namespace PetStore.Controllers
{
    /// <summary>
    /// Admin Controller - Admin panel functionality
    /// Uses single login system via AccountController
    /// </summary>
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // LOGOUT (Redirects to Account/Login)
       

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        
        // DASHBOARD
       

        [AdminAuthorize]
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.LowStockProducts = await _context.Products
                .Where(p => p.StockQuantity < 5)
                .CountAsync();

            var recentProducts = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(recentProducts);
        }

       
        // PRODUCT MANAGEMENT
       

        [AdminAuthorize]
        public async Task<IActionResult> Products(string searchTerm, int? categoryId)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(p =>
                    (p.ProductName != null && p.ProductName.ToLower().Contains(searchLower)) ||
                    (p.Breed != null && p.Breed.ToLower().Contains(searchLower)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryID == categoryId.Value);
            }

            var products = await query.OrderBy(p => p.ProductName).ToListAsync();
            var categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SearchTerm = searchTerm ?? "";

            return View(products);
        }

        [AdminAuthorize]
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            // DEBUG: Show all validation errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["Error"] = "Validation failed: " + string.Join(", ", errors);
                ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
                return View(product);
            }

            product.CreatedAt = DateTime.Now;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product created successfully!";
            return RedirectToAction("Products");
        }

        [AdminAuthorize]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public async Task<IActionResult> EditProduct(int id, Product product)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction("Products");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted successfully!";
            }

            return RedirectToAction("Products");
        }

       
        // USER MANAGEMENT
      

        [AdminAuthorize]
        public async Task<IActionResult> Users(string searchTerm, string roleFilter)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(u =>
                    (u.Email != null && u.Email.ToLower().Contains(searchLower)) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(searchLower)) ||
                    (u.Username != null && u.Username.ToLower().Contains(searchLower)));
            }

            if (!string.IsNullOrEmpty(roleFilter))
            {
                query = query.Where(u => u.Role == roleFilter);
            }

            var users = await query.OrderBy(u => u.CreatedAt).ToListAsync();

            ViewBag.SearchTerm = searchTerm ?? "";
            ViewBag.RoleFilter = roleFilter ?? "";

            return View(users);
        }

        [AdminAuthorize]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public async Task<IActionResult> EditUser(int id, string Username, string Email, string FullName, string PhoneNumber, string Role, string newPassword)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                TempData["Error"] = "User not found!";
                return RedirectToAction("Users");
            }

            // Update fields
            existingUser.Username = Username;
            existingUser.Email = Email;
            existingUser.FullName = FullName;
            existingUser.PhoneNumber = PhoneNumber;
            existingUser.Role = Role;

            // Update password if provided
            if (!string.IsNullOrEmpty(newPassword))
            {
                using var sha = System.Security.Cryptography.SHA256.Create();
                var bytes = System.Text.Encoding.UTF8.GetBytes(newPassword);
                var hash = sha.ComputeHash(bytes);
                existingUser.PasswordHash = Convert.ToBase64String(hash);
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "User updated successfully!";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("EditUser", new { id = id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                var currentUserId = HttpContext.Session.GetInt32("UserID");
                if (user.UserID == currentUserId)
                {
                    TempData["Error"] = "You cannot delete your own account!";
                    return RedirectToAction("Users");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "User deleted successfully!";
            }

            return RedirectToAction("Users");
        }

       
        // CATEGORY MANAGEMENT
        

        [AdminAuthorize]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            return View(categories);
        }

        [AdminAuthorize]
        [AdminAuthorize]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            // DEBUG: Show all validation errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["Error"] = "Validation failed: " + string.Join(", ", errors);
                return View(category);
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category created successfully!";
            return RedirectToAction("Categories");
        }

        [AdminAuthorize]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public async Task<IActionResult> EditCategory(int id, Category category)
        {
            if (id != category.CategoryID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Category updated successfully!";
                    return RedirectToAction("Categories");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                var hasProducts = await _context.Products.AnyAsync(p => p.CategoryID == id);
                if (hasProducts)
                {
                    TempData["Error"] = "Cannot delete category with existing products!";
                    return RedirectToAction("Categories");
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category deleted successfully!";
            }

            return RedirectToAction("Categories");
        }

        // HELPER METHODS
       

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryID == id);
        }
    }
}