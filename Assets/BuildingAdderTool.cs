using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class BuildingAdderTool : MonoBehaviour
{
    public GameObject hexPrefab;
    public Hex prefabHex;
    public GameObject buildingObject;
    public Building building;
    public List<Vector2> offSets = new List<Vector2>();
    public bool bigBuilding = false;
    public void addBuilding()
    {
        Building b = building;


        if (bigBuilding)
        {
            Object GO = PrefabUtility.InstantiatePrefab(buildingObject);
            GameObject goObj = (GameObject)GO;
            Building goBuilding = goObj.GetComponent<Building>();

            prefabHex.bigBuildings.Add(goBuilding);
            goObj.transform.parent = prefabHex.hexObjectsRoot.transform;
            goObj.transform.localPosition = new Vector3(0f, 0f, 0f);
        }
        else
        {

            for (int i = 0; i < prefabHex.segments.Count; i++)
            {
                Object GO = PrefabUtility.InstantiatePrefab(buildingObject);
                GameObject goObj = (GameObject)GO;
                Building goBuilding = goObj.GetComponent<Building>();

                prefabHex.segments[i].plots.Add(goBuilding);

                goObj.transform.parent = prefabHex.segments[i].plotObjectRoot.transform;
                goObj.transform.localPosition = new Vector3(0f, 0f, 0f) + (Vector3)offSets[i];
                if (b is SegmentBuilding)
                {
                    SegmentBuilding segment = (SegmentBuilding)b;
                    segment.parentSegment = prefabHex.segments[i];
                }
            }
        }
        b.parentHex = prefabHex;

        PrefabUtility.ApplyPrefabInstance(hexPrefab, InteractionMode.UserAction);
    }
    public void removeBuilding()
    {
        Building b = building;
        if (bigBuilding)
        {
            for (int k = 0; k < prefabHex.bigBuildings.Count; k++)
            {
                if (prefabHex.bigBuildings[k].name == b.name)
                {
                    prefabHex.bigBuildings.RemoveAt(k);
                }
            }
            if (prefabHex.hexObjectsRoot.transform.Find(b.name) != null){
                DestroyImmediate(prefabHex.hexObjectsRoot.transform.Find(b.name).gameObject);
            }
        }
        else
        {
            for (int i = 0; i < prefabHex.segments.Count; i++)
            {
                for (int k = 0; k < prefabHex.segments[i].plots.Count; k++)
                {
                    if (prefabHex.segments[i].plots[k].name == b.name)
                    {
                        prefabHex.segments[i].plots.RemoveAt(k);
                    }
                    if (prefabHex.segments[i].plotObjectRoot.transform.Find(b.name) != null)
                    {
                        DestroyImmediate(prefabHex.segments[i].plotObjectRoot.transform.Find(b.name).gameObject, true);
                    }
                }
            }
        }
        PrefabUtility.ApplyPrefabInstance(hexPrefab, InteractionMode.UserAction);
    }
}