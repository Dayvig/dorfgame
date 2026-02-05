using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicHouse : SegmentBuilding
{
    public override void onPlace()
    {
        ResourceManager.instance.Housing++;
        UIManager.instance.updateCounterDisplay();
    }
}
