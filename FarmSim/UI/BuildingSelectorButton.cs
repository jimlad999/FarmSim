using FarmSim.Rendering;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UI;

namespace FarmSim.UI;

[DataContract]
class BuildingSelectorButton : Button
{
    private static string HalfTileSizeToScale = (Renderer.TileSizeHalf / 2).ToString();
    private static string TileTop = (Renderer.TileSizeHalf / 4).ToString();

    [IgnoreDataMember]
    public string BuildingKey;
    [IgnoreDataMember]
    public Building Metadata;

    public BuildingSelectorButton(string buildingKey, Building metadata)
    {
        BuildingKey = buildingKey;
        Metadata = metadata;
        ReleasedTexture = PressedTexture = Texture = "building-panel";
        SelectedTexture = "building-panel-selected";
        var children = new List<UIElement>();
        var hasExteriorWall = metadata.ExteriorWall != null;
        var hasInteriorWall = metadata.InteriorWall != null;
        if (!hasExteriorWall && !hasInteriorWall && metadata.Floor != null)
        {
            children.Add(
                new TileUIElement(
                    roof: null,
                    exteriorWall: null,
                    interiorWall: null,
                    floor: metadata.Floor,
                    hasTransparency: metadata.HasTransparency)
                {
                    VerticalAlignment = Alignment.Center,
                    HorizontalAlignment = Alignment.Center,
                    Bottom = "10"
                });
        }
        else
        {
            if (hasExteriorWall)
            {
                children.Add(
                    new TileUIElement(
                        roof: metadata.Roof,
                        exteriorWall: metadata.ExteriorWall,
                        interiorWall: metadata.InteriorWall,
                        floor: metadata.Floor,
                        hasTransparency: metadata.HasTransparency)
                    {
                        VerticalAlignment = Alignment.Center,
                        HorizontalAlignment = Alignment.Center,
                        Top = TileTop,
                        Right = hasInteriorWall ? HalfTileSizeToScale : null,
                    });
            }
            if (hasInteriorWall)
            {
                children.Add(
                    new TileUIElement(
                        roof: null,
                        exteriorWall: null,
                        interiorWall: metadata.InteriorWall,
                        floor: metadata.Floor,
                        hasTransparency: metadata.HasTransparency)
                    {
                        VerticalAlignment = Alignment.Center,
                        HorizontalAlignment = Alignment.Center,
                        Top = TileTop,
                        Left = hasExteriorWall ? HalfTileSizeToScale : null,
                    });
            }
        }
        Children = children;
    }
}