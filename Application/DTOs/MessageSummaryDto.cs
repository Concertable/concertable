using Core.Entities;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class MessageSummaryDto
    {
        public PaginationResponse<MessageDto> Messages { get; set; }
        public int UnreadCount { get; set; }
    }
}
