using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ModelGame;

public class Apothecary : SegmentBuilding
{
    public override void setTask(Segment seg)
    {
        gatheredBuildingResources.Clear();
        foreach (Building.BuildingCost c in costs)
        {
            gatheredBuildingResources.Add(new BuildingCost(0, c.type));
        }
        //DorfManager.instance.setConstructionSite(this);
        isBuilding = true;
        seg.occupied = true;
    }

    public override bool canBePlaced(Segment targetSegment)
    {
        return !targetSegment.parentHex.hasWater;
    }
}
