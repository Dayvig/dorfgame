using System;
using System.Collections;
using System.Collections.Generic;
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

    public int FoodClutter = 0;
    public int RockClutter = 0;
    public int RockDustClutter = 0;
    public int ManureClutter = 0;

    public List<Resource> resourceRefs = new List<Resource>();
    public List<WorldResource> toBeDestroyed = new List<WorldResource>();

    public List<Building> harvestableBuildings = new List<Building>();
    public List<Building> storageBuildings = new List<Building>();
    public List<Building> housing = new List<Building>();

    private void Start()
    {
        instance = this;
    }

    public enum ResourceType
    {
        FOOD,
        ROCKS,
        ROCKDUST,
        MANURE
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
                if (isClutter) { return ref Food; } else { return ref FoodClutter; }
            case ResourceType.ROCKS:
                if (isClutter) { return ref Rocks; } else { return ref RockDustClutter; }
            case ResourceType.ROCKDUST:
                if (isClutter){return ref RockDust;} else {return ref RockDustClutter;}
            case ResourceType.MANURE:
                if (isClutter) { return ref Manure; } else { return ref ManureClutter; }
        }
        Debug.Log("Attempted to get a resource which doesn't exist");
        return ref Food;
    }

    public void addResource(ResourceType type, int amount, bool isClutter)
    {
        getValidResourceCounter(type, false) += amount;
        getValidResourceCounter(type, true) += isClutter ? amount : 0;
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

    public WorldResource createNewWorldResource(Hex targetHex, ResourceManager.ResourceType resource, Vector2 center, float range)
    {
        Resource target = null;
        foreach (Resource r in ResourceManager.instance.resourceRefs)
        {
            if (r.type.Equals(resource))
            {
                target = r;
                break;
            }
        }
        if (target == null) { return null; }
        GameObject newRes = Instantiate(target.obj, ((Vector3)center + (Vector3)(UnityEngine.Random.insideUnitCircle * range)), Quaternion.identity);
        WorldResource wRes = newRes.GetComponent<WorldResource>();
        wRes.setHex(targetHex);
        DorfManager.instance.clutter.Add(wRes);
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
