using FarmSim.Entities;
using FarmSim.Utils;
using System;

namespace FarmSim.Mobs;

class MobData : IClassData
{
    // with assembly: "namespace.class, assembly"
    public string Class { get; set; }
    public string EntitySpriteKey;
    public int MinSpawned;
    public int MaxSpawned;
    //public int Level;
    public int hp;
    public int xp;
    public int Speed;
    // spawnable locations (locations based on tilesets)
    public string[] Spawnable = Array.Empty<string>();
    // Tags that must match to be identifiable to this creature
    public Tags[] Identifiable = Array.Empty<Tags>();
    public Tags[] Eats = Array.Empty<Tags>();
    public RandomRange Hunger;
    public TagSet[] Tags = Array.Empty<TagSet>();
    public ItemSet Drops = new ItemSet();
}
