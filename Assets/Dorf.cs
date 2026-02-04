using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Dorf : MonoBehaviour
{

    public List<Vector2> waypoints = new List<Vector2>();

    public GameObject startDebugText;
    public GameObject endDebugText;

    HexTileCoordinate storedCoord;

    // Start is called before the first frame update
    void Start()
    {
        startDebugText = Instantiate(HexManager.instance.debugTextBox, Vector3.zero, Quaternion.identity);
        startDebugText.GetComponent<TextMeshPro>().text = "Start";
        endDebugText = Instantiate(HexManager.instance.debugTextBox, Vector3.zero, Quaternion.identity);
        endDebugText.GetComponent<TextMeshPro>().text = "End";

        startDebugText.SetActive(false);
        endDebugText.SetActive(false);

        Hex randomHex = HexManager.instance.hexes[Random.Range(0, HexManager.instance.hexes.Count - 1)];
        Hex randomHex2 = HexManager.instance.hexes[Random.Range(0, HexManager.instance.hexes.Count - 1)];

        HexTileCoordinate randomtileCoord = randomHex.NavigationPoints.Values.ElementAt(Random.Range(0, randomHex.NavigationPoints.Values.Count - 1));
        HexTileCoordinate randomtileCoord2 = randomHex2.NavigationPoints.Values.ElementAt(Random.Range(0, randomHex2.NavigationPoints.Values.Count - 1));
        storedCoord = randomtileCoord2;

        startDebugText.transform.position = randomtileCoord.absoluteLoc();
        endDebugText.transform.position = randomtileCoord2.absoluteLoc();
        startDebugText.SetActive(true); endDebugText.SetActive(true);

        waypoints.AddRange(HexManager.instance.pathFromPointToPoint(randomtileCoord, randomtileCoord2));
        transform.position = waypoints[0];

    }

    // Update is called once per frame
    void Update()
    {
        if (waypoints.Count == 0)
        {
            Hex randomHex = HexManager.instance.hexes[Random.Range(0, HexManager.instance.hexes.Count - 1)];
            Hex randomHex2 = HexManager.instance.hexes[Random.Range(0, HexManager.instance.hexes.Count - 1)];

            HexTileCoordinate randomtileCoord = randomHex.NavigationPoints.Values.ElementAt(Random.Range(0, randomHex.NavigationPoints.Values.Count - 1));
            HexTileCoordinate randomtileCoord2 = randomHex2.NavigationPoints.Values.ElementAt(Random.Range(0, randomHex2.NavigationPoints.Values.Count - 1));
            storedCoord = randomtileCoord2;

            startDebugText.transform.position = randomtileCoord.absoluteLoc();
            endDebugText.transform.position = randomtileCoord2.absoluteLoc();
            startDebugText.SetActive(true); endDebugText.SetActive(true);

            waypoints.AddRange(HexManager.instance.pathFromPointToPoint(randomtileCoord, randomtileCoord2));
            transform.position = waypoints[0];

        }else
        {
            transform.position = Vector2.MoveTowards(transform.position, waypoints[0], 0.002f);
            if (Vector2.Distance(transform.position, waypoints[0]) < 0.1f)
            {
                waypoints.Remove(waypoints[0]);
                if (waypoints.Count < 1)
                {
                    Hex randomHex = HexManager.instance.hexes[Random.Range(0, HexManager.instance.hexes.Count - 1)];
                    HexTileCoordinate randomtileCoord = randomHex.NavigationPoints.Values.ElementAt(Random.Range(0, randomHex.NavigationPoints.Values.Count - 1));
                    waypoints.AddRange(HexManager.instance.pathFromPointToPoint(storedCoord, randomtileCoord));

                    storedCoord = randomtileCoord;
                }
            }
        }
    }
}
