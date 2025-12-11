using Arch.Core;

namespace SpaceStationGame.Game.Components;

public static class ResourceFactory
{
    // Create a resource entity with a name and weight (kg).
    public static Entity CreateResource(World world, string resourceName, double weightKg)
    {
        return world.Create(new RecourseComponent
        {
            ResourceName = resourceName,
            WeightKg = weightKg
        }, new NameComponent
        {
            Name_1234 = resourceName
        });
    }
}
