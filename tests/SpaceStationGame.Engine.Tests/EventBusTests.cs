using Moq;
using SpaceStationGame.Engine.Events;
using Xunit;

namespace SpaceStationGame.Engine.Tests;

public class EventBusTests
{
    // Test event data implementations
    private class TestEvent1 : IEventData
    {
        public static int EventTypeId => 1;
        public string Message { get; init; } = "";
    }

    private class TestEvent2 : IEventData
    {
        public static int EventTypeId => 2;
        public int Value { get; init; }
    }

    // Update Tests
    [Fact]
    public void Update_SetsCurrentTick()
    {
        var eventBus = new EventBus();

        eventBus.Update(42, 1000);

        Assert.Equal(42, eventBus.CurrentTick);
    }

    [Fact]
    public void Update_SetsCurrentTime()
    {
        var eventBus = new EventBus();

        eventBus.Update(42, 1000);

        Assert.Equal(1000, eventBus.CurrentTime);
    }

    // Emit Tests
    [Fact]
    public void Emit_AddsEventToPendingEvents()
    {
        var eventBus = new EventBus();

        eventBus.Emit(new TestEvent1 { Message = "Hello" });

        Assert.Single(eventBus.PendingEvents);
    }

    [Fact]
    public void Emit_CreatesEnvelopeWithCurrentTick()
    {
        var eventBus = new EventBus();
        eventBus.Update(currentTick: 5, currentTime: 100);

        eventBus.Emit(new TestEvent1());

        Assert.Equal(5, eventBus.PendingEvents[0].Tick);
    }

    [Fact]
    public void Emit_CreatesEnvelopeWithCurrentTime()
    {
        var eventBus = new EventBus();
        eventBus.Update(currentTick: 5, currentTime: 100);

        eventBus.Emit(new TestEvent1());

        Assert.Equal(100, eventBus.PendingEvents[0].SimulationTime);
    }

    [Fact]
    public void Emit_CreatesEnvelopeWithCorrectEventTypeId()
    {
        var eventBus = new EventBus();

        eventBus.Emit(new TestEvent1());

        Assert.Equal(TestEvent1.EventTypeId, eventBus.PendingEvents[0].EventTypeId);
    }

    [Fact]
    public void Emit_CreatesEnvelopeWithEventData()
    {
        var eventBus = new EventBus();
        var testEvent = new TestEvent1 { Message = "Test Message" };

        eventBus.Emit(testEvent);

        Assert.Same(testEvent, eventBus.PendingEvents[0].EventData);
    }

    [Fact]
    public void Emit_MultipleEvents_AddsAllToPendingEvents()
    {
        var eventBus = new EventBus();

        eventBus.Emit(new TestEvent1 { Message = "First" });
        eventBus.Emit(new TestEvent2 { Value = 42 });
        eventBus.Emit(new TestEvent1 { Message = "Third" });

        Assert.Equal(3, eventBus.PendingEvents.Count);
    }

    // RegisterChannel Tests
    [Fact]
    public void RegisterChannel_AddsChannelToList()
    {
        var eventBus = new EventBus();
        var mockChannel = new Mock<IEventChannel>();

        eventBus.RegisterChannel(mockChannel.Object);

        Assert.Single(eventBus.Channels);
        Assert.Same(mockChannel.Object, eventBus.Channels[0]);
    }

    [Fact]
    public void RegisterChannel_MultipleChannels_AddsAllToList()
    {
        var eventBus = new EventBus();
        var channel1 = new Mock<IEventChannel>();
        var channel2 = new Mock<IEventChannel>();

        eventBus.RegisterChannel(channel1.Object);
        eventBus.RegisterChannel(channel2.Object);

        Assert.Equal(2, eventBus.Channels.Count);
    }

    // Flush Tests
    [Fact]
    public void Flush_WithPendingEvents_SendsToAllChannels()
    {
        var eventBus = new EventBus();
        var channel1 = new Mock<IEventChannel>();
        var channel2 = new Mock<IEventChannel>();
        eventBus.RegisterChannel(channel1.Object);
        eventBus.RegisterChannel(channel2.Object);

        eventBus.Emit(new TestEvent1());
        eventBus.Flush();

        channel1.Verify(c => c.ReceiveBatch(It.IsAny<List<EventEnvelope>>()), Times.Once());
        channel2.Verify(c => c.ReceiveBatch(It.IsAny<List<EventEnvelope>>()), Times.Once());
    }

    [Fact]
    public void Flush_ClearsPendingEvents()
    {
        var eventBus = new EventBus();

        eventBus.Emit(new TestEvent1());
        eventBus.Emit(new TestEvent2 { Value = 1 });
        eventBus.Flush();

        Assert.Empty(eventBus.PendingEvents);
    }

    [Fact]
    public void Flush_WithNoPendingEvents_DoesNotCallChannels()
    {
        var eventBus = new EventBus();
        var mockChannel = new Mock<IEventChannel>();
        eventBus.RegisterChannel(mockChannel.Object);

        eventBus.Flush();

        mockChannel.Verify(c => c.ReceiveBatch(It.IsAny<List<EventEnvelope>>()), Times.Never());
    }

