using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

}
