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
        foreach (Segment s in parentHex.segments)
        {
            s.occupied = true;
        }
        visual.gameObject.SetActive(true);
    }

    public override void remove()
    {
        foreach (Segment s in parentHex.segments)
        {
            s.occupied = false;
        }
        visual.gameObject.SetActive(false);
        base.remove();
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
                    if (!(f.name.Equals("Stone")))
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
            DorfManager.DorfTaskInProgress thisTask = new DorfManager.DorfTaskInProgress(4.0f, DorfTask.MINE,
                () =>
                {
                    parentHex.toRemove.Add(this);
                    taskHover.gameObject.SetActive(false);
                    ResourceManager.instance.addResource(ResourceManager.ResourceType.ROCKS, 50);
                    UIManager.instance.updateCounterDisplay();
                    for (int i = 0; i < 10; i++)
                    {
                        ResourceManager.instance.createNewWorldResource(parentHex, ResourceManager.ResourceType.ROCKS, this.gameObject.transform.position, 1.0f);
                    }

                }
                , parentHex.miningPoints(), parentHex);
            DorfManager.instance.allCurrentTasks.Add(thisTask);
        }
    }
}
