namespace FarmSim.Entities;

abstract class Transportation : Storage
{
    public float TransportRate { get; private set; }
}
