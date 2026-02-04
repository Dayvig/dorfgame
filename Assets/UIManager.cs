using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public float tickCtr = 0.0f;
    public float interval = 1.0f;

    public static UIManager instance
    {
        get; private set;
    }

    public TextMeshProUGUI foodCounter;

    private void Start()
    {
        instance = this;
    }

    private void Update()
    {
        if (tickCtr < 0.0f)
        {
            foodCounter.text = "" + ResourceManager.instance.Food;
            tickCtr += interval;
        }
        else
        {
            tickCtr -= Time.deltaTime;
        }
    }
}
