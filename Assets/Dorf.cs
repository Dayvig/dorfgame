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
    public List<WorldResource> resourcesToPickUp = new List<WorldResource>();
    public List<WorldResource> heldResources = new List<WorldResource>();

    public GameObject startDebugText;
    public GameObject endDebugText;

    HexTileCoordinate storedCoord;
    public float stateUpdateInterval = 1.0f;
    public float stateTimer = 0.0f;

    public float fullness = 0.0f;
    public float mealInterval = 10.0f;

    public float horniness = 0.0f;
    public float sexInterval = 60.0f;

    public DorfManager.DorfTaskInProgress taskInProgress;

    public Building home = null;
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

    public float carryingCapacity = 10;
    public float currentHaul = 0;

    public float maxFood = 100f;
    public float currentFood = 50f;

    public Dorf spouse;

    public float starvationOdds = 5f;
    public float baseStarvationOdds = 0.5f;
    public float starvationFlashTimer = 0.0f;
    public float starvationFlashInterval = 1.0f;
    public bool fadeIn = true;
    public SpriteRenderer flashRen;

    // Start is called before the first frame update
    public void init()
    {
        currentFood = 40f;
        DorfManager.instance.dorves.Add(this);
        this.currentTask = DorfTask.NONE;
        this.currentState = DorfState.IDLE;
    }

    // Update is called once per frame
    void Update()
    {
        bool starving = currentFood / maxFood <= 0.25f;
        if (starving)
        {
            flashRen.gameObject.SetActive(true);
            starvationFlashInterval = (currentFood / maxFood) * 10f;
            starvationFlashTimer += Time.deltaTime;
            flashRen.color = new Color(flashRen.color.r, flashRen.color.g, flashRen.color.b, fadeIn ? starvationFlashTimer / starvationFlashInterval : (1f - (starvationFlashTimer / starvationFlashInterval)));
            if (starvationFlashTimer > starvationFlashInterval)
            {
                starvationFlashTimer -= starvationFlashInterval;
                fadeIn = !fadeIn;
            }
        }
        else
        {
            flashRen.gameObject.SetActive(false);
            starvationOdds = baseStarvationOdds;
        }

        if (stateTimer > stateUpdateInterval)
        {
            currentFood -= 0.1f;
            stateTimer -= stateUpdateInterval;
            if (currentFood/maxFood <= 0.1f)
            {
                float rand = Random.Range(0, 100);
                if (rand <= starvationOdds)
                {
                    DorfManager.instance.dorfGraveyard.Add(this);
                }
                else
                {
                    starvationOdds += 0.1f;
                }
            }
        }
        else
        {
            stateTimer += Time.deltaTime;
        }
        fullness += Time.deltaTime;
        horniness += Time.deltaTime;

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
        foreach (WorldResource w in heldResources)
        {
            w.transform.position = this.transform.position + new Vector3(0f, -0.25f, 0f);
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
