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
    public class BooksController : Controller
    {
        private BookStoreEntities db = new BookStoreEntities();

        // GET: Admin/Books
        public ActionResult Index()
        {
            ViewBag.BookCount = db.Books.Count();
            var books = db.Books.Include(b => b.Category);
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
        public ActionResult Create([Bind(Include = "BookID,CategoryID,Title,AuthorName,Description,Price,StockQuantity,ImageURL")] Book book)
        {
            if (ModelState.IsValid)
            {
                book.IsActive = true;
                book.CreatedAt = DateTime.Now;
                db.Books.Add(book);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
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
    [Bind(Include = "BookID,CategoryID,Title,AuthorName,Description,Price,StockQuantity,ImageURL,IsActive")] Book book)
        {
            if (ModelState.IsValid)
            {
                Book bookInDatabase = db.Books.Find(book.BookID);

                if (bookInDatabase == null)
                {
                    return HttpNotFound();
                }

                bookInDatabase.CategoryID = book.CategoryID;
                bookInDatabase.Title = book.Title;
                bookInDatabase.AuthorName = book.AuthorName;
                bookInDatabase.Description = book.Description;
                bookInDatabase.Price = book.Price;
                bookInDatabase.StockQuantity = book.StockQuantity;
                bookInDatabase.ImageURL = book.ImageURL;
                bookInDatabase.IsActive = book.IsActive;

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
