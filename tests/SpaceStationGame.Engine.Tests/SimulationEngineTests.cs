using Microsoft.Extensions.Time.Testing;
using Moq;
using SpaceStationGame.Engine;
using SpaceStationGame.Engine.Systems;
using Xunit;

namespace SpaceStationGame.Engine.Tests;

public class SimulationEngineTests
{
    // Use exact milliseconds to avoid floating point issues with FakeTimeProvider
    private const int TIMESTEP_MS = 17; // Slightly more than FIXED_TIMESTEP_MS (~16.67ms)

    [Fact]
    public void Constructor_InitializesWithZeroState()
    {
        var timeProvider = new FakeTimeProvider();
        var scheduler = new SystemScheduler();
        var engine = new SimulationEngine(timeProvider, scheduler);

        Assert.Equal(0, engine.TickCount);
        Assert.Equal(0, engine.SimulationTime);
        Assert.Equal(0, engine.Accumulator);
    }

    [Fact]
    public void Tick_WithNoElapsedTime_DoesNotAdvanceSimulation()
    {
        var timeProvider = new FakeTimeProvider();
        var scheduler = new SystemScheduler();
        var engine = new SimulationEngine(timeProvider, scheduler);

        // First tick syncs the timestamp
        engine.Tick();

        // Second tick with no time advancement
        int ticksExecuted = engine.Tick();

        Assert.Equal(0, ticksExecuted);
        Assert.Equal(0, engine.TickCount);
        Assert.Equal(0, engine.SimulationTime);
    }

    [Fact]
    public void Tick_WithOneFrameElapsed_ExecutesOneTick()
    {
        var timeProvider = new FakeTimeProvider();
        var scheduler = new SystemScheduler();
        var engine = new SimulationEngine(timeProvider, scheduler);

        // Sync timestamp
        engine.Tick();

        // Advance time by enough for one tick
        timeProvider.Advance(TimeSpan.FromMilliseconds(TIMESTEP_MS));
        int ticksExecuted = engine.Tick();

        Assert.Equal(1, ticksExecuted);
        Assert.Equal(1, engine.TickCount);
    }

    [Fact]
    public void Tick_WithMultipleFramesElapsed_ExecutesMultipleTicks()
    {
        var timeProvider = new FakeTimeProvider();
        var scheduler = new SystemScheduler();
        var engine = new SimulationEngine(timeProvider, scheduler);

        // Sync timestamp
        engine.Tick();

        // Advance time by 51ms (enough for 3 ticks at ~16.67ms each: 51/16.67 = 3.06)
        timeProvider.Advance(TimeSpan.FromMilliseconds(51));
        int ticksExecuted = engine.Tick();

        Assert.Equal(3, ticksExecuted);
        Assert.Equal(3, engine.TickCount);
    }

    [Fact]
    public void Tick_WithPartialFrame_AccumulatesTime()
    {
        var timeProvider = new FakeTimeProvider();
        var scheduler = new SystemScheduler();
        var engine = new SimulationEngine(timeProvider, scheduler);

        // Sync timestamp
        engine.Tick();

        // Advance by 10ms (less than 16.67ms timestep)
        timeProvider.Advance(TimeSpan.FromMilliseconds(10));
        int ticksExecuted = engine.Tick();

        Assert.Equal(0, ticksExecuted);
        Assert.Equal(0, engine.TickCount);
        Assert.True(engine.Accumulator > 0);
    }

    [Fact]
    public void Tick_AccumulatesPartialFramesOverMultipleCalls()
    {
        var timeProvider = new FakeTimeProvider();
        var scheduler = new SystemScheduler();
        var engine = new SimulationEngine(timeProvider, scheduler);

        // Sync timestamp
        engine.Tick();

        // First call: 10ms (partial)
        timeProvider.Advance(TimeSpan.FromMilliseconds(10));
        engine.Tick();
        Assert.Equal(0, engine.TickCount);

        // Second call: another 10ms (total 20ms, enough for one tick)
        timeProvider.Advance(TimeSpan.FromMilliseconds(10));
        int ticksExecuted = engine.Tick();

        Assert.Equal(1, ticksExecuted);
        Assert.Equal(1, engine.TickCount);
    }

    [Fact]
    public void Tick_ClampsFrameTimeToMaximum()
    {
        var timeProvider = new FakeTimeProvider();
        var scheduler = new SystemScheduler();
        var engine = new SimulationEngine(timeProvider, scheduler);

        // Sync timestamp
        engine.Tick();

        // Advance time by 1 second (way more than MAX_DELTA_TIME_MS of 250ms)
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        int ticksExecuted = engine.Tick();

        // Should only process MAX_DELTA_TIME_MS worth of ticks
        // 250ms / 16.666ms = 15 ticks
        Assert.Equal(15, ticksExecuted);
    }

