//using System.Net;
//using System.Net.Mail;
//using System.Threading.Tasks;
//using AuthMicroservice.Application.Interfaces;
//using AuthMicroservice.Infrastructure.Config;
//using Microsoft.Extensions.Options;

//namespace AuthMicroservice.Infrastructure.Email
//{
//    /// <summary>
//    /// Implémente IEmailService pour envoyer des emails via SMTP.
//    /// </summary>
//    public class SmtpEmailService : IEmailService
//    {
//        private readonly SmtpSettings _smtp;

//        public SmtpEmailService(IOptions<SmtpSettings> smtpOptions)
//        {
//            _smtp = smtpOptions.Value;
//        }

//        /// <summary>
//        /// Envoie un email générique.
//        /// </summary>
//        /// <param name="to">Adresse destinataire.</param>
//        /// <param name="subject">Sujet de l’email.</param>
//        /// <param name="body">Corps HTML de l’email.</param>
//        public async Task SendEmailAsync(string to, string subject, string body)
//        {
//            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
//            {
//                Credentials = new NetworkCredential(_smtp.User, _smtp.Password),
//                EnableSsl = _smtp.EnableSsl
//            };

//            var message = new MailMessage
//            {
//                From = new MailAddress(_smtp.From),
//                Subject = subject,
//                Body = body,
//                IsBodyHtml = true
//            };
//            message.To.Add(to);

//            await client.SendMailAsync(message);
//        }
//    }
//}