namespace FarmSim.Utils;

class ProjectileData : IClassData
{
    // with assembly: "namespace.class, assembly"
    public string Class { get; set; }
    public string EntitySpriteKey;
    public double Speed;
    public int HitRadiusPow2;
}
