using Microsoft.AspNetCore.Mvc;
using Mypro.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Data;


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
            decimal gstPercent = 2m; 
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

            return View(cartFromDb); 
        }

        // Place order - saves order + order items, clears cart
        [HttpPost]
        [HttpPost]
        public IActionResult PlaceOrder(string FullName, string Email, string Phone, string Address, string PaymentMethod)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = User.FindFirstValue(ClaimTypes.Email);

            int newOrderId = 0;
            using (var conn = new SqlConnection(_abc.Database.GetDbConnection().ConnectionString))
            using (var cmd = new SqlCommand("sp_PlaceOrder", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", userId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@FullName", FullName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", Address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PaymentMethod", PaymentMethod ?? (object)DBNull.Value);

                var outParam = new SqlParameter("@NewOrderId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(outParam);

                conn.Open();
                cmd.ExecuteNonQuery();

                newOrderId = (outParam.Value != DBNull.Value) ? (int)outParam.Value : 0;
            }

            if (newOrderId > 0)
            {
                return RedirectToAction("OrderConfirmation", new { id = newOrderId });
            }

            TempData["Error"] = "Could not place order. Please try again.";
            return RedirectToAction("Cart", "Account");
        }


        public IActionResult OrderConfirmation(int id)
        {
            var order = _abc.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Image)
                  .ThenInclude(img => img.Category)
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