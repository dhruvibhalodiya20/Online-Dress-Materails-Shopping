using System;
using System.ComponentModel.DataAnnotations;

namespace Mypro.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Range(1, 5)]
        public int Stars { get; set; }

        [Required, StringLength(1000)]
        public string ReviewText { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class ReviewSummary
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStars { get; set; }
        public int FourStars { get; set; }
        public int ThreeStars { get; set; }
        public int TwoStars { get; set; }
        public int OneStar { get; set; }
    }
}
