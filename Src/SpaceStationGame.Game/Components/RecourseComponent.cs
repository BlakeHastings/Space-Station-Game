using Arch.Core;

namespace SpaceStationGame.Game.Components;

public struct RecourseComponent
{
    // Resource identifier, e.g. "iron_ore"
    public Entity RecourseTypeComponent;

    // Weight in kilograms
    public double WeightKg;

}