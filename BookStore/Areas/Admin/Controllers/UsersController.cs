using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookStore.Models;

namespace BookStore.Areas.Admin.Controllers
{
    public class UsersController : Controller
    {
        private BookStoreEntities db = new BookStoreEntities();
        private bool IsMainAdmin()
        {
            return Session["UserID"] != null
                && Convert.ToInt32(Session["UserID"]) == 1;
        }
        // GET: Admin/Users
        // GET: Admin/Users
        public ActionResult Index(string role, string searchString)
        {
            // Thống kê tài khoản
            ViewBag.UserCount = db.Users.Count();
            ViewBag.CustomerCount = db.Users.Count(u => u.Role == "C");
            ViewBag.AdminCount = db.Users.Count(u => u.Role == "A");
            ViewBag.IsMainAdmin = IsMainAdmin();

            // Lấy danh sách tài khoản
            var users = db.Users.AsQueryable();

            // Lọc theo vai trò
            if (role == "C" || role == "A")
            {
                users = users.Where(u => u.Role == role);
            }

            // Tìm theo mã tài khoản, họ tên hoặc email
            if (!String.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                int userId;
                bool isNumber = int.TryParse(searchString, out userId);

                users = users.Where(u =>
                    (isNumber && u.UserID == userId) ||
                    (u.FullName != null && u.FullName.Contains(searchString)) ||
                    (u.Email != null && u.Email.Contains(searchString))
                );
            }

            // Giữ lại thông tin tìm kiếm và tab đang chọn
            ViewBag.CurrentRole = role;
            ViewBag.SearchString = searchString;

            users = users.OrderBy(u => u.UserID);

            return View(users.ToList());
        }

        // GET: Admin/Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }


        /* GET: Admin/Users/Edit/5 */
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Chỉ tài khoản ID 1 được chỉnh sửa
            if (!IsMainAdmin() || id == 1)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            User user = db.Users.Find(id);

            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        /* POST: Admin/Users/Edit/5 */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
        [Bind(Include = "UserID,Email,Role,IsActive,FullName,Phone,Address")] User formUser)
        {
            // Kiểm tra quyền chỉnh sửa
            if (!IsMainAdmin() || formUser.UserID == 1)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                User user = db.Users.Find(formUser.UserID);

                if (user == null)
                    return HttpNotFound();

                user.Email = formUser.Email;
                user.Role = formUser.Role;
                user.IsActive = formUser.IsActive;
                user.FullName = formUser.FullName;
                user.Phone = formUser.Phone;
                user.Address = formUser.Address;

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(formUser);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
