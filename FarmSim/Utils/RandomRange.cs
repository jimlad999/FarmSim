namespace FarmSim.Utils;

class RandomRange
{
    public int Min;
    public int Max;

    public int GetValue()
    {
        return Min == Max ? Max : RandomUtil.Rand.Next(Min, Max);
    }
}
