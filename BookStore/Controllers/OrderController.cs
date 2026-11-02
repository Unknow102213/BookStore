using BookStore.Models;
using BookStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace BookStore.Controllers
{
    public class OrderController : Controller
    {
        private BookStoreEntities db = new BookStoreEntities();

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(List<int> selectedCartItemIDs)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            if (selectedCartItemIDs == null || !selectedCartItemIDs.Any())
            {
                TempData["CartError"] = "Vui lòng chọn ít nhất một cuốn sách.";
                return RedirectToAction("CartIndex", "Cart");
            }

            int userID = (int)Session["UserID"];

            var selectedIDs = selectedCartItemIDs.Distinct().ToList();

            var cartItems = db.CartItems
                .Include(ci => ci.Book)
                .Include(ci => ci.Cart)
                .Where(ci =>
                    selectedIDs.Contains(ci.CartItemID) &&
                    ci.Cart.UserID == userID)
                .ToList();

            if (cartItems.Count != selectedIDs.Count || !cartItems.Any())
            {
                TempData["CartError"] = "Giỏ hàng đã thay đổi. Vui lòng kiểm tra lại.";
                return RedirectToAction("CartIndex", "Cart");
            }

            foreach (var item in cartItems)
            {
                if (!item.Book.IsActive ||
                    item.Quantity <= 0 ||
                    item.Quantity > item.Book.StockQuantity)
                {
                    TempData["CartError"] =
                        "Một số sách không còn đủ số lượng trong kho.";

                    return RedirectToAction("CartIndex", "Cart");
                }
            }

            User user = db.Users.Find(userID);

            if (user == null)
                return RedirectToAction("Login", "Account");

            CheckoutViewModel model = new CheckoutViewModel
            {
                SelectedCartItemIDs = selectedIDs,

                ReceiverName = user.FullName,
                ReceiverPhone = user.Phone,
                AddressDelivery = user.Address,

                Items = cartItems.Select(item => new CheckoutItemViewModel
                {
                    CartItemID = item.CartItemID,
                    BookID = item.BookID,
                    Title = item.Book.Title,
                    ImageURL = item.Book.ImageURL,
                    UnitPrice = item.Book.Price,
                    Quantity = item.Quantity
                }).ToList()
            };

            model.TotalAmount = model.Items.Sum(item => item.Subtotal);

            return View(model);
        }

        // POST: Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(CheckoutViewModel model)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];

            if (model.SelectedCartItemIDs == null ||
                !model.SelectedCartItemIDs.Any())
            {
                TempData["CartError"] = "Không có sản phẩm để đặt hàng.";
                return RedirectToAction("CartIndex", "Cart");
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("CartIndex", "Cart");
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var selectedIDs = model.SelectedCartItemIDs
                        .Distinct()
                        .ToList();

                    var cartItems = db.CartItems
                        .Include(ci => ci.Book)
                        .Include(ci => ci.Cart)
                        .Where(ci =>
                            selectedIDs.Contains(ci.CartItemID) &&
                            ci.Cart.UserID == userID)
                        .ToList();

                    if (cartItems.Count != selectedIDs.Count ||
                        !cartItems.Any())
                    {
                        transaction.Rollback();

                        TempData["CartError"] =
                            "Giỏ hàng đã thay đổi. Vui lòng thử lại.";

                        return RedirectToAction("CartIndex", "Cart");
                    }

                    foreach (var item in cartItems)
                    {
                        if (!item.Book.IsActive ||
                            item.Quantity <= 0 ||
                            item.Quantity > item.Book.StockQuantity)
                        {
                            transaction.Rollback();

                            TempData["CartError"] =
                                "Sách không còn đủ số lượng trong kho.";

                            return RedirectToAction("CartIndex", "Cart");
                        }
                    }

                    decimal totalAmount = cartItems.Sum(
                        item => item.Book.Price * item.Quantity);

                    Order order = new Order
                    {
                        UserID = userID,
                        OrderDate = DateTime.Now,
                        TotalAmount = totalAmount,

                        Status = "Chờ xác nhận",
                        PaymentMethod = "COD",
                        PaymentStatus = "Chưa thanh toán",

                        ReceiverName = model.ReceiverName.Trim(),
                        ReceiverPhone = model.ReceiverPhone.Trim(),
                        AddressDelivery = model.AddressDelivery.Trim(),
                        Notes = model.Notes,

                        CompletedAt = null,
                        ReturnDeadline = null
                    };

                    db.Orders.Add(order);
                    db.SaveChanges();

                    foreach (var item in cartItems)
                    {
                        db.OrderDetails.Add(new OrderDetail
                        {
                            OrderID = order.OrderID,
                            BookID = item.BookID,
                            Quantity = item.Quantity,
                            UnitPrice = item.Book.Price
                        });

                        item.Book.StockQuantity -= item.Quantity;

                        db.CartItems.Remove(item);
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    return RedirectToAction("OrderSuccess", new { id = order.OrderID });
                }
                catch
                {
                    transaction.Rollback();

                    TempData["CartError"] =
                        "Đặt hàng không thành công. Vui lòng thử lại.";

                    return RedirectToAction("CartIndex", "Cart");
                }
            }
        }

        // GET: Order/OrderSuccess
        public ActionResult OrderSuccess(int id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];

            var order = db.Orders.FirstOrDefault(o =>
                o.OrderID == id && o.UserID == userID);

            if (order == null)
                return HttpNotFound();

            return View(order);
        }

        public ActionResult OrderHistory()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];

            var orders = db.Orders
                .Where(o => o.UserID == userID)
                .OrderByDescending(o => o.OrderDate)
                .ToList();            
            return View(orders);
        }

        // GET: Order/OrderDetails
        // GET: Order/OrderDetails
        public ActionResult OrderDetails(int id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];

            var order = db.Orders.FirstOrDefault(o =>
                o.OrderID == id &&
                o.UserID == userID);

            if (order == null)
                return HttpNotFound();

            // Tải danh sách chi tiết đơn hàng
            db.Entry(order)
                .Collection(o => o.OrderDetails)
                .Load();

            // Tải thông tin sách của từng chi tiết
            foreach (var item in order.OrderDetails)
            {
                db.Entry(item)
                    .Reference(od => od.Book)
                    .Load();
            }

            return View(order);
        }

        // POST: Order/CancelOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrder(int id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var order = db.Orders
                        .Include(o => o.OrderDetails.Select(od => od.Book))
                        .FirstOrDefault(o =>
                            o.OrderID == id &&
                            o.UserID == userID);

                    if (order == null)
                    {
                        transaction.Rollback();
                        return HttpNotFound();
                    }

                    // Chỉ cho phép hủy ở 3 trạng thái
                    var allowedStatuses = new[]
                    {
                "Chờ xác nhận",
                "Đã xác nhận",
                "Đang đóng gói"
            };

                    if (!allowedStatuses.Contains(order.Status))
                    {
                        transaction.Rollback();

                        TempData["OrderMessage"] =
                            "Đơn hàng hiện tại không thể hủy.";

                        return RedirectToAction("OrderDetails", new { id = id });
                    }

                    // Hoàn lại số lượng sách vào kho
                    foreach (var item in order.OrderDetails)
                    {
                        item.Book.StockQuantity += item.Quantity;
                    }

                    // Cập nhật trạng thái đơn hàng
                    order.Status = "Đã hủy";

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["OrderMessage"] =
                        "Hủy đơn hàng thành công.";

                    return RedirectToAction("OrderDetails", new { id = id });
                }
                catch
                {
                    transaction.Rollback();

                    TempData["OrderMessage"] =
                        "Không thể hủy đơn hàng. Vui lòng thử lại.";

                    return RedirectToAction("OrderDetails", new { id = id });
                }
            }
        }

        // POST: Order/ConfirmReceived
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmReceived(int id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var order = db.Orders.FirstOrDefault(o =>
                        o.OrderID == id &&
                        o.UserID == userID);

                    if (order == null)
                    {
                        transaction.Rollback();
                        return HttpNotFound();
                    }

                    // Chỉ xác nhận khi đơn hàng đang giao
                    if (order.Status != "Đang giao hàng" ||
                        order.CompletedAt != null)
                    {
                        transaction.Rollback();

                        TempData["OrderMessage"] =
                            "Đơn hàng không thể xác nhận nhận hàng.";

                        return RedirectToAction("OrderDetails", new { id = id });
                    }

                    // Ghi nhận thời điểm nhận hàng
                    DateTime completedAt = DateTime.Now;

                    order.Status = "Đã giao hàng";
                    order.PaymentStatus = "Đã thanh toán";
                    order.CompletedAt = completedAt;
                    order.ReturnDeadline = completedAt.AddDays(7);

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["OrderMessage"] =
                        "Xác nhận nhận hàng thành công. Bạn có 7 ngày để gửi yêu cầu hoàn trả.";

                    return RedirectToAction("OrderDetails", new { id = id });
                }
                catch
                {
                    transaction.Rollback();

                    TempData["OrderMessage"] =
                        "Không thể xác nhận nhận hàng. Vui lòng thử lại.";

                    return RedirectToAction("OrderDetails", new { id = id });
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}