using Arch.Core;

namespace SpaceStationGame.Game.Components;


public struct RecipeComponent
{
    public Entity[] Ingredients;
    public Entity[] Products;

    public double SpeedMultiplier;
    public double EfficiencyMultiplier;
}
