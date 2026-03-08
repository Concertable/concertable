namespace Application.Interfaces.Search
{
    public interface IHeaderServiceFactory
    {
        IHeaderService? Create(string type);
    }
}
