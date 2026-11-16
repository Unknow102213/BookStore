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
    public class OrdersController : Controller
    {
        private BookStoreEntities db = new BookStoreEntities();

        // GET: Admin/Orders
        public ActionResult Index()
        {
            ViewBag.OrderCount = db.Orders.Count();
            var orders = db.Orders.Include(o => o.User);
            return View(orders.ToList());
        }

        // GET: Admin/Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Order order = db.Orders
                .Include(o => o.OrderDetails.Select(od => od.Book))
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // GET: Admin/Orders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.Users, "UserID", "Email", order.UserID);
            return View(order);
        }

        // POST: Admin/Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderID,Status,Notes")] Order formOrder)
        {
            Order order = db.Orders.Find(formOrder.OrderID);

            if (order == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                string currentStatus = order.Status;
                string newStatus = formOrder.Status;

                // Kiểm tra trạng thái được phép chuyển
                bool isValidTransition = false;

                if (currentStatus == "Chờ xác nhận" &&
                    (newStatus == "Đang chuẩn bị" || newStatus == "Đã hủy"))
                {
                    isValidTransition = true;
                }
                else if (currentStatus == "Đang chuẩn bị" &&
                    (newStatus == "Đang giao hàng" || newStatus == "Đã hủy"))
                {
                    isValidTransition = true;
                }
                else if (currentStatus == "Đang giao hàng" &&
                    newStatus == "Đã nhận hàng")
                {
                    isValidTransition = true;
                }                

                if (!isValidTransition)
                {
                    ModelState.AddModelError("Status",
                        "Trạng thái chuyển đổi không hợp lệ.");

                    formOrder.OrderDate = order.OrderDate;
                    formOrder.TotalAmount = order.TotalAmount;

                    return View(formOrder);
                }

                // Cập nhật trạng thái đơn hàng
                order.Status = newStatus;
                order.Notes = formOrder.Notes;

                // Tự động cập nhật trạng thái thanh toán
                if (newStatus == "Đã nhận hàng" &&
                    order.PaymentStatus == "Chưa thanh toán")
                {
                    order.PaymentStatus = "Đã thanh toán";
                }                

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            formOrder.OrderDate = order.OrderDate;
            formOrder.TotalAmount = order.TotalAmount;

            return View(formOrder);
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
