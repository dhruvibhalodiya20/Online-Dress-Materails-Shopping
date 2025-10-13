using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mypro.Models
{
    public class CategoryImage
    {
        [Key]
        public int ImageId { get; set; }

     
        public string? ImagePath { get; set; }


        [Required]
        public int CategoryId { get; set; }


        [MaxLength(50)]
        public string? Color { get; set; }

        [MaxLength(7)] 
        public string? HexColor { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

       
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
}
