using FarmSim.Utils;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FarmSim.Entities;

class ItemManager : EntityManager<Item>
{
    public const double DespawnTimeSeconds = 120.0;//2 minutes
    private static readonly Color ItemBlue = new Color(21, 124, 221);
    private readonly Player.Player Player;
    private readonly Dictionary<string, ItemData> _itemData;

    public ItemManager(
        Player.Player player,
        Dictionary<string, ItemData> itemData)
    {
        Player = player;
        _itemData = itemData;
    }

    public void Update(GameTime gameTime)
    {
        foreach (var item in Entities)
        {
            if (!Player.TryPickUpItem(item))
            {
                // no point updating item if player has picked it up
                item.Update(gameTime);
                item.FlagForDespawning = item.SecondsInWorld >= DespawnTimeSeconds;
            }
            else
            {
                item.FlagForDespawning = true;
            }
        }
        Entities.RemoveAll(mob => mob.FlagForDespawning);
    }

    public void CreateNewItem(
        string itemId,
        int originX,
        int originY,
        Vector2 normalizedDirection)
    {
        var metadata = _itemData[itemId];
        var instanceInfo = CreateInstance(metadata);
        CreateItem(metadata, instanceInfo, originX, originY, normalizedDirection);
    }

    public void CreateItemFromExisting(
        ItemInfo instanceInfo,
        int originX,
        int originY,
        Vector2 normalizedDirection)
    {
        var metadata = _itemData[instanceInfo.Id];
        CreateItem(metadata, instanceInfo, originX, originY, normalizedDirection);
    }

    private void CreateItem(
        ItemData metadata,
        ItemInfo instanceInfo,
        int originX,
        int originY,
        Vector2 normalizedDirection)
    {
        var newItem = new Item();
        newItem.Metadata = metadata;
        newItem.InstanceInfo = instanceInfo;
        newItem.NormalizedDirection = normalizedDirection;
        newItem.Color = instanceInfo.Tags.Match(new()
        {
            { Tags.White, () => Color.White },
            { Tags.Black, () => Color.Black },
            { Tags.Red, () => Color.Red },
            { Tags.Green, () => Color.Green },
            { Tags.Blue, () => ItemBlue },
            { Tags.Yellow, () => Color.Yellow },
        }, defaultValue: Color.White);
        newItem.EntitySpriteKey = metadata.EntitySpriteKey;
        newItem.X = originX;
        newItem.XInt = originX;
        newItem.Y = originY;
        newItem.YInt = originY;
        newItem.UpdateTilePosition();
        Entities.Add(newItem);
    }

    private static ItemInfo CreateInstance(ItemData metadata)
    {
        var instance = new ItemInfo();
        instance.Id = metadata.Id;
        instance.Tags = metadata.Tags.PickTags();
        instance.Quality = metadata.Quality.GetValue();
        return instance;
    }
}
