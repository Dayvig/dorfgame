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

    public int FoodClutter = 0;
    public int RockClutter = 0;
    public int RockDustClutter = 0;

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
        ROCKDUST
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

    public void addResource(ResourceType type, int amount, bool isClutter)
    {
        switch (type)
        {
            case ResourceType.FOOD:
                Food += amount;
                FoodClutter += isClutter ? amount : 0;
                break;
            case ResourceType.ROCKS:
                Rocks += amount;
                RockClutter += isClutter ? amount : 0;
                break;
            case ResourceType.ROCKDUST:
                RockDust += amount;
                RockDustClutter += isClutter ? amount : 0;
                break;
        }
    }

    public void stowResource(ResourceType res, int amount)
    {
        switch (res)
        {
            case ResourceType.FOOD:
                FoodClutter -= amount;
                break;
            case ResourceType.ROCKS:
                RockClutter -= amount;
                break;
            case ResourceType.ROCKDUST:
                RockDustClutter -= amount;
                break;
        }
    }

    public void consumeResource(ResourceManager.ResourceType res, int amount, bool isClutter)
    {
        switch (res)
        {
            case ResourceManager.ResourceType.FOOD:
                Food -= amount;
                FoodClutter -= isClutter ? amount : 0;
                break;
            case ResourceManager.ResourceType.ROCKS:
                Rocks -= amount;
                RockClutter -= isClutter ? amount : 0;
                break;
            case ResourceManager.ResourceType.ROCKDUST:
                RockDust -= amount;
                RockDustClutter -= isClutter ? amount : 0;
                break;
        }
    }

    public WorldResource createNewWorldResource(Hex targetHex, ResourceManager.ResourceType resource, Vector2 center, float range)
    {
        Resource target = null;
        foreach (Resource r in ResourceManager.instance.resourceRefs)
        {
            if (r.name == resource)
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

    [Serializable]
    public class Resource {

        public GameObject obj;
        public ResourceManager.ResourceType name;
    }
}
