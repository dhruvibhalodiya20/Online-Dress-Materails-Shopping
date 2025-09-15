using System.ComponentModel.DataAnnotations;

namespace Mypro.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryType { get; set; } // "Print", "Plain", "Other"
    }
}
