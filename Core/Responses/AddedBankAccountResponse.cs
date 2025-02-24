using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Responses
{
    public class AddedBankAccountResponse
    {
        public string AccountId { get; set; }
        public string? RedirectUri { get; set; }
    }
}
