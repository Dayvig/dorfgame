using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ModelGame;

public class BasicHouse : SegmentBuilding
{
    public override void onPlace()
    {
        base.onPlace();
        ResourceManager.instance.Housing++;
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
