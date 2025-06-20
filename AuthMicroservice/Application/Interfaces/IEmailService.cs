namespace AuthMicroservice.Application.Interfaces
{
    /// <summary>
    /// Contract for sending emails via an email provider (SMTP, SendGrid, etc.).
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email asynchronously to the specified recipient.
        /// </summary>
        /// <param name="to">Recipient email address.</param>
        /// <param name="subject">Subject of the email.</param>
        /// <param name="htmlBody">HTML content of the message.</param>
        Task SendEmailAsync(string to, string subject, string htmlBody);
    }
}