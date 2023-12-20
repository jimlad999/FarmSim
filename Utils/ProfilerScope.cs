using System;
using System.Diagnostics;

namespace Utils;

public sealed class ProfilerScope : IDisposable
{
    private Stopwatch sw = Stopwatch.StartNew();

    public void Dispose()
    {
        sw.Stop();
        Debug.WriteLine((
            $"Profiler",
            sw.ElapsedTicks
        ));
    }
}
