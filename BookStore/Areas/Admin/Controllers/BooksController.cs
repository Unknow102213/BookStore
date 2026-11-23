using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookStore.Models;
using System.IO;

namespace BookStore.Areas.Admin.Controllers
{
    public class BooksController : Controller
    {
        private BookStoreEntities db = new BookStoreEntities();        

        // GET: Admin/Books
        public ActionResult Index(string searchString)
        {
            // Tổng số sách trong hệ thống
            ViewBag.BookCount = db.Books.Count();

            // Lấy danh sách sách
            var books = db.Books.Include(b => b.Category);

            // Tìm theo tên sách hoặc tên tác giả
            if (!String.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                books = books.Where(b =>
                    b.Title.Contains(searchString) ||
                    (b.AuthorName != null && b.AuthorName.Contains(searchString))
                );
            }

            // Giữ lại từ khóa tìm kiếm trên giao diện
            ViewBag.SearchString = searchString;

            return View(books.ToList());
        }


        // GET: Admin/Books/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Include(b => b.Category).FirstOrDefault(b => b.BookID == id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // Kiểm tra định dạng và dung lượng ảnh
        private bool IsValidImage(HttpPostedFileBase imageFile)
        {
            if (imageFile == null || imageFile.ContentLength == 0)
                return false;

            string extension = Path.GetExtension(imageFile.FileName)
                .ToLower();

            string[] allowedExtensions =
            {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

            if (!allowedExtensions.Contains(extension))
                return false;

            // Giới hạn dung lượng 5 MB
            if (imageFile.ContentLength > 5 * 1024 * 1024)
                return false;

            return true;
        }

        // Lưu ảnh vào thư mục Books
        private string SaveBookImage(HttpPostedFileBase imageFile)
        {
            string folderPath = Server.MapPath("~/Content/img/");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string extension = Path.GetExtension(imageFile.FileName).ToLower();
            string fileName = Guid.NewGuid().ToString() + extension;
            string filePath = Path.Combine(folderPath, fileName);

            imageFile.SaveAs(filePath);

            return "~/Content/img/" + fileName;
        }

        // GET: Admin/Books/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Admin/Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "BookID,CategoryID,Title,AuthorName,Description,Price,StockQuantity")] Book book,
            HttpPostedFileBase imageFile)
        {
            // Bắt buộc chọn ảnh khi thêm sách
            if (imageFile == null || imageFile.ContentLength == 0)
            {
                ModelState.AddModelError("imageFile", "Vui lòng chọn ảnh bìa sách.");
            }
            else if (!IsValidImage(imageFile))
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Chỉ chấp nhận ảnh JPG, JPEG, PNG, WEBP và dung lượng tối đa 5 MB."
                );
            }

            // Không kiểm tra ImageURL vì đường dẫn được tạo sau khi tải ảnh lên
            ModelState.Remove("ImageURL");

            if (ModelState.IsValid)
            {
                book.ImageURL = SaveBookImage(imageFile);
                book.IsActive = true;
                book.CreatedAt = DateTime.Now;

                db.Books.Add(book);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName",
                book.CategoryID
            );

            return View(book);
        }


        // GET: Admin/Books/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
            return View(book);
        }

        // POST: Admin/Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "BookID,CategoryID,Title,AuthorName,Description,Price,StockQuantity,IsActive")] Book book,
            HttpPostedFileBase imageFile)
        {
            Book bookInDatabase = db.Books.Find(book.BookID);

            if (bookInDatabase == null)
            {
                return HttpNotFound();
            }

            // Chỉ kiểm tra ảnh khi Admin chọn ảnh mới
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                if (!IsValidImage(imageFile))
                {
                    ModelState.AddModelError(
                        "imageFile",
                        "Chỉ chấp nhận ảnh JPG, JPEG, PNG, WEBP và dung lượng tối đa 5 MB."
                    );
                }
            }

            ModelState.Remove("ImageURL");

            if (ModelState.IsValid)
            {
                bookInDatabase.CategoryID = book.CategoryID;
                bookInDatabase.Title = book.Title;
                bookInDatabase.AuthorName = book.AuthorName;
                bookInDatabase.Description = book.Description;
                bookInDatabase.Price = book.Price;
                bookInDatabase.StockQuantity = book.StockQuantity;
                bookInDatabase.IsActive = book.IsActive;

                // Chỉ cập nhật ảnh nếu Admin chọn ảnh mới
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    bookInDatabase.ImageURL = SaveBookImage(imageFile);
                }

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName",
                book.CategoryID
            );

            return View(book);
        }

        // GET: Admin/Books/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Admin/Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);

            if (book == null)
            {
                return HttpNotFound();
            }

            db.Books.Remove(book);
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
