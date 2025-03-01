using Application.Interfaces;
using Application.DTOs;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Core.Entities.Identity;


namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IPdfService pdfService;
        private readonly IConfiguration configuration;

        public EmailService(IPdfService pdfService, IConfiguration configuration)
        {
            this.pdfService = pdfService;
            this.configuration = configuration;
        }

        private async Task SendAsync(EmailDto emailDto)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Concertable", configuration["Email:Username"]));
            email.To.Add(MailboxAddress.Parse(emailDto.To));
            email.Subject = emailDto.Subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = emailDto.Body
            };

            if (emailDto.Attachment != null)
            {
                bodyBuilder.Attachments.Add(emailDto.AttachmentName ?? "Ticket.pdf", emailDto.Attachment, ContentType.Parse("application/pdf"));
            }

            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(configuration["Email:SmtpServer"], int.Parse(configuration["Email:SmtpPort"]), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(configuration["Email:Username"], configuration["Email:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendEmailAsync(int userId, string toEmail, string subject, string body)
        {
            var emailDto = new EmailDto
            {
                To = toEmail,
                Subject = subject,
                Body = body,
            };

            await SendAsync(emailDto);
        }

        public async Task SendTicketEmailAsync(int userId, string toEmail, int ticketId)
        {
            byte[] ticketPdf = await pdfService.GenerateTicketReciptAsync(toEmail, ticketId);

            var emailDto = new EmailDto
            {
                To = toEmail,
                Subject = "Your Ticket Receipt",
                Body = "<p>Here is your receipt for your event ticket.</p>",
                Attachment = ticketPdf,
                AttachmentName = "Ticket.pdf"
            };

            await SendAsync(emailDto);
        }
    }
}
