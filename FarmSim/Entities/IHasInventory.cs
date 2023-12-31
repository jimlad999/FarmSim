namespace FarmSim.Entities;

interface IHasInventory
{
    int PickUpDistancePow2 { get; }
    Inventory Inventory { get; }
    void PickUpItem(Item item);
}
