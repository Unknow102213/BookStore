using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using BookStore.Models;

namespace BookStore.Controllers
{
    public class CartController : Controller
    {
        private BookStoreEntities db = new BookStoreEntities();

        // GET: Cart/CartIndex
        public ActionResult CartIndex()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];

            Cart cart = db.Carts
                .Include(c => c.CartItems.Select(ci => ci.Book))
                .FirstOrDefault(c => c.UserID == userID);

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

            return View(cart);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int id, int quantity = 1)
        {
            bool isAjax = Request.IsAjaxRequest();

            if (Session["UserID"] == null)
            {
                if (isAjax)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Vui lòng đăng nhập để thêm sách vào giỏ hàng.",
                        redirectUrl = Url.Action("Login", "Account")
                    });
                }

                return RedirectToAction("Login", "Account");
            }

            int userID = (int)Session["UserID"];
            Book book = db.Books.Find(id);

            if (book == null)
            {
                if (isAjax)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy sách."
                    });
                }

                return HttpNotFound();
            }

            string error = null;

            if (!book.IsActive)
                error = "Sách hiện không được kinh doanh.";
            else if (book.StockQuantity <= 0)
                error = "Sách hiện đã hết hàng.";
            else if (quantity <= 0)
                error = "Số lượng phải lớn hơn 0.";
            else if (quantity > book.StockQuantity)
                error = "Số lượng vượt quá số sách còn trong kho.";

            if (error != null)
            {
                if (isAjax)
                {
                    return Json(new
                    {
                        success = false,
                        message = error
                    });
                }

                TempData["CartError"] = error;
                return RedirectToAction("CartIndex");
            }

            Cart cart = db.Carts.FirstOrDefault(c => c.UserID == userID);

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

            CartItem item = db.CartItems.FirstOrDefault(
                ci => ci.CartID == cart.CartID && ci.BookID == id);

            if (item != null)
            {
                int newQuantity = item.Quantity + quantity;

                if (newQuantity > book.StockQuantity)
                {
                    error = "Tổng số lượng sách trong giỏ vượt quá tồn kho.";

                    if (isAjax)
                    {
                        return Json(new
                        {
                            success = false,
                            message = error
                        });
                    }

                    TempData["CartError"] = error;
                    return RedirectToAction("CartIndex");
                }

                item.Quantity = newQuantity;
            }
            else
            {
                db.CartItems.Add(new CartItem
                {
                    CartID = cart.CartID,
                    BookID = id,
                    Quantity = quantity
                });
            }

            db.SaveChanges();

            int cartCount = db.CartItems.Count(ci => ci.CartID == cart.CartID);

            if (isAjax)
            {
                return Json(new
                {
                    success = true,
                    message = "Đã thêm sách vào giỏ hàng!",
                    cartCount = cartCount
                });
            }

            TempData["CartSuccess"] = "Đã thêm sách vào giỏ hàng.";
            return RedirectToAction("CartIndex");
        }


        // POST: Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int cartItemID, int quantity)
        {
            if (Session["UserID"] == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
                    redirectUrl = Url.Action("Login", "Account")
                });
            }

            int userID = (int)Session["UserID"];

            CartItem item = db.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Book)
                .FirstOrDefault(ci =>
                    ci.CartItemID == cartItemID &&
                    ci.Cart.UserID == userID);

            if (item == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy sách trong giỏ hàng."
                });
            }

            if (quantity <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Số lượng phải lớn hơn 0."
                });
            }

            if (quantity > item.Book.StockQuantity)
            {
                return Json(new
                {
                    success = false,
                    message = "Số lượng vượt quá số sách còn trong kho.",
                    maxQuantity = item.Book.StockQuantity
                });
            }

            item.Quantity = quantity;
            db.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Đã cập nhật số lượng.",
                quantity = item.Quantity,
                unitPrice = item.Book.Price,
                subtotal = item.Book.Price * item.Quantity
            });
        }

        // POST: Cart/RemoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveItem(int cartItemID)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = (int)Session["UserID"];

            CartItem item = db.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefault(ci =>
                    ci.CartItemID == cartItemID &&
                    ci.Cart.UserID == userID);

            if (item == null)
                return HttpNotFound();

            db.CartItems.Remove(item);
            db.SaveChanges();

            TempData["CartSuccess"] =
                "Đã xóa sách khỏi giỏ hàng.";

            return RedirectToAction("CartIndex");
        }

        [ChildActionOnly]
        public ActionResult CartCount()
        {
            if (Session["UserID"] == null)
                return Content("");

            int userID = (int)Session["UserID"];

            var cart = db.Carts
                .FirstOrDefault(c => c.UserID == userID);

            if (cart == null)
                return Content("");

            int count = db.CartItems
                .Count(ci => ci.CartID == cart.CartID);

            if (count == 0)
                return Content("");

            return Content("<span id=\"header-cart-count\" class=\"header-cart-count\">" +count +"</span>");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}