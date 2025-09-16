using Microsoft.AspNetCore.Mvc;
using Mypro.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Mypro.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _abc;
        public OrderController(ApplicationDbContext context)
        {
            _abc = context;
        }

        // Checkout POST - receives the indexed CartItems list from the cart page
        [HttpPost]
        public IActionResult Checkout(List<CartItem> CartItems)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = User.FindFirstValue(ClaimTypes.Email);

            // get user's cart from DB
            var cartFromDb = _abc.CartItems
                .Include(c => c.Image)
                .ThenInclude(i => i.Category)
                .Where(c => c.UserId == userId)
                .ToList();

            // Update quantities from posted CartItems (model binding expects sequential indexes).
            if (CartItems != null)
            {
                foreach (var posted in CartItems)
                {
                    var dbItem = cartFromDb.FirstOrDefault(c => c.Id == posted.Id);
                    if (dbItem != null)
                    {
                        dbItem.Quantity = posted.Quantity;
                    }
                }

                _abc.SaveChanges();
            }

            // calculate discounts
            bool isFirstOrder = !_abc.Orders.Any(o => o.UserId == userId);
            decimal orderDiscPercent = isFirstOrder ? 5m : 2m;
            int day = DateTime.Now.Day;
            decimal dateDiscPercent = day <= 10 ? 4m : day <= 20 ? 3m : 2m;
            decimal totalDiscountPercent = orderDiscPercent + dateDiscPercent;

            decimal subtotal = cartFromDb.Sum(i => i.Price * i.Quantity);
            decimal totalDiscountAmount = Math.Round(subtotal * totalDiscountPercent / 100m, 2);
            decimal gstPercent = 2m; // as you requested earlier
            decimal gstAmount = Math.Round((subtotal - totalDiscountAmount) * gstPercent / 100m, 2);
            decimal finalAmount = Math.Round(subtotal - totalDiscountAmount + gstAmount, 2);

            ViewBag.OrderDiscountPercent = orderDiscPercent;
            ViewBag.DateDiscountPercent = dateDiscPercent;
            ViewBag.TotalDiscountPercent = totalDiscountPercent;
            ViewBag.SubTotal = subtotal;
            ViewBag.TotalDiscountAmount = totalDiscountAmount;
            ViewBag.GSTPercent = gstPercent;
            ViewBag.GSTAmount = gstAmount;
            ViewBag.FinalAmount = finalAmount;
            ViewBag.IsFirstOrder = isFirstOrder;

            return View(cartFromDb); // Renders Checkout view (billing form + order summary)
        }

        // Place order - saves order + order items, clears cart
        [HttpPost]
        public IActionResult PlaceOrder(string FullName, string Email, string Phone, string Address, string PaymentMethod)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = User.FindFirstValue(ClaimTypes.Email);

            var cart = _abc.CartItems
                .Include(c => c.Image)
                .Where(c => c.UserId == userId)
                .ToList();

            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Cart", "Account");
            }

            // compute discounts same as Checkout
            bool isFirstOrder = !_abc.Orders.Any(o => o.UserId == userId);
            decimal orderDiscPercent = isFirstOrder ? 5m : 2m;
            int day = DateTime.Now.Day;
            decimal dateDiscPercent = day <= 10 ? 4m : day <= 20 ? 3m : 2m;
            decimal totalDiscountPercent = orderDiscPercent + dateDiscPercent;

            decimal subtotal = cart.Sum(i => i.Price * i.Quantity);
            decimal totalDiscountAmount = Math.Round(subtotal * totalDiscountPercent / 100m, 2);
            decimal gstPercent = 2m;
            decimal gstAmount = Math.Round((subtotal - totalDiscountAmount) * gstPercent / 100m, 2);
            decimal finalAmount = Math.Round(subtotal - totalDiscountAmount + gstAmount, 2);

            // create Order
            var order = new Order
            {
                UserId = userId,
                FullName = FullName,
                Email = Email,
                Phone = Phone,
                Address = Address,
                OrderDate = DateTime.UtcNow,
                SubTotal = subtotal,
                OrderDiscountPercent = orderDiscPercent,
                DateDiscountPercent = dateDiscPercent,
                TotalDiscountAmount = totalDiscountAmount,
                GSTPercent = gstPercent,
                GSTAmount = gstAmount,
                FinalAmount = finalAmount,
                PaymentMethod = PaymentMethod
            };

            _abc.Orders.Add(order);
            _abc.SaveChanges(); // so order.Id is populated

            // create OrderItems
            foreach (var ci in cart)
            {
                var oi = new OrderItem
                {
                    OrderId = order.Id,
                    ImageId = ci.ImageId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Price,
                    TotalPrice = Math.Round(ci.Price * ci.Quantity, 2)
                };
                _abc.OrderItems.Add(oi);
            }

            // remove items from cart
            _abc.CartItems.RemoveRange(cart);
            _abc.SaveChanges();

            // redirect to order confirmation page (pass order id)
            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            var order = _abc.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Image)
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();
            TempData["OrderConfirmedId"] = order.Id;
            return View(order);
        }


        public IActionResult MyOrders()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = User.FindFirstValue(ClaimTypes.Email);

            var orders = _abc.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Image)
                        .ThenInclude(i => i.Category)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        [HttpPost]
        public IActionResult CancelOrder(int id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = User.FindFirstValue(ClaimTypes.Email);

            var order = _abc.Orders.FirstOrDefault(o => o.Id == id && o.UserId == userId);
            if (order == null)
                return NotFound();

            // Mark as cancelled
            order.IsCancelled = true;
            _abc.SaveChanges();

            TempData["Success"] = $"Order #{order.Id} has been cancelled.";
            return RedirectToAction("MyOrders");
        }


    }
}