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
    public bool canPerform = true;

    void Awake()
    {
        taskSelector.onClick.AddListener(TaskOnClick);
    }

    private void Update()
    {
    }

    void TaskOnClick()
    {
        if (canPerform)
        {
            UIManager.instance.currentTask = thisTask;
            HexManager.instance.currentSelectionMode = mode;
            HexManager.instance.changeMode(mode);
        }
    }
}
