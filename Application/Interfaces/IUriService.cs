using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUriService
    {
        Uri GetEmailConfirmationUri(int userId, string token);
        Uri GetPasswordResetUri(int userId, string token);
    }
}
