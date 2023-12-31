using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Entities;

static class TagExtentions
{
    public static Tags[] PickTags(this TagSet[] tags)
    {
        if (tags.Length == 0)
        {
            return Array.Empty<Tags>();
        }
        return tags.Select(set => set.PickTag()).Where(tag => tag != Tags.None).ToArray();
    }

    public static bool Contains(this ICollection<Tags> source, IEnumerable<Tags> matchingCollection)
    {
        return matchingCollection.Any(source.Contains);
    }
}
