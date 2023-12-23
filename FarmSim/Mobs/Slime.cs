namespace FarmSim.Mobs;

class Slime : Mob
{
    public override void Init()
    {
        _behaviours = new[]
        {
            new RandomWonderBehaviour(wonderDistance: Metadata.Speed * 5, validTerrain: Metadata.Spawnable, maxWaitTimeMilliseconds: 10_000)
        };
    }
}
