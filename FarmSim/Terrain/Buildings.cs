using System.Collections;
using System.Collections.Generic;

namespace FarmSim.Terrain;

class Buildings : IEnumerable<string>
{
    private readonly List<string> _buildings = new();

    public bool HasFloor { get; private set; }

    public void Add(string building)
    {
        if (!HasFloor)
        {
            _buildings.Add(building);
            // TODO: currently only placing floors
            HasFloor = true;
        }
    }

    public bool Any()
    {
        return _buildings.Count > 0;
    }

    public IEnumerator<string> GetEnumerator()
    {
        return ((IEnumerable<string>)_buildings).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_buildings).GetEnumerator();
    }
}
