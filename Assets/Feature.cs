using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feature : MonoBehaviour
{
    public SpriteRenderer visual;
    public bool blockBuildingPlacement = false;
    bool active = false;
    public Hex parentHex;
    public List<Feature> toRemove = new List<Feature>();

    public virtual void activate()
    {
        active = true;
        parentHex.activeFeatures.Add(this);
    }

    public virtual void remove()
    {
        active = false;
        if (parentHex.activeFeatures.Contains(this)) {
            parentHex.activeFeatures.Remove(this);
        }
        ResourceManager.instance.Rocks += 20;
        UIManager.instance.updateCounterDisplay();
    }

    public virtual void onHover()
    {

    }

    public virtual void onUnHover()
    {

    }
    public virtual void onClick()
    {

    }
}
