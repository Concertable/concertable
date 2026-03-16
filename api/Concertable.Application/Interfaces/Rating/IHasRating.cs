namespace Application.Interfaces.Rating;

public interface IHasRating
{
    int Id { get; set; }
    double? Rating { get; set; }
}
