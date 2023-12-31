namespace FarmSim.Mobs;

class Slime : Mob
{
    public override void InitDefaultBehaviours()
    {
        Behaviours = new()
        {
            new RandomWonderBehaviour(wonderDistance: Metadata.Speed * 5, validTerrain: Metadata.Spawnable, maxWaitTimeMilliseconds: 10_000)
        };
    }
}
