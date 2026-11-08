using System;
using System.Linq;
using System.Web.Mvc;
using BookStore.Models;
using BookStore.ViewModels;

namespace BookStore.Controllers
{
    public class AccountController : Controller
    {
        private BookStoreEntities db = new BookStoreEntities();

        // GET: Account/Login
        public ActionResult Login()
        {
            if (Session["UserID"] != null)
            {
                if (Session["UserRole"]?.ToString() == "A")
                    return RedirectToAction("Index", "Books", new { area = "Admin" });

                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string email = model.Email.Trim();

            User user = db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null ||
                string.IsNullOrEmpty(user.PasswordHash) ||
                !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ViewBag.Error = "Email hoặc mật khẩu không chính xác.";
                return View(model);
            }

            if (!user.IsActive)
            {
                ViewBag.Error = "Tài khoản đã bị vô hiệu hóa.";
                return View(model);
            }

            Session["UserID"] = user.UserID;
            Session["UserEmail"] = user.Email;
            Session["FullName"] = user.FullName;
            Session["UserRole"] = user.Role;

            if (user.Role == "A")
                return RedirectToAction("Index", "Books", new { area = "Admin" });

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            if (Session["UserID"] != null)
                return RedirectToAction("Index", "Home");

            return View(new RegisterViewModel());
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string email = model.Email.Trim();

            bool emailExists = db.Users.Any(u => u.Email == email);

            if (emailExists)
            {
                ModelState.AddModelError("Email",
                    "Email này đã được đăng ký.");
                return View(model);
            }

            User newUser = new User
            {
                FullName = model.FullName.Trim(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "C",
                IsActive = true,
                CreatedAt = DateTime.Now,
                Phone = null,
                Address = null
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            TempData["Success"] = "Đăng ký thành công! Bạn có thể đăng nhập.";

            return RedirectToAction("Login");
        }

        // GET: Account/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Vui lòng nhập email.";
                return View();
            }

            email = email.Trim();

            User user = db.Users.FirstOrDefault(
                u => u.Email == email
            );

            if (user == null)
            {
                ViewBag.Error = "Email không tồn tại trong hệ thống.";
                return View();
            }

            TempData["Success"] =
                "Email hợp lệ. Chức năng đặt lại mật khẩu sẽ được bổ sung sau.";

            return RedirectToAction("ForgotPassword");
        }

        // GET: Account/MyProfile
        // GET: Account/MyProfile
        public ActionResult MyProfile()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login");

            int userID = (int)Session["UserID"];

            User user = db.Users.Find(userID);

            if (user == null)
            {
                Session.Clear();
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }                
    }
}
