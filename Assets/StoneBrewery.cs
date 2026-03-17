using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static ModelGame;
using static UnityEditor.ObjectChangeEventStream;

public class StoneBrewery : SegmentBuilding
{
    public List<Vector2> menuOffsets = new List<Vector2>
    {
        new Vector2(0f, 150f),
        new Vector2(0f, -150f)
    };
    public float tickCtr = 0.0f;
    public float shakeCtr = 0.0f;
    float interval = 0.2f;
    public float brewTime = 20f;
    public float brewValue = 0;
    public float alchoholContentInc = 2.5f;
    int brewingTaskTally = 0;

    public UnityEngine.UI.Slider rockSlider;
    Vector2 shake = new Vector2(0f, 0.05f);

    public float hopsRequired;
    public float stockedHops = 0f;

    /*private void Start()
    {
        buttons[0].onClick.AddListener(delegate { setSlotStatus(0); });
        rockSlider.onValueChanged.AddListener(delegate { changeRockdustStatus(rockSlider.value); });
        rockSlider.value = 3;
        changeRockdustStatus(3);
    }

    public override void setTask(Segment seg)
    {
        gatheredBuildingResources.Clear();
        DorfManager.instance.setConstructionSite(this);
        isBuilding = true;
        seg.occupied = true;
    }

    void setSlotStatus(int slot)
    {
        availableSlots[slot] = !availableSlots[slot];
        assignedDorves[0] = assignedDorves[0] == null ? DorfManager.instance.dorves[0] : null;
        buttons[slot].transform.GetChild(0).GetComponentInChildren<UnityEngine.UI.Image>().color = availableSlots[slot] ? Color.green : Color.black;
    }

    void changeRockdustStatus(float sliderValue)
    {
        /*switch (sliderValue)
        {
            case 0:
                rockdustConversionRate = 0.0f; break;
            case 1:
                rockdustConversionRate = 0.1f; break;
            case 2:
                rockdustConversionRate = 0.25f; break;
            case 3:
                rockdustConversionRate = 0.5f; break;
            case 4:
                rockdustConversionRate = 0.75f; break;
            case 5:
                rockdustConversionRate = 0.9f; break;
            case 6:
                rockdustConversionRate = 1.0f; break;
        }
}
public override void onPlace(Dorf builder)
    {
        base.onPlace(builder);
        ResourceManager.instance.activatableBuildings.Add(this);
        ResourceManager.instance.stockableBuildings.Add(this);

    }
    public override bool canBeActivated()
    {
        return stockedHops >= hopsRequired && !running && brewValue == 0f && parentHex.hasWater && !taskSet;
    }

    public override bool canStock()
    {
        return !running && stockedHops < hopsRequired;
    }

    public override void stock(Dorf d)
    {
        base.stock(d);
        int hopsInTransit = 0;
        foreach (WorldResource w in ResourcesInTransit)
        {
            if (w.type.Equals(ResourceManager.ResourceType.HOPS))
            {
                hopsInTransit += (int)w.value;
            }
        }
        if (stockedHops + hopsInTransit < hopsRequired)
        {
            pickupHops(d);
        }
    }

    public void pickupHops(Dorf assignee)
    {
        int hopsInTransit = 0;
        foreach (WorldResource w in ResourcesInTransit)
        {
            if (w.type.Equals(ResourceManager.ResourceType.HOPS))
            {
                hopsInTransit += (int)w.value;
            }
        }
        if (DorfManager.instance.resourceExists(ResourceManager.ResourceType.HOPS, hopsRequired - stockedHops - hopsInTransit) && assignee.currentHaul < assignee.carryingCapacity && hopsInTransit < hopsRequired)
        {
            WorldResource nxt = DorfManager.instance.closestResourceToPickup(assignee.transform.position, ResourceManager.ResourceType.HOPS, hopsRequired - stockedHops - hopsInTransit, false);
            DorfManager.instance.moveToAndPickupResource(assignee, nxt, ResourceManager.ResourceType.HOPS, hopsRequired - stockedHops, false, this, () =>
            {
                pickupHops(assignee);
            });
        }
        else
        {
            DorfManager.instance.moveToBuilding(assignee, this, () =>
            {
                List<WorldResource> toBrew = new List<WorldResource>();
                foreach (WorldResource w in assignee.heldResources)
                {
                    if (w.type.Equals(ResourceManager.ResourceType.HOPS))
                    {
                        toBrew.Add(w);
                    }
                }
                foreach (WorldResource w in toBrew)
                {
                    stockedHops += w.value;
                    DorfManager.instance.drop(assignee, w);
                    ResourceManager.instance.toBeDestroyed.Add(w);
                    ResourceManager.instance.consumeResource(ResourceManager.ResourceType.HOPS, (int)w.value, w.isClutter);
                }
            });
        }
    }

    public override void activate()
    {
        DorfManager.PersonalTask thisTask;

        thisTask = new DorfManager.PersonalTask(1f, DorfTask.BREW, transform.position, this);
        thisTask.setResult(thisTask, () =>
        {
            running = true;
            tickCtr = 0.0f;
            stockedHops = 0f;
        });
        DorfManager.instance.allCurrentTasks.Add(thisTask);
        taskSet = true;
    }

    public void setHarvestTask()
    {
        DorfManager.PersonalTask thisTask = new DorfManager.PersonalTask(3.0f, DorfTask.HARVEST,
        () => {
            while (brewValue > 0)
            {
                WorldResource wres = ResourceManager.instance.createNewWorldResource(parentHex, ResourceManager.ResourceType.BEER, this.transform.position, 0.2f, true);
                float targetValue = brewValue * wres.weight > 6 ? (6 / wres.weight) : brewValue;
                float targetWeight = targetValue * wres.weight;
                wres.value = targetValue;
                wres.weight = targetWeight;
                brewValue -= targetValue;
            }


            if (ResourceManager.instance.harvestableBuildings.Contains(this))
            {
                ResourceManager.instance.harvestableBuildings.Remove(this);
            }
            brewValue = 0f;
            tickCtr = 0f;
            taskSet = false;
        },
        visual.transform.position, parentSegment);
        DorfManager.instance.allCurrentTasks.Add(thisTask);
    }
    public void setTendingTask()
    {
        DorfManager.PersonalTask thisTask = new DorfManager.PersonalTask(2.0f, DorfTask.BREW,
        () => {
            brewValue *= 1.2f;
        },
        visual.transform.position, parentSegment);
        thisTask.setMaxDorves(thisTask, 1);
        DorfManager.instance.assignDorfToTask(assignedDorves[0], thisTask);
        DorfManager.instance.allCurrentTasks.Add(thisTask);
    }*/

    private void Update()
    {
        if (selected)
        {
            for (int i = 0; i < menuObjects.Count; i++)
            {
                menuObjects[i].SetActive(true);
                menuObjects[i].transform.position = Camera.main.WorldToScreenPoint(visual.transform.position) + (Vector3)menuOffsets[i];
            }
        }
        if (isActive)
        {
            if (running)
            {
                tickCtr += Time.deltaTime;
                shakeCtr += Time.deltaTime;
                brewValue += alchoholContentInc * Time.deltaTime;

                if (shakeCtr > interval)
                {
                    shake = new Vector2(shake.x, -shake.y);
                    transform.localPosition += (Vector3)shake;
                    shakeCtr -= interval;
                }

                if (tickCtr > (brewTime / 4) * brewingTaskTally)
                {
                    //create brewing task
                    brewingTaskTally++;
                }

                if (tickCtr > brewTime)
                {
                    running = false;
                    tickCtr = 0f;
                    ResourceManager.instance.harvestableBuildings.Add(this);
                    //setHarvestTask();
                }
            }
        }
    }
}
