using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    public class HomesController : Controller
    {
        // GET: Admin/Homes
        private BookStoreEntities db = new BookStoreEntities();

        // GET: Admin/Home
        public ActionResult Index()
        {
            // Tổng số sách
            ViewBag.BookCount = db.Books.Count();

            // Tổng số thể loại
            ViewBag.CategoryCount = db.Categories.Count();

            // Tổng số tài khoản
            ViewBag.UserCount = db.Users.Count();

            // Tổng số đơn hàng
            ViewBag.OrderCount = db.Orders.Count();

            // Tổng doanh thu từ đơn đã thanh toán
            ViewBag.TotalRevenue = db.Orders
                .Where(o => o.PaymentStatus == "Đã thanh toán")
                .Select(o => (decimal?)o.TotalAmount)
                .Sum() ?? 0m;

            return View();
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