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
            Order order = db.Orders.Find(id);
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
        public ActionResult Edit(
            [Bind(Include = "OrderID,Status,PaymentStatus,Notes")] Order formOrder)
        {
            if (ModelState.IsValid)
            {
                Order order = db.Orders.Find(formOrder.OrderID);
                if (order == null)
                {
                    return HttpNotFound();
                }
                order.Status = formOrder.Status;
                order.PaymentStatus = formOrder.PaymentStatus;
                order.Notes = formOrder.Notes;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            Order currentOrder = db.Orders.Find(formOrder.OrderID);
            if (currentOrder == null)
            {
                return HttpNotFound();
            }
            formOrder.OrderDate = currentOrder.OrderDate;
            formOrder.TotalAmount = currentOrder.TotalAmount;
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
