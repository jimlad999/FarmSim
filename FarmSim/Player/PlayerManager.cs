using FarmSim.Entities;
using FarmSim.Mobs;
using FarmSim.Projectiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Player;

class PlayerManager : EntityManager<Player>
{
    public const int DespawnRadius = MobManager.SpawnRadius + 10;//tiles

    public Player ActivePlayer;

    public void AddPlayer(Player player)
    {
        Entities.Add(player);
    }

    public void Reset()
    {
        foreach (var player in Entities)
        {
            player.InitDefaultAnimation();
        }
    }

    public bool OutsideDespawnRadius(Entity entity)
    {
        return Entities.All(player =>
            Math.Abs(entity.TileX - player.TileX) > DespawnRadius
                || Math.Abs(entity.TileY - player.TileY) > DespawnRadius
            );
    }

    public bool TryPickUpItem(Item item)
    {
        foreach (var player in Entities)
        {
            var itemXDiff = player.XInt - item.XInt;
            var itemYDiff = player.YInt - item.YInt;
            var itemDistancePow2 = itemXDiff * itemXDiff + itemYDiff * itemYDiff;
            if (itemDistancePow2 <= player.PickUpDistancePow2)
            {
                player.PickUpItem(item);
                return true;
            }
        }
        return false;
    }

    public bool DetectCollision(Projectile projectile)
    {
        foreach (var player in Entities.Where(projectile.DetectCollision))
        {
            GlobalState.AnimationManager.PlayOnce(player, "hit");
            // TODO: damage
            Damage(player, 0);
            if (projectile.Effect != null)// && player.HP > 0)
            {
                projectile.Effect.Apply(player, projectile.NormalizedDirection);
                if (projectile.Effect.AnimationKey != null)
                {
                    GlobalState.AnimationManager.Generate(entity: player, animationKey: projectile.Effect.AnimationKey, durationMilliseconds: projectile.Effect.DurationMilliseconds, playOnceOnly: false, direction: Vector2.UnitY);
                }
            }
            return true;
        }
        return false;
    }

    public void Damage(IEnumerable<Player> players, int damage)
    {
        foreach (var player in players)
        {
            Damage(player, damage);
        }
    }

    private static void Damage(Player player, int damage)
    {
        //player.HP -= 0;
        //if (player.HP <= 0)
        //{
        //    // TODO: respawn player
        //}
    }

    public void Update(GameTime gameTime)
    {
        foreach (var player in Entities)
        {
            player.Update(gameTime);
        }
    }
}