    [Fact]
    public void Tick_WithExactlyMaxFrameTime_ProcessesExpectedTicks()
    {
        var timeProvider = new FakeTimeProvider();
        var scheduler = new SystemScheduler();
        var engine = new SimulationEngine(timeProvider, scheduler);

        // Sync timestamp
        engine.Tick();

        timeProvider.Advance(TimeSpan.FromMilliseconds(250));
        int ticksExecuted = engine.Tick();

        // 250ms / 16.666ms = 15 ticks
        Assert.Equal(15, ticksExecuted);
    }

    [Fact]
    public void Run_RespectsCancel()
    {
        var timeProvider = new FakeTimeProvider();
        var scheduler = new SystemScheduler();
        var engine = new SimulationEngine(timeProvider, scheduler);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Should return immediately without hanging
        engine.Run(cts.Token);

        Assert.Equal(0, engine.TickCount);
    }

    [Fact]
    public void SimulationTime_AccumulatesCorrectly()
    {
        var timeProvider = new FakeTimeProvider();
        var scheduler = new SystemScheduler();
        var engine = new SimulationEngine(timeProvider, scheduler);

        // Sync timestamp
        engine.Tick();

        // Advance by a specific amount and verify simulation time matches tick count * timestep
        // Using 250ms (max frame time) to ensure we don't hit the clamp
        timeProvider.Advance(TimeSpan.FromMilliseconds(200));
        engine.Tick();

        // 200ms / 16.67ms = 12 ticks
        Assert.Equal(12, engine.TickCount);
        // Simulation time should be 12 * FIXED_TIMESTEP_MS = ~200ms
        double expectedSimTime = 12 * SimulationEngine.FIXED_TIMESTEP_MS;
        Assert.Equal(expectedSimTime, engine.SimulationTime, precision: 1);
    }

    [Fact]
    public void Tick_CallsSystemSchedulerUpdate()
    {
        var timeProvider = new FakeTimeProvider();
        var mockSystem = new Mock<ISystem>();
        var scheduler = new SystemScheduler();
        scheduler.RegisterSystem(mockSystem.Object, 60); // Match engine's 60Hz

        var engine = new SimulationEngine(timeProvider, scheduler);

        // Sync timestamp
        engine.Tick();

        timeProvider.Advance(TimeSpan.FromMilliseconds(TIMESTEP_MS));
        engine.Tick();

        // System should have been called once via the scheduler
        mockSystem.Verify(s => s.Update(It.IsAny<double>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public void Tick_CallsSystemSchedulerCorrectNumberOfTimes()
    {
        var timeProvider = new FakeTimeProvider();
        var mockSystem = new Mock<ISystem>();
        var scheduler = new SystemScheduler();
        scheduler.RegisterSystem(mockSystem.Object, 60); // Match engine's 60Hz

        var engine = new SimulationEngine(timeProvider, scheduler);

        // Sync timestamp
        engine.Tick();

        // Advance by 51ms (enough for 3 ticks: 51/16.67 = 3.06)
        timeProvider.Advance(TimeSpan.FromMilliseconds(51));
        engine.Tick();

        // System should have been called 3 times via the scheduler
        mockSystem.Verify(s => s.Update(It.IsAny<double>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public void FixedTimestep_IsApproximately16Point67Ms()
    {
        // 1000ms / 60fps = 16.666...ms
        Assert.Equal(1000.0 / 60.0, SimulationEngine.FIXED_TIMESTEP_MS, precision: 10);
    }

    [Fact]
    public void MaxFrameTime_Is250Ms()
    {
        Assert.Equal(250.0, SimulationEngine.MAX_DELTA_TIME_MS);
    }

    [Fact]
    public void Tick_ReturnsCorrectTickCount()
    {
        var timeProvider = new FakeTimeProvider();
        var scheduler = new SystemScheduler();
        var engine = new SimulationEngine(timeProvider, scheduler);

        // No time passed - returns 0
        int result1 = engine.Tick();
        Assert.Equal(0, result1);

        // Advance 100ms - should get 5 ticks (100 / 16.67 = 5.99..., floor = 5)
        // The accumulator keeps the remaining ~16.6ms for the next frame
        timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        int result2 = engine.Tick();
        Assert.Equal(5, result2);
    }
}
