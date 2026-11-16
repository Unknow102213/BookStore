using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookStore.ViewModels
{
    public class BookReviewViewModel
    {
        public int TotalReviews { get; set; }

        public double AverageRating { get; set; }

        public List<BookReviewItemViewModel> Reviews { get; set; }
    }

    public class BookReviewItemViewModel
    {
        public int Rating { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public string FullName { get; set; }
    }
}