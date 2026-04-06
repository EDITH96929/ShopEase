using ShopEase.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ShopEase.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        public string Phone { get; set; }

        // Cart items passed to the view for display
        public List<CartItem> CartItems { get; set; }
        public decimal Total => CartItems == null ? 0 :
            CartItems.Sum(i => i.Price * i.Quantity);
    }
}