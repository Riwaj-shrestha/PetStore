using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetStore.Models;

namespace PetStore.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        private const string CartIdSessionKey = "CartID";

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<int?> GetCartIdAsync()
        {
            return HttpContext.Session.GetInt32(CartIdSessionKey);
        }

        // GET: /Orders/Checkout
        public IActionResult Checkout()
        {
            var model = new CheckoutModel
            {
                Total = 1100m // replace with dynamic total if needed
            };
            return View(model);
        }

        // POST: /Orders/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // --- Clear the cart ---
            var cartId = await GetCartIdAsync();
            if (cartId.HasValue)
            {
                var items = await _context.CartItems
                    .Where(ci => ci.CartID == cartId.Value)
                    .ToListAsync();

                if (items.Any())
                {
                    _context.CartItems.RemoveRange(items);
                    await _context.SaveChangesAsync();
                }

                // Clear session cart
                HttpContext.Session.Remove(CartIdSessionKey);
            }

            // Show pop-up message in view
            ViewBag.OrderSuccess = true;

            return View(model);
        }
    }
}
