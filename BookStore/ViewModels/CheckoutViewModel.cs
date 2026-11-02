using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BookStore.ViewModels
{
    public class CheckoutViewModel
    {
        public List<int> SelectedCartItemIDs { get; set; }

        public List<CheckoutItemViewModel> Items { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận.")]
        [StringLength(100)]
        public string ReceiverName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(15)]
        public string ReceiverPhone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
        [StringLength(500)]
        public string AddressDelivery { get; set; }

        [StringLength(500)]
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