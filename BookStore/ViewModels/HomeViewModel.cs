using System.Collections.Generic;
using BookStore.Models;

namespace BookStore.ViewModels
{
    public class HomeViewModel
    {
        public List<Book> FeaturedBooks { get; set; }

        public List<Book> RecommendedBooks { get; set; }

        public List<Book> BestSellingBooks { get; set; }

        public List<Book> NewBooks { get; set; }

        public List<Category> Categories { get; set; }
    }
}