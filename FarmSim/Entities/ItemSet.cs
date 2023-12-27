using FarmSim.Utils;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Entities;

class ItemSet : List<ItemDropChance>
{
    public List<string> PickItems()
    {
        var items = new List<string>();
        if (Count == 0)
        {
            return items;
        }
        foreach (var itemDropChance in this)
        {
            if (itemDropChance.Chance == 1)
            {
                items.AddRange(Enumerable.Repeat(itemDropChance.Id, itemDropChance.Count));
            }
            else
            {
                for (int i = 0; i < itemDropChance.Count; ++i)
                {
                    if (RandomUtil.Rand.Next(itemDropChance.Chance) == 0)
                    {
                        items.Add(itemDropChance.Id);
                    }
                }
            }
        }
        return items;
    }
}
