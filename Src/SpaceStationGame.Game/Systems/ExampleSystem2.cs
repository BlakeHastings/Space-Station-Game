using SpaceStationGame.Engine;

namespace SpaceStationGame.Game;

public class ExampleSystem2 : ISystem
{
    public void Update(double timeStepMs, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(ExampleSystem2)} Update Call {timeStepMs}");
    }
}