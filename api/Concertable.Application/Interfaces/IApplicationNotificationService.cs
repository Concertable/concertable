public interface IApplicationNotificationService
{
    Task ApplicationAcceptedAsync(string userId, object payload);
}
