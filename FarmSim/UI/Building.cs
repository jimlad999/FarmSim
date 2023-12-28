﻿using FarmSim.Player;
using FarmSim.Rendering;
using FarmSim.Terrain;
using FarmSim.Utils;
using System.Runtime.Serialization;

namespace FarmSim.UI;

[DataContract]
class Building
{
    [DataMember]
    public BuildingType Type;
    // tileset keys
    [DataMember]
    public string Roof;
    [DataMember]
    public string ExteriorWall;
    [DataMember]
    public string InteriorWall;
    [DataMember]
    public string Floor;
    // SPEED HACK to determine rendering/not rendering the backwall
    [DataMember]
    public bool HasTransparency;
    // What this building requires to be built on
    [DataMember]
    public Zoning[] Buildable;
    [DataMember]
    public Cost Cost;

    // will be setup on load
    [IgnoreDataMember]
    public Animation RoofAnimation;
    [IgnoreDataMember]
    public Animation ExteriorWallAnimation;
    [IgnoreDataMember]
    public Animation InteriorWallAnimation;
    [IgnoreDataMember]
    public Animation FloorAnimation;

    public void InitAnimations(TilesetData tilesetData)
    {
        if (Roof != null)
        {
            RoofAnimation = GenerateGlobalRepeatingAnimation(Roof, tilesetData);
        }
        if (ExteriorWall != null)
        {
            ExteriorWallAnimation = GenerateGlobalRepeatingAnimation(ExteriorWall, tilesetData);
        }
        if (InteriorWall != null)
        {
            InteriorWallAnimation = GenerateGlobalRepeatingAnimation(InteriorWall, tilesetData);
        }
        if (Floor != null)
        {
            FloorAnimation = GenerateGlobalRepeatingAnimation(Floor, tilesetData);
        }
    }

    private static Animation GenerateGlobalRepeatingAnimation(string tileDataKey, TilesetData tilesetData)
    {
        return GlobalState.AnimationManager.GenerateTilesetAnimation(tileDataKey, tilesetData.Data[tileDataKey]);
    }
}