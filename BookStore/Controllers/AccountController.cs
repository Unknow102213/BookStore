
using System;
using System.Linq;
using System.Web.Mvc;
using BookStore.Models;

namespace BookStore.Controllers
{
    public class AccountController : Controller
    {
        private BookStoreEntities db = new BookStoreEntities();

        // GET: Account/Login
        public ActionResult Login()
        {
            if (Session["UserID"] != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập email và mật khẩu.";
                return View();
            }

            email = email.Trim();

            User user = db.Users.FirstOrDefault(
                u => u.Email == email
            );

            if (user == null ||
                string.IsNullOrEmpty(user.PasswordHash) ||
                !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Email hoặc mật khẩu không chính xác.";
                return View();
            }

            if (!user.IsActive)
            {
                ViewBag.Error = "Tài khoản đã bị vô hiệu hóa.";
                return View();
            }

            Session["UserID"] = user.UserID;
            Session["UserEmail"] = user.Email;
            Session["FullName"] = user.FullName;
            Session["UserRole"] = user.Role;

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            if (Session["UserID"] != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(
            string fullName,
            string email,
            string password,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            fullName = fullName.Trim();
            email = email.Trim();

            if (fullName.Length > 100)
            {
                ViewBag.Error = "Họ tên không được vượt quá 100 ký tự.";
                return View();
            }

            if (email.Length > 255)
            {
                ViewBag.Error = "Email không được vượt quá 255 ký tự.";
                return View();
            }

            if (password.Length < 8)
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 8 ký tự.";
                return View();
            }

            if (password.Length > 72)
            {
                ViewBag.Error = "Mật khẩu không được vượt quá 72 ký tự.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            bool emailExists = db.Users.Any(
                u => u.Email == email
            );

            if (emailExists)
            {
                ViewBag.Error = "Email này đã được đăng ký.";
                return View();
            }

            User newUser = new User();

            newUser.FullName = fullName;
            newUser.Email = email;
            newUser.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(password);
            newUser.Role = "C";
            newUser.IsActive = true;
            newUser.CreatedAt = DateTime.Now;
            newUser.Phone = null;
            newUser.Address = null;

            db.Users.Add(newUser);
            db.SaveChanges();

            TempData["Success"] =
                "Đăng ký thành công! Bạn có thể đăng nhập.";

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
