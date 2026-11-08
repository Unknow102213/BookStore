using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BookStore.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Email không được chứa dấu cách.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Mật khẩu không được chứa dấu cách.")]
        public string Password { get; set; }
    }
}