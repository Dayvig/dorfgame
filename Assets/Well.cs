using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Well : SegmentBuilding
{
    public bool showingWaterTiles = false;
    public override void setTask(Segment seg)
    {
        gatheredBuildingResources.Clear();
        DorfManager.instance.setConstructionSite(this);
        isBuilding = true;
        seg.occupied = true;
    }

    public override void onPlace(Dorf builder)
    {
        base.onPlace();
        setWaterTiles(this.parentHex, 2);
    }

    public override void onHover()
    {
        base.onHover();
        if (!showingWaterTiles)
        {
            showAllWaterTiles();
            showingWaterTiles = true;
        }
    }
    public override void onUnHover()
    {
        base.onUnHover();
        if (showingWaterTiles)
        {
            hideWaterTiles();
            showingWaterTiles = false;
        }
    }

    public override bool canBePlaced(Segment targetSegment)
    {
        return targetSegment.parentHex.hasOriginalSourceWater;
    }
    public void setWaterTiles(Hex center, int depth)
    {
        int currentDepth = 0;
        List<Hex> hexList = new List<Hex>();
        List<Hex> toAdd = new List<Hex>();

        hexList.Add(center);
        while (currentDepth < depth)
        {
            foreach (Hex h in hexList)
            {
                Debug.Log("Target:" + h.name);
                foreach (Hex neighbor in h.neighbors)
                {
                    Debug.Log(neighbor == null);

                    if (neighbor != null)
                    {
                        Debug.Log("Neighbor: " + neighbor.name);
                        if (!hexList.Contains(neighbor))
                        {
                            Debug.Log("Adding neighbor:" + neighbor.name);
                            toAdd.Add(neighbor);
                        }
                    }
                }
            }
            hexList.AddRange(toAdd);
            toAdd.Clear();
            currentDepth++;
        }
        foreach (Hex water in hexList)
        {
            water.hasWater = true;
        }
    }

    public void showAllWaterTiles()
    {
        foreach (Hex hex in HexManager.instance.hexes)
        {
            hex.waterVisual.color = hex.hasOriginalSourceWater ? new Color(1f, 1f, 1f, 0.8f) : new Color(1f, 1f, 1f, 0.4f);
            hex.waterVisual.gameObject.SetActive(hex.hasWater);
        }
    }

    public void hideWaterTiles()
    {
        foreach (Hex hex in HexManager.instance.hexes)
        {
            hex.waterVisual.gameObject.SetActive(false);
        }
    }
}
