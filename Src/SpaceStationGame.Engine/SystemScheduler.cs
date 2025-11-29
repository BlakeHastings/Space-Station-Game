
using System.Reflection.Metadata.Ecma335;

namespace SpaceStationGame.Engine;

public class ScheduledSystem {
    public required ISystem System;
    public required double TimeStepMs;  // 100 = 10Hz
    public required double Accumulator;
}

public class SystemScheduler
{
    public List<ScheduledSystem> Systems { get; } = [];

    public SystemScheduler RegisterSystem(ISystem system, double updatesPerSecond)
    {
        Systems.Add(new ScheduledSystem()
        {
            System = system,
            TimeStepMs = 1000 / updatesPerSecond,
            Accumulator = 0
        });

        return this;
    }

    public void Update(double frameDeltaTime, CancellationToken cancellationToken = default)
    {
        foreach (var scheduledSystem in Systems)
        {
            if(cancellationToken.IsCancellationRequested) return;

            scheduledSystem.Accumulator += frameDeltaTime;
            while (scheduledSystem.Accumulator >= scheduledSystem.TimeStepMs && !cancellationToken.IsCancellationRequested)
            {
                scheduledSystem.System.Update(scheduledSystem.TimeStepMs, cancellationToken);
                scheduledSystem.Accumulator -= scheduledSystem.TimeStepMs;
            }
        }
    }
}