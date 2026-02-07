using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ModelGame;

public class BasicHouse : SegmentBuilding
{
    public override void onPlace()
    {
        base.onPlace();
        ResourceManager.instance.Housing++;
        UIManager.instance.updateCounterDisplay();
    }

    public override void setTask(Segment seg)
    {
        DorfManager.DorfTaskInProgress thisTask = new DorfManager.DorfTaskInProgress(2.0f, DorfTask.BUILD,
        () => {
            isActive = true;
            onPlace();
            seg.parentHex.activeBuildings.Add(this);
            visual.color = new Color(1f, 1f, 1f, 1f);
            visual.gameObject.SetActive(true);
            isBuilding = false;
        },
        gameObject.transform.position, seg);
        DorfManager.instance.allCurrentTasks.Add(thisTask);
        isBuilding = true;
        seg.occupied = true;
    }
}
