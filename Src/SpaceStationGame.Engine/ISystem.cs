namespace SpaceStationGame.Engine;

public interface ISystem
{
    public void Update(double timeStepMs, CancellationToken cancellationToken);

}