using Microsoft.Xna.Framework.Graphics;

namespace FarmSim.Utils;

class EntitySpriteSheet : SpriteSheet<EntityData>
{
    public EntitySpriteSheet(SpriteBatch spriteBatch, EntitiesData tilesetData)
    {
        ProcessData(spriteBatch, tilesetData);
    }
}
