using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class TicketDto
    {
            public int Id { get; set; }
            public DateTime PurchaseDate { get; set; }
            public byte[]? QrCode { get; set; }
            public EventDto Event { get; set; }
            public UserDto User { get; set; }
    }
}
