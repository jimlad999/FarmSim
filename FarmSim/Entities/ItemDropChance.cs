namespace FarmSim.Entities;

class ItemDropChance
{
    public string Id;
    public int Count;
    // higher number = lower chance. 1 / n chance. i.e. n = 1 = 100% chance, n = 100 = 1% chance.
    public int Chance;
}
