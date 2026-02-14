using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ModelGame;

public class BasicHouse : SegmentBuilding
{
    public List<Dorf> owners = new List<Dorf>();
    public int capacity = 1;
    public override void onPlace(Dorf builder)
    {
        base.onPlace(builder);
        ResourceManager.instance.Housing += capacity;
        if (builder.home == null && owners.Count < capacity)
        {
            owners.Add(builder);
            builder.home = this;
        }
        ResourceManager.instance.housing.Add(this);
        UIManager.instance.updateCounterDisplay();
    }

    public override void setTask(Segment seg)
    {
        gatheredBuildingResources.Clear();
        foreach (Building.BuildingCost c in costs)
        {
            gatheredBuildingResources.Add(new BuildingCost(0, c.type));
        }
        DorfManager.instance.setConstructionSite(this);
        isBuilding = true;
        seg.occupied = true;
    }
}
