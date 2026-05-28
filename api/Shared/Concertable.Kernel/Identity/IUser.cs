namespace Concertable.Kernel.Identity;

public interface IUser
{
    Guid Id { get; set; }
    string Email { get; set; }
    Role Role { get; }
    double? Latitude { get; set; }
    double? Longitude { get; set; }
    string? County { get; set; }
    string? Town { get; set; }
    string BaseUrl { get; set; }
    bool IsEmailVerified { get; set; }
}
