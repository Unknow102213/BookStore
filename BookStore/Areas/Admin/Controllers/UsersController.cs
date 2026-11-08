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

        // GET: Admin/Users
        public ActionResult Index()
        {
            ViewBag.UserCount = db.Users.Count();
            var users = db.Users.OrderBy(u => u.UserID).ToList();
            return View(users);
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

        /* POST: Admin/Users/Edit/5 */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "UserID,Email,Role,IsActive,FullName,Phone,Address")] User formUser)
        {
            if (ModelState.IsValid)
            {
                User user = db.Users.Find(formUser.UserID);

                if (user == null)
                {
                    return HttpNotFound();
                }

                // Chỉ cập nhật những trường được phép sửa
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
