using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mypro.Models
{
    public class Wishlist
    {
        [Key]
        public int Id { get; set; }

        
        public int? ImageId { get; set; }

        [ForeignKey("ImageId")]
        public CategoryImage? CategoryImage { get; set; }

        
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Required]
        public string UserEmail { get; set; }

        public DateTime AddedOn { get; set; } = DateTime.Now;
    }
}
