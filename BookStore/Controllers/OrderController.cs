
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

            if (model == null ||
                model.SelectedCartItemIDs == null ||
                !model.SelectedCartItemIDs.Any())
            {
                TempData["CartError"] = "Không có sản phẩm để đặt hàng.";
                return RedirectToAction("CartIndex", "Cart");
            }

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
                    model.CheckoutError =
                        "Sách không còn đủ số lượng trong kho. Vui lòng kiểm tra lại.";

                    return View("Checkout", model);
                }
            }

            model.SelectedCartItemIDs = selectedIDs;

            model.Items = cartItems.Select(item => new CheckoutItemViewModel
            {
                CartItemID = item.CartItemID,
                BookID = item.BookID,
                Title = item.Book.Title,
                ImageURL = item.Book.ImageURL,
                UnitPrice = item.Book.Price,
                Quantity = item.Quantity
            }).ToList();

            model.TotalAmount = model.Items.Sum(item => item.Subtotal);

            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
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

                    return RedirectToAction(
                        "OrderSuccess",
                        new { id = order.OrderID });
                }
                catch
                {
                    transaction.Rollback();

                    model.CheckoutError =
                        "Đặt hàng không thành công. Vui lòng kiểm tra lại thông tin và thử lại.";

                    return View("Checkout", model);
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

        // GET: Order/OrderHistory
        public ActionResult OrderHistory(string status)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];

            var allowedStatuses = new[]
            {
                "Chờ xác nhận",
                "Đang chuẩn bị",
                "Đang giao hàng",
                "Đã nhận hàng",
                "Đã hủy"
            };

            if (string.IsNullOrEmpty(status) ||
                !allowedStatuses.Contains(status))
            {
                status = "Chờ xác nhận";
            }

            var orders = db.Orders
                .Where(o =>
                    o.UserID == userID &&
                    o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

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

            db.Entry(order)
                .Collection(o => o.OrderDetails)
                .Load();

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

            string previousStatus = "Chờ xác nhận";

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

                    previousStatus = order.Status;

                    var allowedStatuses = new[]
                    {
                        "Chờ xác nhận",
                        "Đang chuẩn bị"
                    };

                    if (!allowedStatuses.Contains(order.Status))
                    {
                        transaction.Rollback();

                        TempData["OrderMessage"] =
                            "Đơn hàng hiện tại không thể hủy.";

                        return RedirectToAction(
                            "OrderHistory",
                            new { status = order.Status });
                    }

                    foreach (var item in order.OrderDetails)
                    {
                        item.Book.StockQuantity += item.Quantity;
                    }

                    order.Status = "Đã hủy";

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["OrderMessage"] =
                        "Hủy đơn hàng thành công.";

                    return RedirectToAction(
                        "OrderHistory",
                        new { status = "Đã hủy" });
                }
                catch
                {
                    transaction.Rollback();

                    TempData["OrderMessage"] =
                        "Không thể hủy đơn hàng. Vui lòng thử lại.";

                    return RedirectToAction(
                        "OrderHistory",
                        new { status = previousStatus });
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

                    if (order.Status != "Đang giao hàng" ||
                        order.CompletedAt != null)
                    {
                        transaction.Rollback();

                        TempData["OrderMessage"] =
                            "Đơn hàng không thể xác nhận nhận hàng.";

                        return RedirectToAction(
                            "OrderHistory",
                            new { status = "Đang giao hàng" });
                    }

                    DateTime completedAt = DateTime.Now;

                    order.Status = "Đã nhận hàng";
                    order.PaymentStatus = "Đã thanh toán";
                    order.CompletedAt = completedAt;

                    // Không sử dụng chức năng hoàn trả
                    order.ReturnDeadline = null;

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["OrderMessage"] =
                        "Xác nhận nhận hàng thành công.";

                    return RedirectToAction(
                        "OrderHistory",
                        new { status = "Đã nhận hàng" });
                }
                catch
                {
                    transaction.Rollback();

                    TempData["OrderMessage"] =
                        "Không thể xác nhận nhận hàng. Vui lòng thử lại.";

                    return RedirectToAction(
                        "OrderHistory",
                        new { status = "Đang giao hàng" });
                }
            }
        }

        // POST: Order/Reorder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reorder(int id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Lấy đơn hàng cũ của khách
                    var order = db.Orders
                        .Include(o => o.OrderDetails.Select(od => od.Book))
                        .FirstOrDefault(o =>
                            o.OrderID == id &&
                            o.UserID == userID);

                    if (order == null)
                        return HttpNotFound();

                    // Chỉ cho phép đặt lại đơn đã nhận hoặc đã hủy
                    if (order.Status != "Đã nhận hàng" &&
                        order.Status != "Đã hủy")
                    {
                        transaction.Rollback();

                        TempData["OrderMessage"] =
                            "Đơn hàng này không thể đặt lại.";

                        return RedirectToAction("OrderHistory");
                    }

                    // Lấy giỏ hàng của khách
                    var cart = db.Carts.FirstOrDefault(c =>
                        c.UserID == userID);

                    if (cart == null)
                    {
                        cart = new Cart
                        {
                            UserID = userID,
                            CreatedAt = DateTime.Now
                        };

                        db.Carts.Add(cart);
                        db.SaveChanges();
                    }

                    int addedCount = 0;
                    int skippedCount = 0;

                    // Gộp các sách trùng nhau trong đơn hàng
                    var orderItems = order.OrderDetails
                        .GroupBy(od => od.BookID)
                        .Select(g => new
                        {
                            BookID = g.Key,
                            Quantity = g.Sum(od => od.Quantity)
                        })
                        .ToList();

                    foreach (var item in orderItems)
                    {
                        var book = db.Books.FirstOrDefault(b =>
                            b.BookID == item.BookID);

                        // Bỏ qua sách đã ngừng kinh doanh
                        if (book == null || !book.IsActive)
                        {
                            skippedCount++;
                            continue;
                        }

                        // Tìm sách đã có trong giỏ hàng
                        var cartItem = db.CartItems.FirstOrDefault(ci =>
                            ci.CartID == cart.CartID &&
                            ci.BookID == item.BookID);

                        int currentQuantity = cartItem != null
                            ? cartItem.Quantity
                            : 0;

                        // Kiểm tra tồn kho
                        if (currentQuantity + item.Quantity > book.StockQuantity)
                        {
                            skippedCount++;
                            continue;
                        }

                        // Cập nhật số lượng hoặc thêm sách mới
                        if (cartItem != null)
                        {
                            cartItem.Quantity += item.Quantity;
                        }
                        else
                        {
                            db.CartItems.Add(new CartItem
                            {
                                CartID = cart.CartID,
                                BookID = item.BookID,
                                Quantity = item.Quantity
                            });
                        }

                        addedCount++;
                    }

                    if (addedCount == 0)
                    {
                        transaction.Rollback();

                        TempData["OrderMessage"] =
                            "Không có sách nào đủ điều kiện để đặt lại. " +
                            "Vui lòng kiểm tra tình trạng kinh doanh và tồn kho.";

                        return RedirectToAction(
                            "OrderHistory",
                            new { status = order.Status });
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["CartMessage"] =
                        "Đã thêm " + addedCount + " đầu sách vào giỏ hàng." +
                        (skippedCount > 0
                            ? " Có " + skippedCount + " đầu sách không được thêm do hết hàng hoặc ngừng kinh doanh."
                            : "");

                    return RedirectToAction("CartIndex", "Cart");
                }
                catch
                {
                    transaction.Rollback();

                    TempData["OrderMessage"] =
                        "Không thể đặt lại đơn hàng. Vui lòng thử lại.";

                    return RedirectToAction("OrderHistory");
                }
            }
        }

        // GET: Order/ReviewOrder
        public ActionResult ReviewOrder(int id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];

            var order = db.Orders
                .Include(o => o.OrderDetails.Select(od => od.Book))
                .FirstOrDefault(o =>
                    o.OrderID == id &&
                    o.UserID == userID);

            if (order == null)
                return HttpNotFound();

            if (order.Status != "Đã nhận hàng")
            {
                TempData["OrderMessage"] =
                    "Chỉ có thể đánh giá đơn hàng đã nhận.";

                return RedirectToAction(
                    "OrderHistory",
                    new { status = order.Status });
            }

            var existingReviews = db.Reviews
                .Where(r =>
                    r.OrderID == order.OrderID &&
                    r.UserID == userID)
                .ToList();

            var firstReview = existingReviews.FirstOrDefault();

            var model = new ReviewOrderViewModel
            {
                OrderID = order.OrderID,

                Rating = firstReview != null
                    ? firstReview.Rating
                    : 5,

                Comment = firstReview != null
                    ? firstReview.Comment
                    : "",

                IsReviewed = existingReviews.Any(),

                Books = order.OrderDetails
                    .GroupBy(od => od.BookID)
                    .Select(g => new ReviewBookViewModel
                    {
                        BookID = g.Key,
                        Title = g.FirstOrDefault().Book.Title,
                        ImageURL = g.FirstOrDefault().Book.ImageURL
                    })
                    .ToList()
            };

            return View(model);
        }

        // POST: Order/SubmitReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitReview(ReviewOrderViewModel model)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];

            var order = db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o =>
                    o.OrderID == model.OrderID &&
                    o.UserID == userID);

            if (order == null)
                return HttpNotFound();

            if (order.Status != "Đã nhận hàng")
                return HttpNotFound();

            if (!ModelState.IsValid)
            {
                return RedirectToAction(
                    "ReviewOrder",
                    new { id = model.OrderID });
            }

            var bookIDs = order.OrderDetails
                .Select(od => od.BookID)
                .Distinct()
                .ToList();

            var reviews = db.Reviews
                .Where(r =>
                    r.OrderID == order.OrderID &&
                    r.UserID == userID)
                .ToList();

            // Cập nhật đánh giá đã tồn tại
            if (reviews.Any())
            {
                foreach (var review in reviews)
                {
                    review.Rating = model.Rating;
                    review.Comment = model.Comment.Trim();
                    review.CreatedAt = DateTime.Now;
                }
            }
            else
            {
                // Tạo đánh giá cho từng cuốn sách trong đơn
                foreach (var bookID in bookIDs)
                {
                    db.Reviews.Add(new Review
                    {
                        OrderID = order.OrderID,
                        UserID = userID,
                        BookID = bookID,
                        Rating = model.Rating,
                        Comment = model.Comment.Trim(),
                        CreatedAt = DateTime.Now
                    });
                }
            }

            db.SaveChanges();

            TempData["OrderMessage"] = "Đánh giá đơn hàng thành công.";

            return RedirectToAction(
                "OrderHistory",
                new { status = "Đã nhận hàng" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
