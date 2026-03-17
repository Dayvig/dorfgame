using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ModelGame;

public class Stone : Feature
{
    public SpriteRenderer taskHover;

    public override void activate()
    {
        base.activate();
        parentHex.placementBlocked = true;
        parentHex.movementBlocked = true;
        visual.gameObject.SetActive(true);
    }

    public override void remove()
    {
        base.remove();
        parentHex.placementBlocked = false;
        parentHex.movementBlocked = false;
        foreach (Feature f in parentHex.activeFeatures)
        {
            f.reactivate();
        }
        visual.gameObject.SetActive(false);
    }

    public override void onHover()
    {
        base.onHover();
        if (UIManager.instance.currentTask.Equals(DorfTask.MINE))
        {
            taskHover.gameObject.SetActive(true);
        }
    }

    public override void onUnHover()
    {
        base.onUnHover();
        taskHover.gameObject.SetActive(false);
    }

    public override void onClick()
    {
        base.onClick();
        bool canClear = false;
        foreach (Hex h in parentHex.neighbors)
        {
           if (h == null) { continue; }
            if (h.activeFeatures.Count == 0)
            {
                canClear = true;
                break;
            }
            else
            {
                foreach (Feature f in h.activeFeatures)
                {
                    if (!(f.type.Equals(featureType.STONE)))
                    {
                        canClear = true;
                        break;
                    }
                }
            }
        }
        if (!canClear) {  return; }
        if (UIManager.instance.currentTask.Equals(DorfTask.MINE))
        {
            taskHover.gameObject.SetActive(false);
            createNewGlobalMiningTask();
        }
    }

    public void createNewGlobalMiningTask()
    {
        DorfManager.GlobalTask newMiningTask = new DorfManager.GlobalTask(DorfTask.MINE, 8);

        DorfManager.PersonalTask Stage1Task = new DorfManager.PersonalTask(0f, DorfTask.HAUL, () => { }, this.parentHex.miningPoints(), this.parentHex);

        DorfManager.GlobalTask.Stage Stage1 = new DorfManager.GlobalTask.Stage(newMiningTask, DorfTask.MINE, 10f, this.parentHex.transform.position,
            () => { },
            Stage1Task);

        Stage1.setMultiLocation(this.parentHex.miningPoints());
        Stage1.setTarget(this.parentHex);
        Stage1.setTaskBar();
        Stage1.setTaskBarVisible(true);

        Stage1Task.parentTask = Stage1;

        newMiningTask.stages.Add(Stage1);
        newMiningTask.completionMethod = () =>
        {
            parentHex.toRemove.Add(this);
            taskHover.gameObject.SetActive(false);
            ResourceManager.instance.addResource(ResourceManager.ResourceType.ROCKS, 50, true);
            UIManager.instance.updateCounterDisplay();
            for (int i = 0; i < 10; i++)
            {
                ResourceManager.instance.createNewWorldResource(parentHex, ResourceManager.ResourceType.ROCKS, this.gameObject.transform.position, 1.0f, true);
            }
        };
        

        DorfManager.instance.taskQueue.Add(newMiningTask);
    }
}
