using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance
    {
        get; private set;
    }

    public int Food = 0;
    public int Housing = 0;
    public int Rocks = 0;
    public int RockDust = 0;
    public int Manure = 0;
    public int Beer = 0;
    public int Hops = 0;
    public int Clay = 0;
    public int Iron = 0;

    public int FoodClutter = 0;
    public int RockClutter = 0;
    public int RockDustClutter = 0;
    public int ManureClutter = 0;
    public int BeerClutter = 0;
    public int HopsClutter = 0;
    public int ClayClutter = 0;
    public int IronClutter = 0;

    public List<Resource> resourceRefs = new List<Resource>();
    public List<WorldResource> toBeDestroyed = new List<WorldResource>();

    public List<Building> harvestableBuildings = new List<Building>();
    public List<Building> storageBuildings = new List<Building>();
    public List<Building> housing = new List<Building>();
    public List<Building> activatableBuildings = new List<Building>();
    public List<Building> stockableBuildings = new List<Building>();

    private void Awake()
    {
        instance = this;
    }
    public enum ResourceType
    {
        FOOD,
        ROCKS,
        ROCKDUST,
        MANURE,
        BEER,
        HOPS,
        CLAY,
        IRON
    }

    private void Update()
    {
        foreach (WorldResource res in toBeDestroyed)
        {
            if (DorfManager.instance.clutter.Contains(res))
            {
                DorfManager.instance.clutter.Remove(res);
            }
        }
        for (int i = 0; i < toBeDestroyed.Count; i++)
        {
            toBeDestroyed[i].gameObject.SetActive(false);
        }
        toBeDestroyed.Clear();
    }

    public ref int getValidResourceCounter(ResourceType type, bool isClutter)
    {
        switch (type)
        {
            case ResourceType.FOOD:
                if (isClutter) { return ref FoodClutter; } else { return ref Food; }
            case ResourceType.ROCKS:
                if (isClutter) { return ref RockClutter; } else { return ref Rocks; }
            case ResourceType.ROCKDUST:
                if (isClutter){return ref RockDustClutter; } else {return ref RockDust;}
            case ResourceType.MANURE:
                if (isClutter) { return ref ManureClutter; } else { return ref Manure; }
            case ResourceType.BEER:
                if (isClutter) { return ref BeerClutter; } else { return ref Beer; }
            case ResourceType.HOPS:
                if (isClutter) { return ref HopsClutter; } else { return ref Hops; }
            case ResourceType.CLAY:
                if (isClutter) { return ref ClayClutter; } else { return ref Clay; }
            case ResourceType.IRON:
                if (isClutter) { return ref IronClutter; } else { return ref Iron; }
        }
        Debug.Log("Attempted to get a resource which doesn't exist");
        return ref Food;
    }

    public void addResource(ResourceType type, int amount, bool isClutter)
    {
        getValidResourceCounter(type, false) += amount;
        getValidResourceCounter(type, true) += isClutter ? amount : 0;
    }

    public void stowResource(ResourceType type, int amount, Building.StorageSlot slot)
    {
        getValidResourceCounter(type, true) -= amount;
        slot.occupiedStorage += amount;
    }

    public void stowResource(ResourceType type, int amount)
    {
        getValidResourceCounter(type, true) -= amount;
    }

    public void consumeResource(ResourceManager.ResourceType type, int amount, bool isClutter)
    {
        getValidResourceCounter(type, false) -= amount;
        getValidResourceCounter(type, true) -= isClutter ? amount : 0;
    }

    public WorldResource createNewWorldResource(Hex targetHex, ResourceManager.ResourceType resource, Vector2 center, float range, bool isClutter)
    {
        //Debug.Log("Attempting Spawn of : " + resource);
        Resource target = null;
        foreach (Resource r in ResourceManager.instance.resourceRefs)
        {
            //Debug.Log(resource + " = " + r.type + " ?");
            if (r.type.Equals(resource))
            {
                //Debug.Log("Spawning " + resource);
                target = r;
                break;
            }
        }
        if (target == null) { return null; }
        GameObject newRes = Instantiate(target.obj, ((Vector3)center + (Vector3)(UnityEngine.Random.insideUnitCircle * range)), Quaternion.identity);
        WorldResource wRes = newRes.GetComponent<WorldResource>();
        wRes.setHex(targetHex);
        wRes.isClutter = isClutter;
        if (isClutter)
        {
            DorfManager.instance.clutter.Add(wRes);
        }
        return wRes;
    }

    public Resource getResource(ResourceType type)
    {
        foreach (Resource r in ResourceManager.instance.resourceRefs)
        {
            if (r.type == type)
            {
                return r;
            }
        }
        return null;
    }

    [Serializable]
    public class Resource {

        public GameObject obj;
        public ResourceManager.ResourceType type;
    }
}
