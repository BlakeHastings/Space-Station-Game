using Moq;
using SpaceStationGame.Engine;
using Xunit;

namespace SpaceStationGame.Engine.Tests;

public class SystemSchedulerTests
{
    // Registration Tests
    [Fact]
    public void RegisterSystem_AddsSystemToList()
    {
        var mockSystem = new Mock<ISystem>();
        var scheduler = new SystemScheduler();

        scheduler.RegisterSystem(mockSystem.Object, 10);

        Assert.Single(scheduler.Systems);
        Assert.Same(mockSystem.Object, scheduler.Systems[0].System);
    }

    [Fact]
    public void RegisterSystem_CalculatesCorrectTimeStep()
    {
        var mockSystem = new Mock<ISystem>();
        var scheduler = new SystemScheduler();

        scheduler.RegisterSystem(mockSystem.Object, 10); // 10 updates/sec = 100ms timestep

        Assert.Equal(100, scheduler.Systems[0].TimeStepMs);
    }

    [Fact]
    public void RegisterSystem_InitializesAccumulatorToZero()
    {
        var mockSystem = new Mock<ISystem>();
        var scheduler = new SystemScheduler();

        scheduler.RegisterSystem(mockSystem.Object, 10);

        Assert.Equal(0, scheduler.Systems[0].Accumulator);
    }

    [Fact]
    public void RegisterSystem_ReturnsSelf()
    {
        var mockSystem = new Mock<ISystem>();
        var scheduler = new SystemScheduler();

        var result = scheduler.RegisterSystem(mockSystem.Object, 10);

        Assert.Same(scheduler, result);
    }

    // Update Timing Tests
    [Fact]
    public void Update_WithInsufficientTime_DoesNotCallSystem()
    {
        var mockSystem = new Mock<ISystem>();
        var scheduler = new SystemScheduler();
        scheduler.RegisterSystem(mockSystem.Object, 10); // 100ms timestep

        scheduler.Update(50); // Only 50ms, not enough for a tick

        mockSystem.Verify(s => s.Update(It.IsAny<double>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Update_WithExactlyOneTimestep_CallsSystemOnce()
    {
        var mockSystem = new Mock<ISystem>();
        var scheduler = new SystemScheduler();
        scheduler.RegisterSystem(mockSystem.Object, 10); // 100ms timestep

        scheduler.Update(100);

        mockSystem.Verify(s => s.Update(100, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Update_WithMultipleTimesteps_CallsSystemMultipleTimes()
    {
        var mockSystem = new Mock<ISystem>();
        var scheduler = new SystemScheduler();
        scheduler.RegisterSystem(mockSystem.Object, 10); // 100ms timestep

        scheduler.Update(350); // Enough for 3 ticks (300ms), 50ms remainder

        mockSystem.Verify(s => s.Update(100, It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public void Update_AccumulatesPartialFrames()
    {
        var mockSystem = new Mock<ISystem>();
        var scheduler = new SystemScheduler();
        scheduler.RegisterSystem(mockSystem.Object, 10); // 100ms timestep

        scheduler.Update(60); // Not enough
        mockSystem.Verify(s => s.Update(It.IsAny<double>(), It.IsAny<CancellationToken>()), Times.Never);

        scheduler.Update(60); // Now 120ms total, enough for 1 tick
        mockSystem.Verify(s => s.Update(100, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Update_PreservesRemainderInAccumulator()
    {
        var mockSystem = new Mock<ISystem>();
        var scheduler = new SystemScheduler();
        scheduler.RegisterSystem(mockSystem.Object, 10); // 100ms timestep

        scheduler.Update(150); // 1 tick, 50ms remainder

        Assert.Equal(50, scheduler.Systems[0].Accumulator);
    }

    // Multiple Systems Tests
    [Fact]
    public void Update_CallsAllRegisteredSystems()
    {
        var mockSystem1 = new Mock<ISystem>();
        var mockSystem2 = new Mock<ISystem>();
        var scheduler = new SystemScheduler();
        scheduler.RegisterSystem(mockSystem1.Object, 10); // 100ms timestep
        scheduler.RegisterSystem(mockSystem2.Object, 10); // 100ms timestep

        scheduler.Update(100);

        mockSystem1.Verify(s => s.Update(100, It.IsAny<CancellationToken>()), Times.Once);
        mockSystem2.Verify(s => s.Update(100, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Update_SystemsCanHaveDifferentUpdateRates()
    {
        var fastSystem = new Mock<ISystem>(); // 100Hz = 10ms timestep
        var slowSystem = new Mock<ISystem>(); // 10Hz = 100ms timestep
        var scheduler = new SystemScheduler();
        scheduler.RegisterSystem(fastSystem.Object, 100);
        scheduler.RegisterSystem(slowSystem.Object, 10);

        scheduler.Update(100); // 10 fast ticks, 1 slow tick

        fastSystem.Verify(s => s.Update(10, It.IsAny<CancellationToken>()), Times.Exactly(10));
        slowSystem.Verify(s => s.Update(100, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Cancellation Tests
    [Fact]
    public void Update_StopsImmediatelyWhenCancelled()
    {
        var mockSystem = new Mock<ISystem>();
        var scheduler = new SystemScheduler();
        scheduler.RegisterSystem(mockSystem.Object, 10); // 100ms timestep

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        scheduler.Update(500, cts.Token);

        mockSystem.Verify(s => s.Update(It.IsAny<double>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Update_StopsMidLoopWhenCancelled()
    {
        var mockSystem = new Mock<ISystem>();
        var scheduler = new SystemScheduler();
        scheduler.RegisterSystem(mockSystem.Object, 10); // 100ms timestep

        using var cts = new CancellationTokenSource();

        // Cancel after first call
        mockSystem
            .Setup(s => s.Update(It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .Callback(() => cts.Cancel());

        scheduler.Update(500, cts.Token); // Would be 5 ticks without cancellation

        mockSystem.Verify(s => s.Update(100, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Edge Cases
    [Fact]
    public void Update_WithZeroDeltaTime_DoesNothing()
    {
        var mockSystem = new Mock<ISystem>();
        var scheduler = new SystemScheduler();
        scheduler.RegisterSystem(mockSystem.Object, 10);

        scheduler.Update(0);

        mockSystem.Verify(s => s.Update(It.IsAny<double>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(0, scheduler.Systems[0].Accumulator);
    }

    [Fact]
    public void Update_WithNoRegisteredSystems_DoesNotThrow()
    {
        var scheduler = new SystemScheduler();

        var exception = Record.Exception(() => scheduler.Update(100));

        Assert.Null(exception);
    }
}
