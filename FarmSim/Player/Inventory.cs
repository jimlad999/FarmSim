using FarmSim.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Player;

class Inventory
{
    public Dictionary<string, List<ItemInfo>> SimplifiedItems = new();

    public Inventory(List<ItemInfo> items)
    {
        SimplifiedItems = items
            .GroupBy(i => i.Id, (key, group) => (key, value: group.ToList()))
            .ToDictionary(a => a.key, a => a.value);
    }

    public List<ItemInfo> GetDataForSave()
    {
        return SimplifiedItems.Values.Aggregate(new List<ItemInfo>(), (agg, next) =>
        {
            agg.AddRange(next);
            return agg;
        });
    }

    public void AddItem(ItemInfo item)
    {
        item.PickedUpTimestampTicks = DateTime.UtcNow.Ticks;
        if (!SimplifiedItems.TryGetValue(item.Id, out var itemlist))
        {
            SimplifiedItems[item.Id] = itemlist = new();
        }
        itemlist.Add(item);
    }

    // you already have the exact item (e.g. the player chose it from the inventory list)
    public void RemoveItem(ItemInfo item)
    {
        SimplifiedItems[item.Id].Remove(item);
    }

    // you are trying to find an item (e.g. an AI controlled mob doing a fetch task)
    public bool TryGetItem(string id, Func<ItemInfo, bool> filter, out ItemInfo item)
    {
        var itemList = SimplifiedItems[id];
        item = itemList.FirstOrDefault(filter);
        if (item == null)
        {
            return false;
        }
        itemList.Remove(item);
        return true;
    }
}
