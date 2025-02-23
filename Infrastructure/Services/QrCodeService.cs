using Application.Interfaces;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class QrCodeService : IQrCodeService
    {
        private readonly QRCodeGenerator qrCodeGenerator;

        public QrCodeService()
        {
            this.qrCodeGenerator = new QRCodeGenerator();
        }

        public byte[] GenerateFromTicketId(int id)
        {
            QRCodeData qrCodeData = qrCodeGenerator.CreateQrCode(id.ToString(), QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

            return qrCode.GetGraphic(20);
        }
    }
}
