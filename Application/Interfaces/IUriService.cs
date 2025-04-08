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
        Uri GetEmailChangeConfirmationUri(int userId, string token, string newEmail);
        Uri GetPasswordResetUri(int userId, string token);
    }
}
