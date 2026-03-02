using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class BuildingAdderTool : MonoBehaviour
{
    public GameObject hexPrefab;
    public Hex prefabHex;
    public GameObject buildingObject;
    public List<Vector2> offSets = new List<Vector2>();
    public bool bigBuilding = false;
    public void addBuilding()
    {
        if (bigBuilding)
        {
            Object GO = PrefabUtility.InstantiatePrefab(buildingObject);
            GameObject goObj = (GameObject)GO;
            Building goBuilding = goObj.GetComponent<Building>();

            prefabHex.bigBuildings.Add(goBuilding);
            goObj.transform.parent = prefabHex.hexObjectsRoot.transform;
            goObj.transform.localPosition = new Vector3(0f, 0f, 0f);
            goBuilding.parentHex = prefabHex;

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
                if (goBuilding is SegmentBuilding)
                {
                    SegmentBuilding segment = (SegmentBuilding)goBuilding;
                    segment.parentSegment = prefabHex.segments[i];
                }
                goBuilding.parentHex = prefabHex;
            }
        }

        PrefabUtility.ApplyPrefabInstance(hexPrefab, InteractionMode.UserAction);
    }
    public void removeBuilding()
    {
        Building goBuilding = buildingObject.GetComponent<Building>();

        List<GameObject> toDie = new List<GameObject>();
        if (bigBuilding)
        {
            for (int k = 0; k < prefabHex.bigBuildings.Count; k++)
            {
                if (prefabHex.bigBuildings[k].ID.Equals(goBuilding.ID))
                {
                    prefabHex.bigBuildings.RemoveAt(k);
                }
            }
        }
        else
        {
            for (int i = 0; i < prefabHex.segments.Count; i++)
            {
                for (int k = 0; k < prefabHex.segments[i].plots.Count; k++)
                {
                    if (prefabHex.segments[i].plots[k].ID.Equals(goBuilding.ID))
                    {
                        toDie.Add(prefabHex.segments[i].plots[k].gameObject);
                        prefabHex.segments[i].plots.RemoveAt(k);
                    }
                }
                foreach (GameObject go in toDie)
                {
                    DestroyImmediate(go, true);
                }
                toDie.Clear();
            }
        }
        PrefabUtility.ApplyPrefabInstance(hexPrefab, InteractionMode.UserAction);
    }
}