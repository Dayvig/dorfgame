using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outhouse : SegmentBuilding
{
    public override void setTask(Segment seg)
    {
        gatheredBuildingResources.Clear();
        DorfManager.instance.setConstructionSite(this);
        isBuilding = true;
        seg.occupied = true;
    }

}
