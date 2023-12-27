using System;
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
}
