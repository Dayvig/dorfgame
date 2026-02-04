using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Segment : MonoBehaviour {
    public Hex parentHex;
    public GameObject tint;
    public List<Building> plots;

    public void OnMouseEnter()
    {
        foreach (Segment s in parentHex.segments)
        {
            s.tint.gameObject.SetActive(false);
        }
        tint.SetActive(true);
        foreach (Building b in plots)
        {
            b.plot.SetActive(true);
            if (!b.isActive) {
                b.visual.color = new Color(1f, 1f, 1f, 0.4f);
            }
        }

        HexManager.instance.segmentHovered = this;
    }

    public void OnMouseUp()
    {
        if (HexManager.instance.segmentHovered == this)
        {
            foreach (Building b in plots)
            {
                b.isActive = true;
                b.visual.color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }

    public void OnMouseExit()
    {
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
