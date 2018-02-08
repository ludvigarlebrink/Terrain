using System.Diagnostics;

public class Profiler
{
    private Stopwatch stopwatch;
    private int times = 0;

    public Profiler()
    {
        stopwatch = new Stopwatch();
    }

    public void Start()
    {
        stopwatch.Start();
        ++times;
    }

    public void Stop()
    {
        stopwatch.Stop();
        ++times;
    }

    public void Reset()
    {
        stopwatch.Reset();
        times = 0;
    }

    public double GetAverage()
    {
        if (times == 0)
        {
            return 0.0d;
        }

        return stopwatch.ElapsedMilliseconds / times;
    }
}
