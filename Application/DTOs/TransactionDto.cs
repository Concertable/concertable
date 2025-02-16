using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class TransactionDto
    {
        public string FromUserEmail { get; set; }
        public decimal Amount { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }
}
