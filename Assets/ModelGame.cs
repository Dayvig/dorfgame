using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelGame : MonoBehaviour
{
    public static ModelGame instance
    {
        get; private set;
    }

    public enum DorfTask
    {
        MINE,
        NONE,
        BUILD,
        WORKBUILDING,
        HARVEST,
        HAUL,
        EAT
    }

    private void Start()
    {
        instance = this;
    }

    private void Update()
    {

    }
}
