using Mypro.Models;

namespace Mypro.Controllers
{
    internal class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public Customer Customer { get; set; }
    }
}