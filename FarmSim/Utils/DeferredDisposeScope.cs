using System;
using System.Collections.Generic;

namespace FarmSim.Utils;

class DeferredDisposeScope : IDisposable
{
    public List<IDisposable> Disposables { get; init; } = new();

    public void Dispose()
    {
        foreach (var disposable in Disposables)
        {
            disposable.Dispose();
        }
    }
}
