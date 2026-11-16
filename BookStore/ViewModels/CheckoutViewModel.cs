using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BookStore.ViewModels
{
    public class CheckoutViewModel
    {
        public string CheckoutError { get; set; }
        public List<int> SelectedCartItemIDs { get; set; }

        public List<CheckoutItemViewModel> Items { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string ReceiverName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(
            @"^(03[2-9]|05[25689]|07[06789]|08[1-9]|09[0-9])[0-9]{7}$",
            ErrorMessage = "Số điện thoại không hợp lệ. Vui lòng nhập số di động Việt Nam gồm 10 chữ số."
        )]
        public string ReceiverPhone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
        [StringLength(255, ErrorMessage = "Địa chỉ giao hàng không được vượt quá 255 ký tự.")]
        public string AddressDelivery { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string Notes { get; set; }

        public decimal TotalAmount { get; set; }
    }

    public class CheckoutItemViewModel
    {
        public int CartItemID { get; set; }
        public int BookID { get; set; }
        public string Title { get; set; }
        public string ImageURL { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal Subtotal
        {
            get { return UnitPrice * Quantity; }
        }
    }
}