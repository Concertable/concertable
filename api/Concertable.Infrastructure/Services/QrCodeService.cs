using Concertable.Application.Interfaces;
using Concertable.Application.Exceptions;
using QRCoder;

namespace Concertable.Infrastructure.Services;

public class QrCodeService : IQrCodeService
{
    private readonly QRCodeGenerator qrCodeGenerator;
    private readonly ITicketRepository ticketRepository;

    public QrCodeService(QRCodeGenerator qrCodeGenerator, ITicketRepository ticketRepository)
    {
        this.qrCodeGenerator = qrCodeGenerator;
        this.ticketRepository = ticketRepository;
    }

    public byte[] GenerateFromTicketId(int id)
    {
        QRCodeData qrCodeData = qrCodeGenerator.CreateQrCode(id.ToString(), QRCodeGenerator.ECCLevel.Q);
        PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }

    public async Task<byte[]> GetByTicketIdAsync(int ticketId)
    {
        return await ticketRepository.GetQrCodeByIdAsync(ticketId)
            ?? throw new NotFoundException("QR Code not found");
    }
}
