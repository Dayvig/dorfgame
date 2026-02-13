using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static ModelGame;

public class Quarry : Building
{
    public List<Vector2> menuOffsets = new List<Vector2>
    {
        new Vector2(-50f, 150f),
        new Vector2(50f, 150f),
        new Vector2(0f, -150f)
    };
    public float tickCtr = 0.0f;
    public float interval = 1.0f;
    public float rockdustConversionRate = 0.5f;

    public UnityEngine.UI.Slider rockSlider;

    private void Start()
    {
        buttons[0].onClick.AddListener(delegate { setSlotStatus(0); });
        buttons[1].onClick.AddListener(delegate { setSlotStatus(1); });
        rockSlider.onValueChanged.AddListener(delegate { changeRockdustStatus(rockSlider.value); });
        rockSlider.value = 3;
        changeRockdustStatus(3);
    }

    public override void setTask(Hex hex)
    {
        gatheredBuildingResources.Clear();
        foreach (Building.BuildingCost c in costs)
        {
            gatheredBuildingResources.Add(new BuildingCost(0, c.type));
        }
        DorfManager.instance.setConstructionSite(this);
        isBuilding = true;
        foreach (Segment s in hex.segments)
        {
            s.occupied = true;
        }
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

    void changeRockdustStatus(float sliderValue)
    {
        switch (sliderValue)
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

    private void Update()
    {
        if (selected)
        {
            for (int i = 0; i < menuObjects.Count;i++)
            {
                menuObjects[i].SetActive(true);
                menuObjects[i].transform.position = Camera.main.WorldToScreenPoint(visual.transform.position) + (Vector3)menuOffsets[i];
            }
        }
        if (isActive)
        {
            if (tickCtr < 0.0f)
            {
                foreach (Dorf d in assignedDorves)
                {
                    if (d == null)
                    {
                        continue;
                    }
                    if (d.currentState.Equals(Dorf.DorfState.PERFORMINGTASK)) {
                        if (((float)ResourceManager.instance.RockDust / (float)(ResourceManager.instance.Rocks + ResourceManager.instance.RockDust)) < rockdustConversionRate)
                        {
                            ResourceManager.instance.addResource(ResourceManager.ResourceType.ROCKS, -1);
                            ResourceManager.instance.addResource(ResourceManager.ResourceType.ROCKDUST, 1);
                            storage[1].occupiedStorage += 1;
                            UIManager.instance.updateCounterDisplay();
                        }
                    }
                }
                tickCtr += interval;
            }
            else
            {
                tickCtr -= Time.deltaTime;
            }
        }

    }

}
