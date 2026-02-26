using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class River : Feature
{
    public SpriteRenderer taskHover;
    public bool waterTilesShown = false;

    public override void activate()
    {
        base.activate();
        parentHex.placementBlocked = true;
        parentHex.movementBlocked = true;
        visual.gameObject.SetActive(true);
    }

    public override void reactivate()
    {
        base.reactivate();
        parentHex.placementBlocked = true;
        parentHex.movementBlocked = true;
        setWaterTiles(parentHex, 3);
        visual.gameObject.SetActive(true);
    }

    public override void remove()
    {
        base.remove();
        parentHex.placementBlocked = false;
        parentHex.movementBlocked = false;
        foreach (Feature f in parentHex.activeFeatures)
        {
            f.reactivate();
        }
        visual.gameObject.SetActive(false);
    }

    public override void onHover()
    {
        base.onHover();
        if (!waterTilesShown)
        {
            showAllWaterTiles();
            waterTilesShown = true;
        }
    }

    public override void onUnHover()
    {
        base.onUnHover();
        if (waterTilesShown)
        {
            hideWaterTiles();
            waterTilesShown = false;
        }
    }

    public override void onClick()
    {
        
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
                foreach (Hex neighbor in h.neighbors)
                {
                    if (neighbor != null)
                    {
                        if (!hexList.Contains(neighbor))
                        {
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
            water.hasOriginalSourceWater = true;
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
