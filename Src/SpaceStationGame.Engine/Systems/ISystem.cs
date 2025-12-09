namespace SpaceStationGame.Engine.Systems;

public interface ISystem
{
    public void Update(double timeStepMs, CancellationToken cancellationToken);

}