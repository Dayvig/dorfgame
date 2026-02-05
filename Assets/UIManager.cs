using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static ModelGame;

public class UIManager : MonoBehaviour
{
    public float tickCtr = 0.0f;
    public float interval = 1.0f;

    public Building currentlySelectedBuilding = null;
    public DorfTask currentTask = DorfTask.NONE;

    public static UIManager instance
    {
        get; private set;
    }

    public TextMeshProUGUI foodCounter;
    public TextMeshProUGUI housingCounter;
    public TextMeshProUGUI rocksCounter;

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
        rocksCounter.text = "" + ResourceManager.instance.Rocks;
    }
}
