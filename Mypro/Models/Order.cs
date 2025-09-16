
using System;
using System.Collections.Generic;

namespace Mypro.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }                // you use email as id
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; }

        public decimal SubTotal { get; set; }             // sum of item price * qty
        public decimal OrderDiscountPercent { get; set; } // 5 or 2
        public decimal DateDiscountPercent { get; set; }  // 4/3/2 by date
        public decimal TotalDiscountAmount { get; set; }  // subtotal * (order+date)/100
        public decimal GSTPercent { get; set; }           // 2
        public decimal GSTAmount { get; set; }
        public decimal FinalAmount { get; set; }          // subtotal - totalDiscount + GST
        public string PaymentMethod { get; set; }

        // ✅ Add this new property
        public bool IsCancelled { get; set; } = false;
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
