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
    void Awake()
    {
        buildingSelector.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        UIManager.instance.currentlySelectedBuilding = thisBuilding;
        HexManager.instance.currentSelectionMode = mode;
        HexManager.instance.onModeChange(mode);
    }
}
