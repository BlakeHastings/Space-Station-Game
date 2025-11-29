using SpaceStationGame.Engine;

namespace SpaceStationGame.Game;

public class ExampleSystem1 : ISystem
{
    public void Update(double timeStepMs, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(ExampleSystem1)} Update Call {timeStepMs}");
    }
}