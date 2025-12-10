using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetStore.Models;

namespace PetStore.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private const string CartIdSessionKey = "CartID";

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<int> GetOrCreateCartIdAsync(int userId)
        {
            // 1. 先看 Session 里有没有 CartID
            int? cartId = HttpContext.Session.GetInt32(CartIdSessionKey);
            if (cartId.HasValue)
            {
                return cartId.Value;
            }

            // 2. 看数据库里是否已有这个用户的购物车
            var existingCart = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (existingCart != null)
            {
                HttpContext.Session.SetInt32(CartIdSessionKey, existingCart.CartID);
                return existingCart.CartID;
            }

            // 3. 没有就新建一个
            var cart = new ShoppingCart
            {
                UserID = userId,
                CreatedAt = DateTime.Now
            };

            _context.ShoppingCarts.Add(cart);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32(CartIdSessionKey, cart.CartID);
            return cart.CartID;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (!userId.HasValue)
            {
                // 未登录，跳到登录页
                return RedirectToAction("Login", "Account", new
                {
                    returnUrl = Url.Action("Index", "Cart")
                });
            }

            var cartId = await GetOrCreateCartIdAsync(userId.Value);

            var items = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartID == cartId)
                .ToListAsync();

            var total = items.Sum(i => i.Product.Price * i.Quantity);
            ViewBag.CartTotal = total;

            return View(items);
        }

        // POST: /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (!userId.HasValue)
            {
                // 未登录就先让他登录，登录后再回到原页面
                return RedirectToAction("Login", "Account", new
                {
                    returnUrl = Url.Action("Index", "Products")
                });
            }

            if (quantity < 1) quantity = 1;

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            var cartId = await GetOrCreateCartIdAsync(userId.Value);

            var existing = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartID == cartId && ci.ProductID == productId);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartID = cartId,
                    ProductID = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        // UPDATE QUANTITY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var cartId = await GetOrCreateCartIdAsync(userId.Value);

            var item = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.CartItemID == cartItemId && ci.CartID == cartId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    _context.CartItems.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // REMOVE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var cartId = await GetOrCreateCartIdAsync(userId.Value);

            var item = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartItemID == cartItemId && ci.CartID == cartId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // CLEAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var cartId = await GetOrCreateCartIdAsync(userId.Value);

            var items = await _context.CartItems
                .Where(ci => ci.CartID == cartId)
                .ToListAsync();

            if (items.Any())
            {
                _context.CartItems.RemoveRange(items);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
