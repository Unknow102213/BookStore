using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BookStore.ViewModels
{
    public class ReviewOrderViewModel
    {
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá.")]
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá.")]
        [StringLength(1000, ErrorMessage = "Nội dung không được vượt quá 1000 ký tự.")]
        public string Comment { get; set; }

        public List<ReviewBookViewModel> Books { get; set; }

        public bool IsReviewed { get; set; }
    }

    public class ReviewBookViewModel
    {
        public int BookID { get; set; }

        public string Title { get; set; }

        public string ImageURL { get; set; }
    }
}