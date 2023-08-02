namespace FarmSim.Utils;

static class ModUtils
{
    public static int Mod(this int value, int mod)
    {
        var remainder = value % mod;
        return remainder < 0 ? remainder + mod : remainder;
    }
}
