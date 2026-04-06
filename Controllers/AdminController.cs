using System.Web.Mvc;
using ShopEase.DAL;
using ShopEase.Models;

namespace ShopEase.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminRepository _adminRepo = new AdminRepository();
        private readonly ProductRepository _productRepo = new ProductRepository();
        private readonly CategoryRepository _categoryRepo = new CategoryRepository();
        private readonly OrderRepository _orderRepo = new OrderRepository();

        // Shared admin auth check
        private bool IsAdmin()
        {
            return Session["Role"] != null && Session["Role"].ToString() == "Admin";
        }

        // GET: /Admin
        public ActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.TotalProducts = _adminRepo.GetTotalProducts();
            ViewBag.TotalOrders = _adminRepo.GetTotalOrders();
            ViewBag.TotalUsers = _adminRepo.GetTotalUsers();
            ViewBag.TotalRevenue = _adminRepo.GetTotalRevenue();
            ViewBag.RecentOrders = _adminRepo.GetRecentOrders();

            return View();
        }

        // ── PRODUCTS ──────────────────────────────────────────

        // GET: /Admin/Products
        public ActionResult Products()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var products = _productRepo.GetAll();
            return View(products);
        }

        // GET: /Admin/CreateProduct
        public ActionResult CreateProduct()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.Categories = _categoryRepo.GetAll();
            return View(new Product());
        }

        // POST: /Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProduct(Product product, System.Web.HttpPostedFileBase imageFile)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Handle image upload
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string fileName = System.IO.Path.GetFileName(imageFile.FileName);
                string savePath = System.IO.Path.Combine(Server.MapPath("~/Images/Products/"), fileName);
                imageFile.SaveAs(savePath);
                product.ImageURL = "/Images/Products/" + fileName;
            }
            else
            {
                // Fallback placeholder if no image uploaded
                product.ImageURL = "https://placehold.co/400x400/f5f5f5/999?text=No+Image";
            }

            if (ModelState.IsValid)
            {
                _productRepo.Insert(product);
                TempData["Success"] = "Product added successfully.";
                return RedirectToAction("Products");
            }

            ViewBag.Categories = _categoryRepo.GetAll();
            return View(product);
        }

        // GET: /Admin/EditProduct/5
        public ActionResult EditProduct(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var product = _productRepo.GetByID(id);
            if (product == null) return HttpNotFound();

            ViewBag.Categories = _categoryRepo.GetAll();
            return View(product);
        }

        // POST: /Admin/EditProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(Product product, System.Web.HttpPostedFileBase imageFile)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Only update image if a new one was uploaded
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string fileName = System.IO.Path.GetFileName(imageFile.FileName);
                string savePath = System.IO.Path.Combine(Server.MapPath("~/Images/Products/"), fileName);
                imageFile.SaveAs(savePath);
                product.ImageURL = "/Images/Products/" + fileName;
            }
            else
            {
                // Keep existing image URL — fetch it from DB
                var existing = _productRepo.GetByID(product.ProductID);
                product.ImageURL = existing?.ImageURL ?? "";
            }

            if (ModelState.IsValid)
            {
                _productRepo.Update(product);
                TempData["Success"] = "Product updated.";
                return RedirectToAction("Products");
            }

            ViewBag.Categories = _categoryRepo.GetAll();
            return View(product);
        }

        // POST: /Admin/DeleteProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProduct(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            _productRepo.Delete(id);
            TempData["Success"] = "Product removed.";
            return RedirectToAction("Products");
        }

        // ── ORDERS ────────────────────────────────────────────

        // GET: /Admin/Orders
        public ActionResult Orders()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var orders = _orderRepo.GetAllOrders();
            return View(orders);
        }

        // POST: /Admin/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderStatus(int orderID, string status)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            _orderRepo.UpdateStatus(orderID, status);
            TempData["Success"] = "Order status updated.";
            return RedirectToAction("Orders");
        }

        // ── CATEGORIES ────────────────────────────────────────

        // GET: /Admin/Categories
        public ActionResult Categories()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var cats = _categoryRepo.GetAll();
            return View(cats);
        }
    }
}