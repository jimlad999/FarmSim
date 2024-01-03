using FarmSim.Utils;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FarmSim.Entities;

class ItemManager : EntityManager<Item>
{
    public const double PickUpDelayTimeMilliseconds = 400;
    public const double DespawnTimeSeconds = 120.0;//2 minutes
    private static readonly Color ItemBlue = new Color(21, 124, 221);

    private readonly Dictionary<string, ItemData> _itemData;
    private readonly Dictionary<string, EntityData> _entityData;

    public ItemManager(
        Dictionary<string, ItemData> itemData,
        Dictionary<string, EntityData> entityData)
    {
        _itemData = itemData;
        _entityData = entityData;
    }

    public void Update(GameTime gameTime)
    {
        foreach (var item in Entities)
        {
            if (item.PickUpDelayTimeMilliseconds <= 0
                && (GlobalState.PlayerManager.TryPickUpItem(item) || GlobalState.MobManager.TryPickUpItem(item)))
            {
                item.FlagForDespawning = true;
                // TODO: should there be "collect" animation (e.g. pull the item towards the player)?
            }
            else
            {
                // no point updating item if player has picked it up
                item.Update(gameTime);
                if (item.SecondsInWorld >= DespawnTimeSeconds)
                {
                    item.FlagForDespawning = true;
                    GlobalState.AnimationManager.Generate(x: item.XInt, y: item.YInt, animationKey: "generic-despawn", scale: 0.5f);
                }
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
        newItem.Color = PickColor(instanceInfo);
        newItem.EntitySpriteKey = metadata.EntitySpriteKey;
        newItem.DefaultAnimationKey = _entityData[metadata.EntitySpriteKey].DefaultAnimationKey;
        newItem.PickUpDelayTimeMilliseconds = PickUpDelayTimeMilliseconds;
        newItem.X = originX;
        newItem.XInt = originX;
        newItem.Y = originY;
        newItem.YInt = originY;
        newItem.UpdateTileIndex();
        newItem.InitDefaultAnimation();
        Entities.Add(newItem);
    }

    public static Color PickColor(ItemInfo instanceInfo)
    {
        return instanceInfo.Tags.Match(new()
        {
            { Tags.White, () => Color.White },
            { Tags.Black, () => Color.Black },
            { Tags.Red, () => Color.Red },
            { Tags.Green, () => Color.Green },
            { Tags.Blue, () => ItemBlue },
            { Tags.Yellow, () => Color.Yellow },
        }, defaultValue: Color.White);
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
