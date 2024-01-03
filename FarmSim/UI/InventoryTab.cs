using FarmSim.Entities;
using System.Linq;
using UI;

namespace FarmSim.UI;

class InventoryTab : Tab
{
    public Inventory Inventory;
    public Tags[] PredefinedFilters;
    public WrapContainer ItemContainer;

    public InventoryTab(Inventory inventory, Tags[] predefinedFilters, string text)
    {
        Inventory = inventory;
        PredefinedFilters = predefinedFilters;
        ReleasedTexture = "button-released";
        SelectedTexture = "button-selected";
        PressedTexture = "button-pressed";
        Width = "100";
        Height = "40";
        Children.Add(new Text
        {
            Value = $"<white>{text}",
            Weight = Text.FontWeight.Medium,
            VerticalAlignment = Alignment.Center,
            HorizontalAlignment = Alignment.Center,
        });
        ItemContainer = new WrapContainer
        {
            Padding = "10"
        };
        var scrollableContainer = new ScrollableContainer
        {
            Width = "100%",
            Height = "100%",
            Margin = "20",
            Texture = "scrollable-container-background",
            ScrollboxTexture = "scrollbox",
            ScrollbarBackgroundTexture = "scrollbar-background",
        };
        scrollableContainer.Children.Add(ItemContainer);
        TabContent = new[]
        {
            scrollableContainer,
        };
    }

    public void RefreshInventory()
    {
        ItemContainer.Resize();
        ItemContainer.Children.Clear();
        foreach (var (itemId, itemList) in Inventory.SimplifiedItems)
        {
            var itemCount = PredefinedFilters.Length == 0 ? itemList.Count : itemList.Count(i => i.Tags.Any(PredefinedFilters.Contains));
            if (itemCount > 0)
            {
                ItemContainer.Children.Add(new SimplifiedItemUIElement(itemId, itemCount, color: ItemManager.PickColor(itemList[0])));
            }
        }
    }
}
