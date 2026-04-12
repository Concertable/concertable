using System;
using System.Collections.Generic;
using System.Text;

namespace Concertable.Application.Interfaces;

public interface IBackgroundTaskRunner
{
    Task RunAsync(Func<CancellationToken, Task> work);

    Task RunAsync<TService>(
        Func<TService, CancellationToken, Task> work)
        where TService : notnull;
}
