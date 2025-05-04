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

            email.From.Add(new MailboxAddress("Concertable", configuration["Email:From"]));

            email.To.Add(MailboxAddress.Parse(emailDto.To));
            email.Subject = emailDto.Subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = emailDto.Body
            };

            if (emailDto.Attachments != null)
            {
                foreach (var attachment in emailDto.Attachments)
                {
                    bodyBuilder.Attachments.Add(
                        attachment.FileName,
                        attachment.Content,
                        ContentType.Parse(attachment.MimeType)
                    );
                }
            }

            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                configuration["Email:SmtpServer"],
                int.Parse(configuration["Email:SmtpPort"]),
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                configuration["Email:Username"],
                configuration["Email:Password"]
            );

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }


        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailDto = new EmailDto
            {
                To = toEmail,
                Subject = subject,
                Body = body,
            };

            await SendAsync(emailDto);
        }

        public async Task SendTicketsToEmailAsync(string toEmail, IEnumerable<int> ticketIds)
        {
            var attachments = new List<AttachmentDto>();

            foreach (var ticketId in ticketIds)
            {
                byte[] ticketPdf = await pdfService.GenerateTicketReciptAsync(toEmail, ticketId);

                attachments.Add(new AttachmentDto
                {
                    Content = ticketPdf,
                    FileName = $"Ticket-{ticketId}.pdf"
                });
            }

            var emailDto = new EmailDto
            {
                To = toEmail,
                Subject = "Your Ticket Receipt",
                Body = $"<p>Thank you for your order! Your {ticketIds.Count()} ticket(s) are attached.</p>",
                Attachments = attachments
            };

            await SendAsync(emailDto);
        }
    }
}
