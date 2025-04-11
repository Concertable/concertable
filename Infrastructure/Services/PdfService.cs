using Application.Interfaces;
using Core.Entities;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class PdfService : IPdfService
    {
        private readonly IQrCodeService qrCodeService;
        private readonly Lazy<ITicketService> ticketService;

        public PdfService(IQrCodeService qrCodeService , Lazy<ITicketService> ticketService)
        {
            this.qrCodeService = qrCodeService;
            this.ticketService = ticketService;
        }

        public async Task<byte[]> GenerateTicketReciptAsync(string email, int ticketId)
        {
            byte[] qrCode = await ticketService.Value.GetQrCodeByIdAsync(ticketId);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A5); // Use a small ticket format

                    page.Header()
                        .Text($"🎟 Ticket")
                        .FontSize(20)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .Column(column =>
                        {
                            column.Spacing(10);
                            column.Item().Text($"Email: {email}").FontSize(14);
                            column.Item().Text($"TicketId: {ticketId}").FontSize(14);

                            if (qrCode != null)
                            {
                                column.Item().Image(qrCode);
                            }

                            column.Item().Text("Show this QR code at the entrance").Italic();
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Thank you for using Concertable! 🎶");
                });
            }).GeneratePdf();
        }
    }
}
