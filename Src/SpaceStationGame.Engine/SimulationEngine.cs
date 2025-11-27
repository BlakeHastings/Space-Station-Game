namespace SpaceStationGame.Engine;

public class SimulationEngine
{
    public const double MAX_FRAME_TIME_MS = 250;
    public const double FIXED_TIMESTEP_MS = 1000.0 / 60.0;

    private readonly TimeProvider _timeProvider;
    
    private long _previousTimestamp;

    public double Accumulator {get; set; } = 0;
    public double SimulationTime {get; set; } = 0;
    public long TickCount {get; set;} = 0;

    public SimulationEngine() : this(TimeProvider.System)
    {
    }

    public SimulationEngine(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _previousTimestamp = _timeProvider.GetTimestamp();
    }

    public void Run(CancellationToken cancellationToken)
    {
        _previousTimestamp = _timeProvider.GetTimestamp();

        while (!cancellationToken.IsCancellationRequested)
        {
            Tick();
        }
    }

    public int Tick()
    {
        long currentTimestamp = _timeProvider.GetTimestamp();
        double frameTime = _timeProvider.GetElapsedTime(_previousTimestamp, currentTimestamp).TotalMilliseconds;
        _previousTimestamp = currentTimestamp;

        // ** This section is to handle when loops slow down. We don't want to rip through all
        // ** frames to catch up when some bloatware app steals CPU time from our engine.

        // take min to only render max 0.25s of frames.
        frameTime = Math.Min(frameTime, MAX_FRAME_TIME_MS);
        // store the actual frametime "Accumulation" to use it to "catch up" to the current frame
        Accumulator += frameTime;

        int ticksThisFrame = 0;
        while (Accumulator >= FIXED_TIMESTEP_MS)
        {
            RunAllSystems(FIXED_TIMESTEP_MS);
            PublishPendingEvents();

            Accumulator -= FIXED_TIMESTEP_MS;
            TickCount += 1;
            SimulationTime += FIXED_TIMESTEP_MS;
            ticksThisFrame++;
        }

        return ticksThisFrame;
    }

    public virtual void RunAllSystems(double fixedTimestep)
    {
        Console.WriteLine($"Running All Systems: {TickCount}:{SimulationTime}:{Accumulator}");
    }

    public virtual void PublishPendingEvents()
    {
        Console.WriteLine($"Publishing Pending Events: {TickCount}:{SimulationTime}:{Accumulator}");
    }
}
