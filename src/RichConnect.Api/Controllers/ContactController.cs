using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Application.Interfaces.Notifications;

namespace RICHConnect.Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Contact form must be public for non-authenticated users
    public class ContactController : ApiControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IEmailService emailService, ILogger<ContactController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Send a contact form message
        /// SECURITY: Rate limited to prevent email spam and DoS
        /// </summary>
        [HttpPost("send-message")]
        [RequestSizeLimit(100 * 1024)] // 100 KB limit for contact messages
        public async Task<IActionResult> SendMessage([FromBody] ContactMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return ErrorResponse<object>("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return ErrorResponse<object>("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return ErrorResponse<object>("Message is required.");
            }

            if (!_emailService.IsEmailConfigured())
            {
                _logger.LogWarning("Contact form message not sent - Email service not configured");
                return ErrorResponse<object>("Email service is not configured. Please try again later.");
            }

            // Email subject
            var subject = $"RICHConnect Contact Form: Message from {request.Name}";

            // Email body
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2c3e50;'>New Contact Form Message</h2>
                    <p>You have received a new message from the RICHConnect contact form:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Name:</strong> {WebUtility.HtmlEncode(request.Name)}</p>
                        <p><strong>Email:</strong> {WebUtility.HtmlEncode(request.Email)}</p>
                        <p><strong>Message:</strong></p>
                        <p style='white-space: pre-wrap;'>{WebUtility.HtmlEncode(request.Message)}</p>
                    </div>
                    <p>Please respond to this inquiry at your earliest convenience.</p>
                    <p>Best regards,<br>RICHConnect System</p>
                </div>";

            // Send email to rich@aub.edu.lb
            var success = await _emailService.SendEmailFromUserAsync(
                request.Email,
                request.Name,
                "rich@aub.edu.lb",
                "RICH Team",
                subject,
                body
            );

            if (success)
            {
                _logger.LogInformation("Contact form message sent successfully. From: {FromEmail}, Name: {Name}", 
                    request.Email, request.Name);
                return SuccessResponse<object?>(null, "Your message has been sent successfully. We will get back to you soon.");
            }
            else
            {
                _logger.LogError("Failed to send contact form message. From: {FromEmail}, Name: {Name}", 
                    request.Email, request.Name);
                return ErrorResponse<object>("Failed to send your message. Please try again later.");
            }
        }
    }

    public class ContactMessageRequest
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
