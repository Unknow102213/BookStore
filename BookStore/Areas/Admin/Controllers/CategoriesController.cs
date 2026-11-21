
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BookStore.Models;

namespace BookStore.Areas.Admin.Controllers
{
    public class CategoriesController : Controller
    {
        private BookStoreEntities db = new BookStoreEntities();

        // GET: Admin/Categories
        public ActionResult Index(string searchString)
        {
            // Tổng số thể loại trong database
            ViewBag.CategoryCount = db.Categories.Count();

            // Lấy danh sách thể loại
            var categories = db.Categories.AsQueryable();

            // Tìm kiếm theo tên thể loại
            if (!String.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                categories = categories.Where(c =>
                    c.CategoryName.Contains(searchString));
            }

            // Giữ lại từ khóa tìm kiếm
            ViewBag.SearchString = searchString;

            return View(categories.ToList());
        }


        // GET: Admin/Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "CategoryName,IsActive")] Category category)
        {                        
            // Bỏ qua kiểm tra bắt buộc của trường Description
            ModelState.Remove("Description");

            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Thêm thể loại thành công.";

                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra thể loại có sách hay không
            bool hasBooks = db.Books.Any(
                b => b.CategoryID == id);

            if (hasBooks)
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa thể loại này vì vẫn còn sách thuộc thể loại. " +
                    "Vui lòng chuyển các sách sang thể loại khác trước.";

                return RedirectToAction("Index");
            }

            db.Categories.Remove(category);
            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Xóa thể loại thành công.";

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
