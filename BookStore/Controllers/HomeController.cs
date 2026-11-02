using BookStore.Models;
using BookStore.ViewModels;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace BookStore.Controllers
{
    public class HomeController : Controller
    {
        private BookStoreEntities db = new BookStoreEntities();

        public ActionResult Index()
        {
            // Lấy sách đang hoạt động
            var books = db.Books
                .Where(b => b.IsActive)
                .AsQueryable();

            // Sách nổi bật: điểm đánh giá trung bình cao
            var featuredBooks = books
                .OrderByDescending(b =>
                    b.Reviews.Any()
                    ? b.Reviews.Average(r => (double?)r.Rating) ?? 0
                    : 0)
                .Take(10)
                .ToList();

            // Sách đề cử: có nhiều lượt đánh giá
            var recommendedBooks = books
                .OrderByDescending(b => b.Reviews.Count())
                .Take(10)
                .ToList();

            // Sách mới cập nhật
            var newBooks = books
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .ToList();

            // Sách bán chạy
            var bestSellingBooks = db.OrderDetails
                .Where(od => od.Order.Status == "Đã nhận hàng"
                          && od.Book.IsActive)
                .GroupBy(od => od.BookID)
                .Select(g => new
                {
                    BookID = g.Key,
                    TotalSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .Join(books,
                    sold => sold.BookID,
                    book => book.BookID,
                    (sold, book) => book)
                .ToList();

            // Chỉ lấy thể loại có ít nhất một sách đang hoạt động
            var categories = db.Categories
                .Include(c => c.Books)
                .Where(c => c.Books.Any(b => b.IsActive))
                .OrderBy(c => c.CategoryName)
                .ToList();

            HomeViewModel model = new HomeViewModel
            {
                FeaturedBooks = featuredBooks,
                RecommendedBooks = recommendedBooks,
                BestSellingBooks = bestSellingBooks,
                NewBooks = newBooks,
                Categories = categories
            };

            return View(model);
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