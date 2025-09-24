using System;
using System.Collections.Generic;
using System.Linq;
using Mypro.Models;

namespace Mypro.Services
{
    public class OrderCalculationService
    {
        public (decimal SubTotal, decimal OrderDiscPercent, decimal DateDiscPercent,
                decimal TotalDiscountAmount, decimal GSTPercent, decimal GSTAmount, decimal FinalAmount)
            CalculateOrderAmounts(List<CartItem> cart, bool isFirstOrder)
        {
            decimal subtotal = cart.Sum(i => i.Price * i.Quantity);
            decimal orderDiscPercent = isFirstOrder ? 5m : 2m;
            int day = DateTime.Now.Day;
            decimal dateDiscPercent = day <= 10 ? 4m : day <= 20 ? 3m : 2m;
            decimal totalDiscountPercent = orderDiscPercent + dateDiscPercent;

            decimal totalDiscountAmount = Math.Round(subtotal * totalDiscountPercent / 100m, 2);
            decimal gstPercent = 2m;
            decimal gstAmount = Math.Round((subtotal - totalDiscountAmount) * gstPercent / 100m, 2);
            decimal finalAmount = Math.Round(subtotal - totalDiscountAmount + gstAmount, 2);

            return (subtotal, orderDiscPercent, dateDiscPercent, totalDiscountAmount,
                    gstPercent, gstAmount, finalAmount);
        }
    }
}
