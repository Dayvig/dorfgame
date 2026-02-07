using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    public Button buildingSelector;
    public Building thisBuilding;
    public HexManager.SelectionMode mode;
    public bool canPeform = true;
    ColorBlock disabledColors = new ColorBlock();

    void Awake()
    {
        buildingSelector.onClick.AddListener(TaskOnClick);
    }

    private void Update()
    {
        canPeform = true;
        foreach (Building.BuildingCost cost in thisBuilding.costs)
        {
            if (!cost.canPayCost())
            {
                canPeform = false;
                break;
            }
        }
        buildingSelector.interactable = canPeform;
    }

    void TaskOnClick()
    {
        if (canPeform)
        {
            UIManager.instance.currentlySelectedBuilding = thisBuilding;
            HexManager.instance.currentSelectionMode = mode;
            HexManager.instance.onModeChange(mode);
            UIManager.instance.updateCounterDisplay();
        }
    }
}
