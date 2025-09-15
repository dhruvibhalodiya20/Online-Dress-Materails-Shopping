using System.ComponentModel.DataAnnotations;

namespace Mypro.Models
{
    public class CategoryImageVM
    {
        [Required]
        public IFormFile ImageFile { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [MaxLength(50)]
        public string Color { get; set; }

        [MaxLength(7)]
        public string HexColor { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}
