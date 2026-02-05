using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Segment : MonoBehaviour {
    public Hex parentHex;
    public GameObject tint;
    public List<Building> plots;

    public bool occupied = false;

    public void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        foreach (Segment s in parentHex.segments)
        {
            s.tint.gameObject.SetActive(false);
        }
        tint.SetActive(true);
        foreach (Building b in plots)
        {
            if (UIManager.instance.currentlySelectedBuilding == null || occupied)
            {
                break;
            }
            if (b.name.Equals(UIManager.instance.currentlySelectedBuilding.name))
            {
                b.plot.SetActive(true);
                if (!b.isActive)
                {
                    b.visual.color = new Color(1f, 1f, 1f, 0.4f);
                }
            }
        }

        HexManager.instance.segmentHovered = this;
    }

    public void OnMouseUp()
    {
        if (EventSystem.current.IsPointerOverGameObject() || occupied)
        {
            return;
        }
        if (HexManager.instance.segmentHovered == this)
        {
            foreach (Building b in plots)
            {
                if (UIManager.instance.currentlySelectedBuilding == null)
                {
                    break;
                }
                if (b.name.Equals(UIManager.instance.currentlySelectedBuilding.name))
                {
                    b.isActive = true;
                    b.onPlace();
                    b.visual.color = new Color(1f, 1f, 1f, 1f);
                    occupied = true;
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
            if (!b.isActive)
            {
                b.plot.SetActive(false);
            }
        }
    }

}
