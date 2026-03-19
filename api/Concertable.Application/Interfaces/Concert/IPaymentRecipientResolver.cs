using Core.Entities;

namespace Application.Interfaces.Concert;

public interface IPaymentRecipientResolver
{
    Task<UserEntity> ResolveAsync(int concertId);
}
