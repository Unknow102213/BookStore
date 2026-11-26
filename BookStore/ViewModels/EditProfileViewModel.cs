using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BookStore.ViewModels
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100,
            ErrorMessage = "Họ tên tối đa 100 ký tự.")]
        [RegularExpression(@"^[\p{L} ]+$",
            ErrorMessage = "Họ tên chỉ được chứa chữ cái và dấu cách.")]
        public string FullName { get; set; }

        [RegularExpression(
            @"^(03[2-9]|05[25689]|07[06789]|08[1-9]|09[0-9])[0-9]{7}$",
            ErrorMessage = "Số điện thoại không hợp lệ. Vui lòng nhập số di động Việt Nam gồm 10 chữ số."
        )]
        public string Phone { get; set; }

        [StringLength(255,
            ErrorMessage = "Địa chỉ giao hàng không được vượt quá 255 ký tự.")]
        public string Address { get; set; }
    }

    // ViewModel đổi mật khẩu
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [StringLength(72, MinimumLength = 8,
            ErrorMessage = "Mật khẩu phải từ 8 đến 72 ký tự.")]
        [RegularExpression(@"^\S+$",
            ErrorMessage = "Mật khẩu không được chứa dấu cách.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [Compare("NewPassword",
            ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}