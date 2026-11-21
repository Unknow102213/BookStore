
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
        public ActionResult Index(string searchString)
        {
            // Tổng số đánh giá trong hệ thống
            ViewBag.ReviewCount = db.Reviews.Count();

            // Lấy danh sách đánh giá
            var reviews = db.Reviews
                .Include(r => r.Book)
                .Include(r => r.User);

            // Tìm theo mã đánh giá, tên sách hoặc số sao
            if (!String.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                int searchNumber;
                bool isNumber = int.TryParse(searchString, out searchNumber);

                reviews = reviews.Where(r =>
                    (isNumber && r.ReviewID == searchNumber) ||
                    (r.Book != null && r.Book.Title.Contains(searchString)) ||
                    (isNumber && r.Rating == searchNumber)
                );
            }

            // Giữ lại từ khóa tìm kiếm
            ViewBag.SearchString = searchString;

            return View(reviews
                .OrderByDescending(r => r.CreatedAt)
                .ToList());
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
