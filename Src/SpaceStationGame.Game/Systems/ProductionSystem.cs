using SpaceStationGame.Engine;
using SpaceStationGame.Engine.Systems;

namespace SpaceStationGame.Game;

public class ProductionSystem : ISystem
{
    public void Update(double timeStepMs, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(ProductionSystem)} Update Call {timeStepMs}");
    }
}