using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static ModelGame;

public class TaskButton : MonoBehaviour
{
    public Button taskSelector;
    public DorfTask thisTask;
    public HexManager.SelectionMode mode;

    void Awake()
    {
        taskSelector.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        UIManager.instance.currentTask = thisTask;
        HexManager.instance.currentSelectionMode = mode;
        HexManager.instance.onModeChange(mode);
    }
}
