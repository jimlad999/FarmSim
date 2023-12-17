using System.Collections;
using System.Collections.Generic;

namespace FarmSim.Terrain;

class Buildings : IEnumerable<string>
{
    private string _building;
    private readonly List<string> _stations = new();

    public void Add(BuildingType buildingType, string buildingKey)
    {
        if (buildingType == BuildingType.Building)
        {
            _building = buildingKey;
        }
        else if (buildingType == BuildingType.Station)
        {
            _stations.Add(buildingKey);
        }
    }

    public bool Any()
    {
        return _building != null || _stations.Count > 0;
    }

    public IEnumerator<string> GetEnumerator()
    {
        if (_building != null)
        {
            var allBuildings = new List<string>(_stations.Count + 1) { _building };
            allBuildings.AddRange(_stations);
            return allBuildings.GetEnumerator();
        }
        else
        {
            return _stations.GetEnumerator();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
