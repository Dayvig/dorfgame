using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static ModelGame;

public class Dorf : MonoBehaviour
{

    public List<Vector2> waypoints = new List<Vector2>();

    public GameObject startDebugText;
    public GameObject endDebugText;

    HexTileCoordinate storedCoord;

    public DorfManager.DorfTaskInProgress taskInProgress;

    public enum DorfState
    {
        WALKING,
        PERFORMINGTASK,
        IDLE
    }

    public DorfTask currentTask = DorfTask.NONE;
    public Vector2 currentTaskTargetPos;
    public DorfState currentState = DorfState.IDLE;

    public float workRate = 1.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (waypoints.Count > 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, waypoints[0], 0.006f);
            if (Vector2.Distance(transform.position, waypoints[0]) < 0.1f)
            {
                waypoints.RemoveAt(0);
            }
        }
        if (taskInProgress != null && Vector2.Distance(transform.position, currentTaskTargetPos) < 0.1f)
        {
            currentState = DorfState.PERFORMINGTASK;
        }
    }

    public void addWaypoints(Vector2 target, Hex endpointHex)
    {
        HexTileCoordinate closestToStart = HexManager.instance.closestCoordinateToLoc(transform.position, endpointHex, false);
        HexTileCoordinate closestToEnd = HexManager.instance.closestCoordinateToLoc(target, endpointHex, false);
        waypoints.AddRange(HexManager.instance.pathFromPointToPoint(closestToStart, closestToEnd));
        waypoints.Add(target);
    }
}
