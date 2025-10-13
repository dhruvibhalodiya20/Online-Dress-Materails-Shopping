namespace Mypro.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string? ImagePath { get; set; }
        public int? CategoryId { get; set; }
        public int? ImageId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public int? CustomerId { get; set; }
       
    }
}
