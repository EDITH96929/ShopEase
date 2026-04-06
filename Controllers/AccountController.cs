using System.Web.Mvc;
using ShopEase.DAL;
using ShopEase.Models;
using ShopEase.Models.ViewModels;

namespace ShopEase.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepo = new UserRepository();

        public ActionResult Login()
        {
            if (Session["UserID"] != null) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            User user = _userRepo.Login(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            Session["UserID"] = user.UserID;
            Session["FullName"] = user.FullName;
            Session["Role"] = user.Role;

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            if (Session["UserID"] != null) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = model.Password,
                Phone = model.Phone,
                Address = model.Address
            };

            bool success = _userRepo.Register(user);
            if (!success)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            TempData["Success"] = "Account created! Please login.";
            return RedirectToAction("Login");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}