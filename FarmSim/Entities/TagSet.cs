using FarmSim.Utils;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Entities;

// higher number = higher chance of being Tag chosen from set. Base of 1. 2 = twice as likely, etc.
class TagSet : Dictionary<Tags, int>
{
    public Tags PickTag()
    {
        if (Count == 1)
        {
            return Keys.First();
        }
        else
        {
            var tagSet = new List<Tags>();
            foreach (var (tag, chance) in this)
            {
                tagSet.AddRange(Enumerable.Repeat(tag, chance));
            }
            tagSet.Shuffle();
            return tagSet.PickRandom();
        }
    }
}
