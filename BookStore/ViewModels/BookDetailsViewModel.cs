using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookStore.ViewModels
{
    public class BookDetailsViewModel
    {
        public Book Book { get; set; }
        public int BookID { get; set; }
        public int Quantity { get; set; } = 1;
        public string ErrorMessage { get; set; }
        public BookReviewViewModel ReviewInfo { get; set; }

    }
}