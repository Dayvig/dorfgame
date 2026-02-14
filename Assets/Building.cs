using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
    public GameObject plot;
    public SpriteRenderer visual;
    public bool isActive;
    public bool isBuilding = false;
    public List<BuildingCost> costs = new List<BuildingCost>();
    public List<BuildingCost> gatheredBuildingResources = new List<BuildingCost>();

    public bool selectable = false;
    public List<GameObject> menuObjects = new List<GameObject>();
    public bool selected = false;
    public Dorf[] assignedDorves = new Dorf[2];
    public bool[] availableSlots = new bool[2];
    public List<Button> buttons = new List<Button>();
    public Hex parentHex;
    public List<StorageSlot> storage = new List<StorageSlot>();

    public bool isBig = false;
    public float constructionTime;

    public enum BuildingTypes
    {
        HOUSE,
        STORAGE,
        MISC
    }

    public virtual void onPlace() {
        if (storage.Count > 0)
        {
            ResourceManager.instance.storageBuildings.Add(this);
        }
    }
    public virtual void onPlace(Dorf builder)
    {
        if (storage.Count > 0)
        {
            ResourceManager.instance.storageBuildings.Add(this);
        }
    }
    public virtual void select()
    {
        selected = true;
        UIManager.instance.currentActiveBuildingChangingProperties = this;
    }

    public virtual void deselect()
    {
        if (UIManager.instance.currentActiveBuildingChangingProperties.Equals(this))
        {
            UIManager.instance.currentActiveBuildingChangingProperties = null;
        }
        for (int i = 0; i < menuObjects.Count; i++)
        {
            menuObjects[i].SetActive(false);
        }
        selected = false;
    }

    public virtual void onPlotPlaced()
    {
        foreach (BuildingCost cost in costs)
        {
            cost.pay();
        }
        foreach (BuildingCost cost2 in UIManager.instance.currentlySelectedBuilding.costs)
        {
            if (!cost2.canPayCost())
            {
                UIManager.instance.currentlySelectedBuilding = null;
                break;
            }
        }
    }

    public virtual void onClicked() { }

    public virtual void setTask(Segment s) { }
    public virtual void setTask(Hex h) { }


    [Serializable]
    public class BuildingCost
    {
        public int numericalCost;
        public ResourceManager.ResourceType type;
        public bool markedAsComplete = false;

        public BuildingCost(int cost, ResourceManager.ResourceType type)
        {
            this.numericalCost = cost;
            this.type = type;
        }

        public void pay()
        {
            switch (type)
            {
                case ResourceManager.ResourceType.FOOD:
                    ResourceManager.instance.Food -= numericalCost; break;
                case ResourceManager.ResourceType.ROCKS:
                    ResourceManager.instance.Rocks -= numericalCost; break;
                case ResourceManager.ResourceType.ROCKDUST:
                    ResourceManager.instance.RockDust -= numericalCost; break;

            }
            UIManager.instance.updateCounterDisplay();
        }

        public bool canPayCost()
        {
            switch (type)
            {
                case ResourceManager.ResourceType.FOOD:
                    return ResourceManager.instance.Food >= numericalCost;
                case ResourceManager.ResourceType.ROCKS:
                    return ResourceManager.instance.Rocks >= numericalCost;
                case ResourceManager.ResourceType.ROCKDUST:
                    return ResourceManager.instance.RockDust >= numericalCost;

            }
            return false;
        }
    }

    [Serializable]
    public class StorageSlot
    {
        public float maxStorage;
        public float occupiedStorage;
        public ResourceManager.ResourceType type;

        public StorageSlot(int maxStorage, ResourceManager.ResourceType type)
        {
            this.maxStorage = maxStorage;
            this.type = type;
        }
    }
}
