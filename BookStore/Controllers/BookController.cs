
using System;
using System.Linq;
using System.Web.Mvc;
using BookStore.Models;
using System.Data.Entity;
using System.Linq;

namespace BookStore.Controllers
{
    public class BookController : Controller
    {
        private BookStoreEntities db = new BookStoreEntities();

        // Trang danh sach sach

        public ActionResult BookIndex(
            string keyword,
            int? categoryId,
            string sortOrder,
            int page = 1)
        {
            int pageSize = 20;

            var books = db.Books
                .Where(b => b.IsActive);

            // Tim kiem theo ten sach
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                books = books.Where(b => b.Title.Contains(keyword));
            }

            // Loc theo the loai
            if (categoryId.HasValue)
            {
                books = books.Where(b => b.CategoryID == categoryId.Value);
            }

            // Sap xep sach
            switch (sortOrder)
            {
                case "price_asc":
                    books = books.OrderBy(b => b.Price)
                                 .ThenBy(b => b.BookID);
                    break;

                case "price_desc":
                    books = books.OrderByDescending(b => b.Price)
                                 .ThenBy(b => b.BookID);
                    break;

                default:
                    books = books.OrderByDescending(b => b.CreatedAt)
                                 .ThenByDescending(b => b.BookID);
                    break;
            }

            // Dem tong so sach sau khi loc
            int totalBooks = books.Count();

            int totalPages = (int)Math.Ceiling(
                (double)totalBooks / pageSize);

            // Dam bao so trang hop le
            if (page < 1)
            {
                page = 1;
            }

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            // Lay sach cua trang hien tai
            var bookList = books
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Categories = db.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalBooks = totalBooks;

            return View(bookList);
        }


        // Trang chi tiet sach
        public ActionResult BookDetails(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            var book = db.Books
                .Include(b => b.Category)
                .FirstOrDefault(b => b.BookID == id.Value);

            if (book == null)
            {
                return HttpNotFound();
            }

            return View(book);
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
