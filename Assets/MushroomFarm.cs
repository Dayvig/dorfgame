using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ModelGame;

public class MushroomFarm : SegmentBuilding
{
    public float tickCtr = 0.0f;
    public float interval = 1.0f;
    public List<Vector2> menuOffsets = new List<Vector2>
    {
        new Vector2(-50f, 150f),
        new Vector2(50f, 150f),
    };

    public float baseGrowthRate = 1.0f;
    public float finalGrowthRate = 1.0f;
    public int mushrooms = 0;
    public int fieldCapacity = 6;

    public GameObject[] shroomVisuals = new GameObject[6];
    private void Start()
    {
        buttons[0].onClick.AddListener(delegate { setSlotStatus(0); });
        buttons[1].onClick.AddListener(delegate { setSlotStatus(1); });
    }
    void setSlotStatus(int slot)
    {
        availableSlots[slot] = !availableSlots[slot];
        if (availableSlots[slot])
        {
            DorfManager.DorfTaskInProgress thisTask = new DorfManager.DorfTaskInProgress(DorfTask.WORKBUILDING, this.gameObject.transform.position, this, slot);
            thisTask.setMaxDorves(thisTask, 1);
            DorfManager.instance.allCurrentTasks.Add(thisTask);
        }
        else
        {
            assignedDorves[slot] = null;
            foreach (DorfManager.DorfTaskInProgress task in DorfManager.instance.allCurrentTasks)
            {
                if (task.targetBuilding != null && task.targetBuilding.Equals(this) && task.targetBuildingSlot == slot)
                {
                    task.complete();
                }
            }
        }

        buttons[slot].transform.GetChild(0).GetComponentInChildren<UnityEngine.UI.Image>().color = availableSlots[slot] ? Color.green : Color.black;
    }

    private void Update()
    {
        setGrowthRate();
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
            if (tickCtr > interval)
            {
                if (mushrooms < fieldCapacity)
                {
                    mushrooms++;
                    shroomVisuals[mushrooms-1].SetActive(true);
                    if (mushrooms >= fieldCapacity)
                    {
                        ResourceManager.instance.harvestableBuildings.Add(this);
                        setHarvestTask();
                    }
                }
                tickCtr -= interval;
            }
            else
            {
                tickCtr += Time.deltaTime * finalGrowthRate ;
            }
        }
    }

    public void setGrowthRate()
    {
        finalGrowthRate = baseGrowthRate;
        foreach (Dorf d in assignedDorves)
        {
            if (d != null)
            {
                finalGrowthRate += (baseGrowthRate / 5);
            }
        }
    }


    public override void setTask(Segment seg)
    {
        gatheredBuildingResources.Clear();
        foreach (Building.BuildingCost c in costs)
        {
            gatheredBuildingResources.Add(new BuildingCost(0, c.type));
        }
        DorfManager.instance.setConstructionSite(this);
        isBuilding = true;
        seg.occupied = true;
    }

    public void setHarvestTask()
    {
        DorfManager.DorfTaskInProgress thisTask = new DorfManager.DorfTaskInProgress(3.0f, DorfTask.HARVEST,
        () => {
            ResourceManager.instance.addResource(ResourceManager.ResourceType.FOOD, (int)ResourceManager.instance.getResource(ResourceManager.ResourceType.FOOD).obj.GetComponent<WorldResource>().value * mushrooms, true);
            for (int i = 0; i < mushrooms; i++)
            {
                ResourceManager.instance.createNewWorldResource(parentHex, ResourceManager.ResourceType.FOOD, this.gameObject.transform.position, 0.1f);
            }
            mushrooms = 0;
            foreach (GameObject shroom in shroomVisuals)
            {
                shroom.SetActive(false);
            }
            if (ResourceManager.instance.harvestableBuildings.Contains(this))
            {
                ResourceManager.instance.harvestableBuildings.Remove(this);
            }
        },
        visual.transform.position, this.parentSegment);
        DorfManager.instance.allCurrentTasks.Add(thisTask);
    }
}
