using System.Web.Mvc;
using ShopEase.DAL;

namespace ShopEase.Controllers
{
    public class CartController : Controller
    {
        private readonly CartRepository _cartRepo = new CartRepository();

        // GET: /Cart
        public ActionResult Index()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];
            var items = _cartRepo.GetCartItems(userID);
            return View(items);
        }

        // POST: /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int productID, int quantity = 1)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];
            _cartRepo.AddItem(userID, productID, quantity);

            TempData["Success"] = "Item added to cart!";
            return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Index", "Product"));
        }

        // POST: /Cart/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(int cartItemID, int quantity)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

            if (quantity <= 0)
                _cartRepo.RemoveItem(cartItemID);
            else
                _cartRepo.UpdateQuantity(cartItemID, quantity);

            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int cartItemID)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

            _cartRepo.RemoveItem(cartItemID);
            return RedirectToAction("Index");
        }
    }
}