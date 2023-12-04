﻿namespace FarmSim.Utils;

class TileData
{
    public string Source { get; set; }
    // "origin" is from top left corner of the tile
    public TilesetOrigin Origin { get; set; }
    public BuildingType[] Buildable { get; set; } 
}
