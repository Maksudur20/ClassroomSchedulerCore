using Microsoft.AspNetCore.Identity.UI.Services;

namespace ClassroomSchedulerCore.Services
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Log the email instead of actually sending it
            Console.WriteLine($"Email to {email}: Subject: {subject}");
            Console.WriteLine($"Content: {htmlMessage}");
            
            // Return a completed task since we're not actually sending emails
            return Task.CompletedTask;
        }
    }
}
