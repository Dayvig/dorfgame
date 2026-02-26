using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldResource : MonoBehaviour
{
    public ResourceManager.ResourceType type;
    public float weight;
    public float value;
    public Sprite icon;
    public Hex thisHex;
    public bool toBePickedUp = false;
    public bool stowed = false;
    public bool isClutter = false;

    public WorldResource (ResourceManager.ResourceType type, float weight, float value)
    {
        this.type = type;
        this.weight = weight;
        this.value = value;
    }

    public void setHex(Hex h)
    {
        thisHex = h;
    }
}
