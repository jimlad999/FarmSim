using FarmSim.Entities;
using FarmSim.Rendering;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FarmSim.Mobs;

abstract class Mob : Entity, IDespawnble, IHasInventory, IHasHunger
{
    public const int PickUpDistancePow2Const = Renderer.TileSize * Renderer.TileSize;
    public int PickUpDistancePow2 => PickUpDistancePow2Const;
    public Inventory Inventory { get; set; }
    public MobData Metadata;
    public Tags[] Tags;
    public int HP;
    public int Hunger;
    public bool FlagForDespawning { get; set; } = false;

    public bool Hit;

    protected List<Behaviour> Behaviours;
    public Player.Player Owner;
    public List<Behaviour> PlayerBehaviours = new();

    // called after construction
    public abstract void InitDefaultBehaviours();

    public void AddBehaviour(Player.Player player, Behaviour playerOrders)
    {
        if (Owner == player)
        {
            Behaviours.RemoveAll(PlayerBehaviours.Contains);
            PlayerBehaviours.Clear();
            Behaviours.Add(playerOrders);
            PlayerBehaviours.Add(playerOrders);
        }
    }

    public void RemoveBehaviour(Player.Player player, Behaviour playerOrders)
    {
        if (Owner == player)
        {
            Behaviours.Remove(playerOrders);
            PlayerBehaviours.Remove(playerOrders);
        }
    }

    public void PickUpItem(Item item)
    {
        // Don't eat things while unconscious (just in case some unconscious mobs are nearby when another mob dies and drops its loot)
        if (HP > 0 && Metadata.Eats.Contains(item.InstanceInfo.Tags))
        {
            Feed(item.Owner, item.InstanceInfo);
        }
        else
        {
            Inventory.AddItem(item.InstanceInfo);
        }
    }

    public void Feed(Player.Player player, ItemInfo food)
    {
        // TODO: formulae based on mob.HP (less HP = higher reduction), food.Quality (higher quality = higher reduction), MobData (Tags modifiers?)
        --Hunger;
        if (Hunger <= 0)
        {
            // Don't allow owners to change simply by feed. Only untamed mobs can be tamed this way/
            if (Owner == null)
            {
                Owner = player;
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        if (Hit || HP <= 0)
        {
            if (UpdateForces(gameTime))
            {
                XInt = (int)X;
                YInt = (int)Y;
                this.UpdateTileIndex();
            }
            return;
        }
        foreach (var behaviour in Behaviours)
        {
            if (behaviour.TryExecute(this, gameTime))
                break;
        }
    }

    public bool TryMove(GameTime gameTime, Vector2 normalizedDirection, int targetX, int targetY)
    {
        var movementPerFrame = gameTime.ElapsedGameTime.TotalSeconds * Metadata.Speed;
        var xDirectionPositive = normalizedDirection.X > 0;
        var yDirectionPositive = normalizedDirection.Y > 0;
        var newX = X + normalizedDirection.X * movementPerFrame;
        var newY = Y + normalizedDirection.Y * movementPerFrame;
        // TODO: detect collision or unpassable terrain and return false
        X = newX;
        Y = newY;
        UpdateForces(gameTime);
        XInt = (int)X;
        YInt = (int)Y;
        this.UpdateTileIndex();
        UpdateFacingDirection(directionX: normalizedDirection.X, directionY: normalizedDirection.Y);
        return (xDirectionPositive ? XInt < targetX : XInt > targetX)
            && (yDirectionPositive ? YInt < targetY : YInt > targetY);
    }

    public IEnumerable<string> GetDrops()
    {
        return Metadata.Drops.PickItems();
    }
}
