using System;
using System.Collections.Generic;

namespace ShopEase.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string ShipAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItem> Items { get; set; }
    }

    public class OrderItem
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ImageURL { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}