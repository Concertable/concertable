
namespace Concertable.Identity.Application.Interfaces;

internal interface IManagerRepository<T> : IGuidRepository<T> where T : ManagerEntity { }
