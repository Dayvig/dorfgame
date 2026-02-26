using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuildingAdderTool))]
public class CustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BuildingAdderTool adderTool = (BuildingAdderTool)target;
        if (GUILayout.Button("Add Building"))
        {
            adderTool.addBuilding();
        }
        if (GUILayout.Button("Remove Building"))
        {
            adderTool.removeBuilding();
        }
    }
}
