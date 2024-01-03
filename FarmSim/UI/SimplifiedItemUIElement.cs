using Microsoft.Xna.Framework;
using System;
using UI;

namespace FarmSim.UI;

class SimplifiedItemUIElement : Panel
{
    public SimplifiedItemUIElement(string itemId, int itemCount, Color color)
    {
        Texture = "item-panel";
        var itemData = GlobalState.ItemData[itemId];
        var entityData = GlobalState.EntitiesData.Data[itemData.EntitySpriteKey];
        Children.Add(new EntityUIElement(itemData.EntitySpriteKey, entityData, color)
        {
            VerticalAlignment = Alignment.Center,
            HorizontalAlignment = Alignment.Center,
            Bottom = "10",
        });
        Children.Add(new Text
        {
            Value = $"<black>{Format(itemCount)}",
            Weight = Text.FontWeight.Small,
            HorizontalAlignment = Alignment.Center,
            Bottom = "2",
        });
    }

    private static string Format(int itemCount)
    {
        if (itemCount > 1_000_000)
        {
            return $"{Math.Round(itemCount / 1_000_000.0, 2)}m";
        }
        if (itemCount > 1_000)
        {
            return $"{Math.Round(itemCount / 1_000.0, 2)}k";
        }
        return itemCount.ToString();
    }
}
