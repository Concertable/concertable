namespace Application.Interfaces.Search
{
    public interface ISearchServiceFactory
    {
        ISearchService? Create(string type);
    }
}
