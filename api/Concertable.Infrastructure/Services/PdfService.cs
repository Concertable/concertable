using Application.Interfaces;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace Infrastructure.Services;

public class PdfService : IPdfService
{
    private readonly IQrCodeService qrCodeService;

    public PdfService(IQrCodeService qrCodeService)
    {
        this.qrCodeService = qrCodeService;
    }

    public async Task<byte[]> GenerateTicketReciptAsync(string email, int ticketId)
    {
        byte[] qrCode = await qrCodeService.GetByTicketIdAsync(ticketId);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A5);

                page.Header()
                    .Text($"Ticket")
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
                            column.Item().Image(qrCode);

                        column.Item().Text("Show this QR code at entrance").Italic();
                    });

                page.Footer()
                    .AlignCenter()
                    .Text("Thank you for using Concertable");
            });
        }).GeneratePdf();
    }
}
