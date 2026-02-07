using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public GameObject plot;
    public SpriteRenderer visual;
    public bool isActive;
    public bool isBuilding = false;
    public List<BuildingCost> costs = new List<BuildingCost>();

    public virtual void onPlace() {
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

    public virtual void setTask(Segment s) { }
    public virtual void setTask(Hex h) { }


    [Serializable]
    public class BuildingCost
    {
        public int numericalCost;
        public string type;

        public BuildingCost(int cost, string type)
        {
            this.numericalCost = cost;
            this.type = type;
        }

        public void pay()
        {
            switch (type)
            {
                case "Food":
                    ResourceManager.instance.Food -= numericalCost; break;
                case "Rocks":
                    ResourceManager.instance.Rocks -= numericalCost; break;
            }
            UIManager.instance.updateCounterDisplay();
        }

        public bool canPayCost()
        {
            switch (type)
            {
                case "Food":
                    return ResourceManager.instance.Food >= numericalCost; break;
                case "Rocks":
                    return ResourceManager.instance.Rocks >= numericalCost; break;
            }
            return false;
        }
    }
}
