using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IUriService
{
    Uri GetEmailConfirmationUri(Guid userId, string token);
    Uri GetEmailChangeConfirmationUri(Guid userId, string token, string newEmail);
    Uri GetPasswordResetUri(Guid userId, string token);
}
