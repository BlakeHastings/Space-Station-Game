using SpaceStationGame.Engine;
using SpaceStationGame.Engine.Systems;

namespace SpaceStationGame.Game;

public class PopSystem : ISystem
{
    public void Update(double timeStepMs, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(PopSystem)} Update Call {timeStepMs}");
    }
}