using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance
    {
        get; private set;
    }

    public int Food = 0;

    private void Start()
    {
        instance = this;
    }

    private void Update()
    {

    }
}
