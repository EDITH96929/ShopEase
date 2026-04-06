using System;
using System.Web.Mvc;
using ShopEase.DAL;
using ShopEase.Models;
using ShopEase.Models.ViewModels;

namespace ShopEase.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderRepository _orderRepo = new OrderRepository();
        private readonly CartRepository _cartRepo = new CartRepository();

        // GET: /Order/Checkout
        public ActionResult Checkout()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];
            var cartItems = _cartRepo.GetCartItems(userID);

            if (cartItems.Count == 0)
                return RedirectToAction("Index", "Cart");

            var model = new CheckoutViewModel
            {
                FullName = Session["FullName"]?.ToString(),
                CartItems = cartItems
            };

            return View(model);
        }

        // POST: /Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutViewModel model)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];
            var cartItems = _cartRepo.GetCartItems(userID);

            // Re-attach cart items (they aren't posted back from form)
            model.CartItems = cartItems;

            if (!ModelState.IsValid)
                return View(model);

            if (cartItems.Count == 0)
            {
                ModelState.AddModelError("", "Your cart is empty.");
                return View(model);
            }

            var order = new Order
            {
                UserID = userID,
                TotalAmount = model.Total,
                ShipAddress = $"{model.FullName}, {model.Address}, {model.City} — Ph: {model.Phone}"
            };

            try
            {
                int orderID = _orderRepo.PlaceOrder(order, cartItems);
                return RedirectToAction("Confirmation", new { id = orderID });
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Something went wrong placing your order. Please try again.");
                return View(model);
            }
        }

        // GET: /Order/Confirmation/5
        public ActionResult Confirmation(int? id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            if (id == null) return RedirectToAction("History");

            int userID = (int)Session["UserID"];
            var order = _orderRepo.GetOrderByID(id.Value, userID);

            if (order == null) return HttpNotFound();
            return View(order);
        }

        // GET: /Order/History
        public ActionResult History()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];
            var orders = _orderRepo.GetOrdersByUser(userID);
            return View(orders);
        }

        // GET: /Order/Detail/5
        public ActionResult Detail(int? id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            if (id == null) return RedirectToAction("History");

            int userID = (int)Session["UserID"];
            var order = _orderRepo.GetOrderByID(id.Value, userID);

            if (order == null) return HttpNotFound();
            return View(order);
        }
    }
}