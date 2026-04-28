
namespace Concertable.User.Application.Interfaces;

internal interface IManagerRepository<T> : IGuidRepository<T> where T : ManagerEntity { }
