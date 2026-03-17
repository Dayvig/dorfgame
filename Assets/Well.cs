using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Well : SegmentBuilding
{
    public bool showingWaterTiles = false;
    public override void setTask(Segment seg)
    {
        gatheredBuildingResources.Clear();
        //DorfManager.instance.setConstructionSite(this);
        isBuilding = true;
        seg.occupied = true;
    }

    public override void onPlace(Dorf builder)
    {
        base.onPlace();
        HexManager.instance.setWaterTiles(this.parentHex, 2);
    }

    public override void onHover()
    {
        base.onHover();
        if (!showingWaterTiles && !HexManager.instance.showingWaterTiles)
        {
            HexManager.instance.showAllWaterTiles();
            showingWaterTiles = true;
        }
    }
    public override void onUnHover()
    {
        base.onUnHover();
        if (showingWaterTiles && !HexManager.instance.showingWaterTiles)
        {
            HexManager.instance.hideWaterTiles();
            showingWaterTiles = false;
        }
    }

    public override bool canBePlaced(Segment targetSegment)
    {
        return targetSegment.parentHex.hasOriginalSourceWater;
    }
}
