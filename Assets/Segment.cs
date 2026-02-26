using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static ModelGame;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Segment : MonoBehaviour {
    public Hex parentHex;
    public GameObject tint;
    public List<Building> plots;

    public bool occupied = false;

    public RectTransform progressBar;
    public Canvas taskbarCanvas;

    public GameObject plotObjectRoot;
    public void OnMouseEnter()
    {
        if (parentHex.activeBigBuildings.Count != 0 || UIManager.instance.currentTask.Equals(DorfTask.MINE) || parentHex.hasFeature(Feature.featureType.RIVER))
        {
            HexManager.instance.changeMode(HexManager.SelectionMode.HEX);
            return;
        }
        foreach (Segment s in parentHex.segments)
        {
            s.tint.gameObject.SetActive(false);
        }
        tint.SetActive(true);
        foreach (Building b in plots)
        {
            Debug.Log(b.name);
            Debug.Log(UIManager.instance.currentlySelectedBuilding.ID);
            Debug.Log(b.ID);

            if (UIManager.instance.currentlySelectedBuilding == null || occupied)
            {
                break;
            }
            if (UIManager.instance.currentlySelectedBuilding.ID.Equals(b.ID))
            {
                Debug.Log("Setting active" + b.plot.name + (b.plot.gameObject.activeSelf == true));
                b.plot.SetActive(true);
                if (!b.isActive)
                {
                    b.visual.color = new Color(1f, 1f, 1f, 0.4f);
                }
            }
        }

        foreach (Building b in parentHex.activeBuildings)
        {
            if (b is SegmentBuilding)
            {
                SegmentBuilding s = (SegmentBuilding)b;
                if (s.parentSegment == this)
                {
                    s.onHover();
                }
            }
            else
            {
                b.onHover();
            }
        }
        HexManager.instance.segmentHovered = this;
    }


public void OnMouseUp()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (HexManager.instance.segmentHovered == this)
        {
            foreach (Building b in plots)
            {
                if (UIManager.instance.currentlySelectedBuilding == null)
                {
                    if (b.isActive && b.selectable)
                    {
                        if (b.selected)
                        {
                            b.deselect();
                            continue;
                        }
                        if (UIManager.instance.currentActiveBuildingChangingProperties != null)
                        {
                            UIManager.instance.currentActiveBuildingChangingProperties.deselect();
                        }
                        UIManager.instance.currentActiveBuildingChangingProperties = b;
                        b.select();
                    }
                    continue;
                }
                if (b.name.Equals(UIManager.instance.currentlySelectedBuilding.name) && b.canBePlaced(this))
                {
                    b.parentHex = this.parentHex;
                    if (b is SegmentBuilding){
                        SegmentBuilding seg = (SegmentBuilding)b;
                        seg.parentSegment = this;
                    }
                    b.setTask(this);
                    b.onPlotPlaced();
                }
            }
        }
    }

    public void OnMouseExit()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        tint.SetActive(false);
        foreach (Building b in plots)
        {
            if (!b.isActive && !b.isBuilding)
            {
                b.plot.SetActive(false);
            }
        }
        foreach (Building b in parentHex.activeBuildings)
        {
            if (b is SegmentBuilding)
            {
                SegmentBuilding s = (SegmentBuilding)b;
                if (s.parentSegment == this)
                {
                    s.onUnHover();
                }
            }
            else
            {
                b.onUnHover();
            }
        }

    }

}
