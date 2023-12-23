using FarmSim.Entities;
using FarmSim.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Mobs;

class MobData
{
    // with assembly: "namespace.class, assembly"
    public string Class;
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
    // higher number = higher chance of being Tag chosen from set. Base of 1. 2 = twice as likely, etc.
    public Dictionary<Tags, int>[] Tags = Array.Empty<Dictionary<Tags, int>>();

    public Tags[] GetTags()
    {
        if (Tags.Length == 0)
        {
            return Array.Empty<Tags>();
        }
        var tags = new Tags[Tags.Length];
        for (var i = 0; i < tags.Length; ++i)
        {
            var tagChances = Tags[i];
            if (tagChances.Count == 1)
            {
                tags[i] = tagChances.Keys.First();
            }
            else
            {
                var tagSet = new List<Tags>();
                foreach (var (tag, chance) in tagChances)
                {
                    tagSet.AddRange(Enumerable.Repeat(tag, chance));
                }
                tagSet.Shuffle();
                tags[i] = tagSet.PickRandom();
            }
        }
        return tags;
    }
}