    [Fact]
    public void Flush_CalledTwice_SecondCallDoesNothing()
    {
        var eventBus = new EventBus();
        var mockChannel = new Mock<IEventChannel>();
        eventBus.RegisterChannel(mockChannel.Object);

        eventBus.Emit(new TestEvent1());
        eventBus.Flush();
        eventBus.Flush(); // Second call should be safe and do nothing

        mockChannel.Verify(c => c.ReceiveBatch(It.IsAny<List<EventEnvelope>>()), Times.Once());
    }

    [Fact]
    public void Flush_ChannelsReceiveSameListReference()
    {
        var eventBus = new EventBus();
        List<EventEnvelope>? receivedList1 = null;
        List<EventEnvelope>? receivedList2 = null;

        var channel1 = new Mock<IEventChannel>();
        channel1.Setup(c => c.ReceiveBatch(It.IsAny<List<EventEnvelope>>()))
            .Callback<List<EventEnvelope>>(list => receivedList1 = list);

        var channel2 = new Mock<IEventChannel>();
        channel2.Setup(c => c.ReceiveBatch(It.IsAny<List<EventEnvelope>>()))
            .Callback<List<EventEnvelope>>(list => receivedList2 = list);

        eventBus.RegisterChannel(channel1.Object);
        eventBus.RegisterChannel(channel2.Object);

        eventBus.Emit(new TestEvent1());
        eventBus.Flush();

        Assert.NotNull(receivedList1);
        Assert.NotNull(receivedList2);
        Assert.Same(receivedList1, receivedList2);
    }

    [Fact]
    public void Flush_SendsAllPendingEvents()
    {
        var eventBus = new EventBus();
        List<EventEnvelope>? receivedEvents = null;

        var mockChannel = new Mock<IEventChannel>();
        mockChannel.Setup(c => c.ReceiveBatch(It.IsAny<List<EventEnvelope>>()))
            .Callback<List<EventEnvelope>>(list => receivedEvents = list);

        eventBus.RegisterChannel(mockChannel.Object);

        eventBus.Emit(new TestEvent1 { Message = "First" });
        eventBus.Emit(new TestEvent2 { Value = 42 });
        eventBus.Flush();

        Assert.NotNull(receivedEvents);
        Assert.Equal(2, receivedEvents.Count);
    }

    [Fact]
    public void Flush_WithNoChannels_ClearsPendingEventsWithoutError()
    {
        var eventBus = new EventBus();

        eventBus.Emit(new TestEvent1());
        var exception = Record.Exception(() => eventBus.Flush());

        Assert.Null(exception);
        Assert.Empty(eventBus.PendingEvents);
    }

    // Integration-style Tests
    [Fact]
    public void EmitAndFlush_PreservesEventOrder()
    {
        var eventBus = new EventBus();
        List<EventEnvelope>? receivedEvents = null;

        var mockChannel = new Mock<IEventChannel>();
        mockChannel.Setup(c => c.ReceiveBatch(It.IsAny<List<EventEnvelope>>()))
            .Callback<List<EventEnvelope>>(list => receivedEvents = list);

        eventBus.RegisterChannel(mockChannel.Object);

        eventBus.Emit(new TestEvent1 { Message = "First" });
        eventBus.Emit(new TestEvent1 { Message = "Second" });
        eventBus.Emit(new TestEvent1 { Message = "Third" });
        eventBus.Flush();

        Assert.NotNull(receivedEvents);
        Assert.Equal("First", ((TestEvent1)receivedEvents[0].EventData).Message);
        Assert.Equal("Second", ((TestEvent1)receivedEvents[1].EventData).Message);
        Assert.Equal("Third", ((TestEvent1)receivedEvents[2].EventData).Message);
    }

    [Fact]
    public void MultipleFlushCycles_WorkIndependently()
    {
        var eventBus = new EventBus();
        var receivedBatches = new List<List<EventEnvelope>>();

        var mockChannel = new Mock<IEventChannel>();
        mockChannel.Setup(c => c.ReceiveBatch(It.IsAny<List<EventEnvelope>>()))
            .Callback<List<EventEnvelope>>(list => receivedBatches.Add(new List<EventEnvelope>(list)));

        eventBus.RegisterChannel(mockChannel.Object);

        // First cycle
        eventBus.Update(1, 100);
        eventBus.Emit(new TestEvent1 { Message = "Batch1" });
        eventBus.Flush();

        // Second cycle
        eventBus.Update(2, 200);
        eventBus.Emit(new TestEvent2 { Value = 42 });
        eventBus.Flush();

        Assert.Equal(2, receivedBatches.Count);
        Assert.Single(receivedBatches[0]);
        Assert.Single(receivedBatches[1]);
        Assert.Equal(1, receivedBatches[0][0].Tick);
        Assert.Equal(2, receivedBatches[1][0].Tick);
    }
}
