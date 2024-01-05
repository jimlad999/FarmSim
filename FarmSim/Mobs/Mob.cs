using FarmSim.Entities;
using FarmSim.Rendering;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

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

    public void AddBehaviours(Player.Player player, params Behaviour[] playerOrders)
    {
        if (Owner == player)
        {
            Behaviours.RemoveAll(PlayerBehaviours.Contains);
            PlayerBehaviours.Clear();
            Behaviours.InsertRange(0, playerOrders);
            PlayerBehaviours.AddRange(playerOrders);
        }
    }

    public void RemoveBehaviours<TBehaviour>(Player.Player player)
        where TBehaviour : Behaviour
    {
        if (Owner == player)
        {
            var behavioursToRemove = PlayerBehaviours.OfType<TBehaviour>().ToList();
            Behaviours.RemoveAll(behavioursToRemove.Contains);
            PlayerBehaviours.RemoveAll(behavioursToRemove.Contains);
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

    // return true if newly tamed. false otherwise.
    public bool Feed(Player.Player player, ItemInfo food)
    {
        // TODO: formulae based on mob.HP (less HP = higher reduction), food.Quality (higher quality = higher reduction), MobData (Tags modifiers?)
        // If mob is reduced to 1 HP then give an easy tame since the monster would be unconscious soon anyway (or it was recently unconscious anyway)
        --Hunger;
        if (Hunger <= 0 || HP <= 1)
        {
            // Don't allow owners to change simply by feed. Only untamed mobs can be tamed this way
            // Setting of owner will happen in MobManager.Tame
            return Owner == null;
        }
        return false;
    }

    public void Update(GameTime gameTime)
    {
        if (Hit || HP <= 0)
        {
            var newX = X;
            var newY = Y;
            if (UpdateForces(gameTime, x: ref newX, y: ref newY))
            {
                var newXInt = (int)newX;
                var newYInt = (int)newY;
                if (GlobalState.TerrainManager.ValidateMovement(oldX: XInt, oldY: YInt, newX: newXInt, newY: newYInt))
                {
                    X = newX;
                    Y = newY;
                    XInt = newXInt;
                    YInt = newYInt;
                    this.UpdateTileIndex();
                }
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
        UpdateForces(gameTime, x: ref newX, y: ref newY);
        var newXInt = (int)newX;
        var newYInt = (int)newY;
        if (GlobalState.TerrainManager.ValidateMovement(oldX: XInt, oldY: YInt, newX: newXInt, newY: newYInt))
        {
            X = newX;
            Y = newY;
            XInt = newXInt;
            YInt = newYInt;
            this.UpdateTileIndex();
            UpdateFacingDirection(directionX: normalizedDirection.X, directionY: normalizedDirection.Y);
            return (xDirectionPositive ? XInt < targetX : XInt > targetX)
                && (yDirectionPositive ? YInt < targetY : YInt > targetY);
        }
        else
        {
            return false;
        }
    }

    public IEnumerable<string> GetDrops()
    {
        return Metadata.Drops.PickItems();
    }
}
