using System.Collections.Generic;

namespace FarmSim.Entities;

abstract class Storage : Entity
{
    public List<Tags> CanHold { get; init; } = new();
    public Entity Entity { get; set; }
}
