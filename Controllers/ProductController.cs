using System.Web.Mvc;
using ShopEase.DAL;

namespace ShopEase.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductRepository _productRepo = new ProductRepository();
        private readonly CategoryRepository _categoryRepo = new CategoryRepository();

        // GET: /Product  or  /Product?categoryID=1&search=shirt&sort=price_asc
        public ActionResult Index(int? categoryID, string search, string sort)
        {
            var products = _productRepo.GetAll(categoryID, search, sort);
            var categories = _categoryRepo.GetAll();

            ViewBag.Categories = categories;
            ViewBag.SelectedCat = categoryID;
            ViewBag.Search = search;
            ViewBag.Sort = sort;

            return View(products);
        }

        // GET: /Product/Detail/5
        public ActionResult Detail(int id)
        {
            var product = _productRepo.GetByID(id);
            if (product == null) return HttpNotFound();

            // Get related products from same category
            var related = _productRepo.GetAll(product.CategoryID);
            related.RemoveAll(p => p.ProductID == id);
            if (related.Count > 4) related = related.GetRange(0, 4);

            ViewBag.Related = related;
            return View(product);
        }
    }
}