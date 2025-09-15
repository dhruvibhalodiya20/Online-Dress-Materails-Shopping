using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mypro.Models
{
    public class City
    {
        [Key]
        public int CityId { get; set; }

        [Required]
        [StringLength(100)]
        public string CityName { get; set; }

        
        public ICollection<Customer> Customers { get; set; }
    }
}
