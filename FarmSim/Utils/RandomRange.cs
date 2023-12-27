namespace FarmSim.Utils;

class RandomRange
{
    public int Min;
    public int Max;

    public int GetValue()
    {
        return RandomUtil.Rand.Next(Min, Max);
    }
}
