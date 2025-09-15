using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mypro.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required, StringLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact number must be 10 digits.")]
        [Repeatenumber(ErrorMessage = "Contact number cannot have all identical digits.")]
        public string ContactNumber { get; set; }

        public string Gender { get; set; }
        public string Address { get; set; }

        [Required]
        public int CityId { get; set; }   

        [ForeignKey("CityId")]
        public City? City { get; set; }   

        [Required]
        public string Role { get; set; } = "User";

        public string? PhotoPath { get; set; }
    }

    public class RepeatenumberAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            var str = value.ToString();
            if (str.Distinct().Count() == 1)
            {
                return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}
