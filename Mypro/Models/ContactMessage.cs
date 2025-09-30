using System;
using System.ComponentModel.DataAnnotations;

namespace Mypro.Models
{
    public class ContactMessage
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; }

        [Required, StringLength(200)]
        public string Subject { get; set; }

        [Required, StringLength(2000)]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Reply { get; set; }
        public DateTime? ReplyAt { get; set; }
    }
}
