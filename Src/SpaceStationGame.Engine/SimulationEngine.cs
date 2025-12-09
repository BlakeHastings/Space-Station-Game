using SpaceStationGame.Engine.Events;
using SpaceStationGame.Engine.Systems;

namespace SpaceStationGame.Engine;

public class SimulationEngine
{
    public const double MAX_DELTA_TIME_MS = 250;
    public const double FIXED_TIMESTEP_MS = 1000.0 / 60.0;
    private long _previousTimestamp;

    private readonly SystemScheduler _systemScheduler;
    private readonly EventBus _eventBus;
    private readonly TimeProvider _timeProvider;

    public double Accumulator {get; private set; } = 0;
    public double SimulationTime {get; private set; } = 0;
    public long TickCount {get; private set;} = 0;

    public SimulationEngine() : this(TimeProvider.System, new(), new()) {}
    public SimulationEngine(SystemScheduler systemScheduler, EventBus eventBus) : this(TimeProvider.System, systemScheduler, eventBus) {}

    public SimulationEngine(
        TimeProvider timeProvider, 
        SystemScheduler systemScheduler,
        EventBus eventBus)
    {
        _timeProvider = timeProvider;
        _previousTimestamp = _timeProvider.GetTimestamp();
        _systemScheduler = systemScheduler;
        _eventBus = eventBus;
    }

    public void Run(CancellationToken cancellationToken)
    {
        _previousTimestamp = _timeProvider.GetTimestamp();

        while (!cancellationToken.IsCancellationRequested)
        {
            Tick(cancellationToken);
            Thread.Sleep(1); // avoid spinning CPU 100%
        }
    }

    public int Tick(CancellationToken cancellationToken = default)
    {
        long currentTimestamp = _timeProvider.GetTimestamp();
        double deltaTime = _timeProvider.GetElapsedTime(_previousTimestamp, currentTimestamp).TotalMilliseconds;
        _previousTimestamp = currentTimestamp;

        // ** This section is to handle when loops slow down. We don't want to rip through all
        // ** frames to catch up when some bloatware app steals CPU time from our engine.

        // take min to only render max 0.25s of frames.
        deltaTime = Math.Min(deltaTime, MAX_DELTA_TIME_MS);
        // store the actual frametime "Accumulation" to use it to "catch up" to the current frame
        Accumulator += deltaTime;

        int ticksThisFrame = 0;
        while (Accumulator >= FIXED_TIMESTEP_MS)
        {
            _systemScheduler.Update(FIXED_TIMESTEP_MS, cancellationToken);
            _eventBus.Update(TickCount, currentTimestamp);
            _eventBus.Flush();

            Accumulator -= FIXED_TIMESTEP_MS;
            TickCount += 1;
            SimulationTime += FIXED_TIMESTEP_MS;
            ticksThisFrame++;
        }

        return ticksThisFrame;
    }

    public virtual void PublishPendingEvents()
    {
        Console.WriteLine($"Publishing Pending Events: {TickCount}:{SimulationTime}:{Accumulator}");
    }
}
