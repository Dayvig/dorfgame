using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ModelGame;

public class MushroomFarm : SegmentBuilding
{
    public float tickCtr = 0.0f;
    public float interval = 1.0f;

    private void Update()
    {
        if (isActive)
        {
            if (tickCtr < 0.0f)
            {
                ResourceManager.instance.Food++;
                tickCtr += interval;
            }
            else
            {
                tickCtr -= Time.deltaTime;
            }
        }
    }

    public override void setTask(Segment seg)
    {
        DorfManager.DorfTaskInProgress thisTask = new DorfManager.DorfTaskInProgress(1.0f, DorfTask.BUILD,
        () => {
            isActive = true;
            onPlace();
            seg.parentHex.activeBuildings.Add(this);
            visual.color = new Color(1f, 1f, 1f, 1f);
            visual.gameObject.SetActive(true);
            seg.occupied = true;
            isBuilding = false;
        },
        gameObject.transform.position, seg);
        DorfManager.instance.assignTask(thisTask);
        isBuilding = true;
    }
}
