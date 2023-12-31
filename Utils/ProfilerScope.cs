using System;
using System.Diagnostics;

namespace Utils;

#if DEBUG
public sealed class ProfilerScope : IDisposable
{
    private readonly string _operationName;
    private readonly Stopwatch sw = Stopwatch.StartNew();

    public ProfilerScope(string operationName = "Profiler")
    {
        _operationName = operationName;
    }

    public void Dispose()
    {
        sw.Stop();
        Debug.WriteLine((
            _operationName,
            sw.ElapsedTicks
        ));
    }
}
#endif
