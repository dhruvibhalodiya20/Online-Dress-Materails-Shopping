using Microsoft.AspNetCore.Mvc;
using Mypro.Models;
using System;
using System.Linq;

namespace Mypro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatApiController : ControllerBase
    {
        private readonly ApplicationDbContext _abc;

        public ChatApiController(ApplicationDbContext context)
        {
            _abc = context;
        }

        [HttpPost("SaveMessage")]
        public IActionResult SaveMessage([FromBody] ChatMessageRequest request)
        {
            try
            {
                var chatMessage = new ChatMessage
                {
                    Query = request.Query,
                    BotResponse = request.BotResponse,
                    UserEmail = request.UserEmail,
                    SessionId = request.SessionId,
                    CreatedAt = DateTime.Now
                };

                _abc.ChatMessages.Add(chatMessage);
                _abc.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Message saved successfully",
                    id = chatMessage.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("GetHistory")]
        public IActionResult GetHistory(string sessionId)
        {
            try
            {
                var messages = _abc.ChatMessages
                    .Where(m => m.SessionId == sessionId)
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new {

                        m.Id,
                        m.Query,
                        m.BotResponse,
                        m.CreatedAt
                    })
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = messages
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }

    public class ChatMessageRequest
    {
        public string Query { get; set; }
        public string BotResponse { get; set; }
        public string UserEmail { get; set; }
        public string SessionId { get; set; }
    }
}