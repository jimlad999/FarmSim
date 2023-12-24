using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UI;
using Utils;

namespace FarmSim.UI;

[DataContract]
class BuildingSelector : UIElement
{
    [DataMember]
    public string ItemContainerId;

    [IgnoreDataMember]
    private UIElement ItemContainer;
    [IgnoreDataMember]
    public ButtonEventHandler EventHandler;

    public override void Update(GameTime gameTime, UIState state, UISpriteSheet uiSpriteSheet, ControllerManager controllerManager)
    {
        if (ItemContainer == null && TryGetById(ItemContainerId, out UIElement itemContainer))
        {
            ItemContainer = itemContainer;
            var buildings = new List<UIElement>(GlobalState.BuildingData.Buildings.Count);
            foreach (var buildingData in GlobalState.BuildingData.Buildings)
            {
                var buildingSelectorButton = new BuildingSelectorButton(buildingData.Key, buildingData.Value);
                buildingSelectorButton.EventHandler += EventHandler;
                buildings.Add(buildingSelectorButton);
            }
            ItemContainer.Children = buildings;
        }

        base.Update(gameTime, state, uiSpriteSheet, controllerManager);
    }
}
