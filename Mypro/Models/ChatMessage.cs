using System.ComponentModel.DataAnnotations;

namespace Mypro.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Query { get; set; }

        [Required]
        public string BotResponse { get; set; }

        public string UserEmail { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string SessionId { get; set; }
    }
}
