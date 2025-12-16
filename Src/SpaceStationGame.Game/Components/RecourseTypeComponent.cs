using Arch.Core;

namespace SpaceStationGame.Game.Components;

public struct RecourseTypeComponent
{
    public string RecourseName; // e.g. "iron_ore"
    public double meltingPointCelsius; // e.g. 1560.0 for iron ore
    public Entity stackability ; // reference to a stackability component
}