
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BookStore.Models;

namespace BookStore.Areas.Admin.Controllers
{
    public class ReviewsController : Controller
    {
        private BookStoreEntities db = new BookStoreEntities();

        // GET: Admin/Reviews
        public ActionResult Index()
        {
            var reviews = db.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
            ViewBag.ReviewCount = db.Reviews.Count();

            return View(reviews);
        }

        // GET: Admin/Reviews/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Review review = db.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefault(r => r.ReviewID == id);

            if (review == null)
            {
                return HttpNotFound();
            }

            return View(review);
        }

        // GET: Admin/Reviews/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Review review = db.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefault(r => r.ReviewID == id);

            if (review == null)
            {
                return HttpNotFound();
            }

            return View(review);
        }

        // POST: Admin/Reviews/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Review review = db.Reviews.Find(id);

            if (review == null)
            {
                return HttpNotFound();
            }

            db.Reviews.Remove(review);
            db.SaveChanges();

            return RedirectToAction("Index");
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
