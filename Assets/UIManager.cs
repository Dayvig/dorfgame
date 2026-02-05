using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public float tickCtr = 0.0f;
    public float interval = 1.0f;

    public Building currentlySelectedBuilding = null;

    public static UIManager instance
    {
        get; private set;
    }

    public TextMeshProUGUI foodCounter;
    public TextMeshProUGUI housingCounter;

    private void Start()
    {
        instance = this;
    }

    private void Update()
    {
        if (tickCtr < 0.0f)
        {
            updateCounterDisplay();
            tickCtr += interval;
        }
        else
        {
            tickCtr -= Time.deltaTime;
        }
    }

    public void updateCounterDisplay()
    {
        foodCounter.text = "" + ResourceManager.instance.Food;
        housingCounter.text = "0 / " + ResourceManager.instance.Housing;
    }
}
