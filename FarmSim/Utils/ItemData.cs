using FarmSim.Entities;
using System;

namespace FarmSim.Utils;

class ItemData
{
    public string Id;
    public string EntitySpriteKey;
    public RandomRange Quality;
    public TagSet[] Tags = Array.Empty<TagSet>();
}
