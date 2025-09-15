using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mypro.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ImageId { get; set; }

        [ForeignKey("ImageId")]
        public CategoryImage Image { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        public string UserId { get; set; }  

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }   
    }
}
