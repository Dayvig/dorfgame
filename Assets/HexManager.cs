using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class HexManager : MonoBehaviour
{
    public List<Hex> hexes = new List<Hex>();
    public GameObject hexObject;
    public GameObject gridCenter;
    public bool generateNewGrid = false;

    int depth = 12;

    public Hex heldStartLoc;
    public Hex heldEndLoc;

    public Hex hexHovered;
    public Segment segmentHovered;

    public GameObject debugTextBox;

    public List<Hex> hexPath= new List<Hex>();
    public List<Feature> allFeatures = new List<Feature>();

    public enum SelectionMode
    {
        HEX,
        SEGMENT
    }

    public SelectionMode currentSelectionMode = SelectionMode.SEGMENT;

    public static HexManager instance
    {
        get; private set;
    }

    private void Start()
    {
        instance = this;
    }

    List<Vector2> offsets = new List<Vector2>
    {
        new Vector2(0f, 1.875f),
        new Vector2(1.5f, 0.925f),
        new Vector2(1.5f, -0.925f),
        new Vector2(0f, -1.875f),
        new Vector2(-1.5f, -0.925f),
        new Vector2(-1.5f, 0.925f)
    };

    public List<Hex> FindPathAStar (Hex start, Hex end)
    {
        foreach (Hex h in hexes)
        {
            h.ResetPathfindingValues();
        }

        List<Hex> openList = new List<Hex>();
        List<Hex> closedList = new List<Hex>();
        List<Hex> path = new List<Hex>();

        Hex currentHex = start;


        openList.Add(start);

        int iter = 0;

        while (openList.Count > 0)
        {
            iter++;
            if (iter > 10000) { Debug.Log("Reached 10000 iters"); return path; }
            float least = 999;
            foreach (Hex open in openList)
            {
                if (open.f < least)
                {
                    least = open.f;
                    currentHex = open;
                }
            }
            openList.Remove(currentHex);
            closedList.Add(currentHex);

            if (currentHex == end)
            {
                Hex cursorHex = end;
                do
                {
                    path.Add(cursorHex);
                    cursorHex = cursorHex.parentHex;
                }
                while (cursorHex.parentHex != null);
                return path;
            }

            foreach (Hex hex in currentHex.neighbors)
            {
                if (hex == null || closedList.Contains(hex)) { continue; }

                hex.g = currentHex.g + Vector3.Distance(hex.gameObject.transform.position, currentHex.gameObject.transform.position);
                hex.h = Vector3.Distance(hex.transform.position, end.transform.position);
                hex.f = hex.g + hex.h;


                if (!openList.Contains(hex)) { 
                    openList.Add(hex);
                    hex.parentHex = currentHex;
                }
                else
                {
                   float newG = currentHex.g + Vector3.Distance(hex.gameObject.transform.position, currentHex.gameObject.transform.position);
                   if (newG < hex.g)
                    {
                        hex.g = newG;
                        hex.parentHex = currentHex;
                        hex.h = Vector3.Distance(hex.transform.position, end.transform.position);
                        hex.f = hex.g + hex.h;
                    }
                }
            }
        }
        Debug.Log("Unable to find path");
        return path;
    }
    public void generateHexGrid()
    {
        GameObject nextHex;

        Vector2 totalOffset;

        //move to next row
        for (int k = 0; k < depth; k++)
        {
            totalOffset = Vector2.zero + (offsets[3] * k);
            //create cells in row
            for (int i = 0; i < depth; i++)
            {
                totalOffset += (i % 2 == 0) ? offsets[1] : offsets[2];

                nextHex = Instantiate(hexObject, gridCenter.transform.position + (Vector3)totalOffset, Quaternion.identity, gridCenter.transform);
                Hex hexScript = nextHex.GetComponent<Hex>();
                hexScript.gridCoordX = i;
                hexScript.gridCoordY = k;
                nextHex.name = "Hex " + hexScript.gridCoordX + "," + hexScript.gridCoordY;

                switch (i, k)
                {
                    case (7, 11):
                            break;
                    case (8, 11):
                        break;
                    case (6, 11):
                        break;
                    case (7, 10):
                        break;
                    case (8, 10):
                        break;
                    default:
                        hexScript.setFeature(getFeatureByName("Stone"));
                        break;
                }

                hexes.Add(hexScript);
            }
        }


    }
    public void assignNeighbors(List<Hex> grid)
    {
        foreach (Hex hex in grid)
        {
            //if hex on even column
            if (hex.gridCoordX % 2 == 0)
            {
                foreach (Hex target in grid)
                {
                    //continue checking if target is current hex or at least 2 away
                    if (target == hex || Mathf.Abs(target.gridCoordX-hex.gridCoordX) > 1 || Mathf.Abs(target.gridCoordY - hex.gridCoordY) > 1) { continue; }

                    //North
                    if (target.gridCoordY == hex.gridCoordY - 1 && target.gridCoordX == hex.gridCoordX)
                    {
                        hex.neighbors[0] = target;
                    }
                    //NorthEast
                    if (target.gridCoordY == hex.gridCoordY - 1 && target.gridCoordX == hex.gridCoordX + 1)
                    {
                        hex.neighbors[1] = target;
                    }
                    //SouthEast
                    if (target.gridCoordY == hex.gridCoordY && target.gridCoordX == hex.gridCoordX + 1)
                    {
                        hex.neighbors[2] = target;
                    }
                    //South
                    if (target.gridCoordY == hex.gridCoordY+1 && target.gridCoordX == hex.gridCoordX)
                    {
                        hex.neighbors[3] = target;
                    }
                    //SouthWest
                    if (target.gridCoordY == hex.gridCoordY && target.gridCoordX == hex.gridCoordX - 1)
                    {
                        hex.neighbors[4] = target;
                    }
                    //SouthEast
                    if (target.gridCoordY == hex.gridCoordY-1 && target.gridCoordX == hex.gridCoordX - 1)
                    {
                        hex.neighbors[5] = target;
                    }

                }
            }
            //hex is on odd column
            else
            {
                foreach (Hex target in grid)
                {
                    //continue checking if target is current hex or at least 2 away
                    if (target == hex || Mathf.Abs(target.gridCoordX - hex.gridCoordX) > 1 || Mathf.Abs(target.gridCoordY - hex.gridCoordY) > 1) { continue; }

                    //North
                    if (target.gridCoordY == hex.gridCoordY - 1 && target.gridCoordX == hex.gridCoordX)
                    {
                        hex.neighbors[0] = target;
                    }
                    //NorthEast
                    if (target.gridCoordY == hex.gridCoordY && target.gridCoordX == hex.gridCoordX + 1)
                    {
                        hex.neighbors[1] = target;
                    }
                    //SouthEast
                    if (target.gridCoordY == hex.gridCoordY + 1 && target.gridCoordX == hex.gridCoordX + 1)
                    {
                        hex.neighbors[2] = target;
                    }
                    //South
                    if (target.gridCoordY == hex.gridCoordY + 1 && target.gridCoordX == hex.gridCoordX)
                    {
                        hex.neighbors[3] = target;
                    }
                    //SouthWest
                    if (target.gridCoordY == hex.gridCoordY+1 && target.gridCoordX == hex.gridCoordX - 1)
                    {
                        hex.neighbors[4] = target;
                    }
                    //NorthWest
                    if (target.gridCoordY == hex.gridCoordY && target.gridCoordX == hex.gridCoordX-1)
                    {
                        hex.neighbors[5] = target;
                    }

                }
            }

        }
    }
    public List<Vector2> pathFromPointToPoint(HexTileCoordinate start, HexTileCoordinate end) {

        List<Vector2> path = new List<Vector2>();
        HexTileCoordinate nextCoord = null;

        //If in same hex
        if (start.parentHex == end.parentHex)
        {
            //If on an edge
            if (start.edgeId != -99 && end.edgeId != -99)
            {
                //If within 3 edge points
                if (Mathf.Abs(end.edgeId - start.edgeId) <= 3 || Mathf.Abs(end.edgeId - (start.edgeId + 13)) <= 3)
                {
                    //find closest corner
                    nextCoord = closestCornerCoordinateToLoc(start.absoluteLoc(), end.absoluteLoc(), start.parentHex);

                    //go to corner, then destination
                    path.Add(nextCoord.absoluteLoc());
                    path.Add(end.absoluteLoc());
                }
                else
                {
                    //if on corner or corner edge, add nav pair
                    if ((start.edgeId != -99) &&
                        !((start.edgeId - 1) % 3 == 0))
                    {
                        path.Add(start.navPair.absoluteLoc());
                    }
                    //then move to center
                    path.Add(start.parentHex.NavigationPoints.GetValueOrDefault("Center").absoluteLoc());

                    //if destination is on corner or corner edge, add its nav pair
                    if (end.edgeId != -99 &&
                        !((end.edgeId - 1) % 3 == 0))
                    {
                        path.Add(end.navPair.absoluteLoc());
                    }

                    //move to destination
                    path.Add(end.absoluteLoc());
                }
            }
            else
            {
                //if not on edge, simply go to destination
                path.Add(end.absoluteLoc());
            }

            return path;
        }

        //if in different hex
        else
        {
            Debug.Log("Different Hex iter");

            hexPath.Clear();
            hexPath = FindPathAStar(start.parentHex, end.parentHex);
            Debug.Log("pathctr:" + hexPath.Count);

            HexTileCoordinate cursorCoord = start;
            HexTileCoordinate tmp = null;

            for (int i = hexPath.Count-1; i >= 0; i--)
            {
                //find closest point in next hex
                nextCoord = closestCoordinateToLoc(cursorCoord.absoluteLoc(), hexPath[i].parentHex.transform.position, hexPath[i], true);

                //find closest coordinate in this hex's id that corrosponds to that hex (should be the same point in space)
                tmp = closestCoordinateToLoc(nextCoord.absoluteLoc(), cursorCoord.parentHex, true);

                if (tmp.parentHex != nextCoord.parentHex)
                {
                    Debug.Log("Point in Hex A not same as point in Hex B");
                }

                //run recursively to get inter-hex path
                path.AddRange(pathFromPointToPoint(cursorCoord, tmp));

                //update cursor
                cursorCoord = nextCoord;
            }
            //if in final hex, navigate recursively to point and return
            if (cursorCoord.parentHex == end.parentHex)
            {
                path.AddRange(pathFromPointToPoint(cursorCoord, end));
            }
            return path;
        }
    }
    HexTileCoordinate closestCoordinateToLoc(Vector2 currentPos, Vector2 dest, Hex toCheck, bool edgesOnly)
    {
        float least = -1;
        HexTileCoordinate toReturn = null;

        foreach (HexTileCoordinate h in toCheck.NavigationPoints.Values)
        {
            if (h.edgeId == -99 && edgesOnly) { continue; }

            float dist = (Vector2.Distance(currentPos, h.absoluteLoc()) + Vector2.Distance(dest, h.absoluteLoc()));

            if (least == -1 || dist < least)
            {
                least = dist;
                toReturn = h;
            }
        }
        return toReturn;
    }
    HexTileCoordinate closestCornerCoordinateToLoc(Vector2 currentPos, Vector2 dest, Hex toCheck)
    {
        float least = -1;
        HexTileCoordinate toReturn = null;

        foreach (HexTileCoordinate h in toCheck.NavigationPoints.Values)
        {
            if (h.edgeId == -99 || h.edgeId % 2 == 0) { continue; }

            float dist = (Vector2.Distance(currentPos, h.absoluteLoc()) + Vector2.Distance(dest, h.absoluteLoc()));

            if (least == -1 || dist < least)
            {
                least = dist;
                toReturn = h;
            }
        }
        return toReturn;
    }
    public HexTileCoordinate closestCoordinateToLoc(Vector2 currentPos, Hex toCheck, bool edgesOnly)
    {
        float least = -1;
        HexTileCoordinate toReturn = null;

        foreach (HexTileCoordinate h in toCheck.NavigationPoints.Values)
        {
            if (h.edgeId == -99 && edgesOnly) { continue; }

            float dist = (Vector2.Distance(currentPos, h.absoluteLoc()));
            if (least == -1 || dist < least)
            {
                least = dist;
                toReturn = h;
            }
        }
        return toReturn;
    }

    public Hex closestHexToLoc(Vector2 currentPos)
    {
        float least = -1;
        Hex toReturn = null;
        foreach (Hex h in hexes)
        {
            float dist = (Vector2.Distance(currentPos, h.gameObject.transform.position));
            if (least == -1 || dist < least)
            {
                least = dist;
                toReturn = h;
            }

        }
        return toReturn;
    }
    public bool canBePlaced(Building b, Segment s)
    { 
        return true;
    }
    public bool canBePlaced(Building b, Hex h)
    {
        return h.allSegmentsClear();
    }

    public Feature getFeatureByName(string s)
    {
        foreach (Feature f in allFeatures)
        {
            if (f.name.Equals(s))
            {
                return f;
            }
        }
        return null;
    }

    public void changeMode(SelectionMode mode)
    {
        HexManager.instance.currentSelectionMode = mode;

        switch (mode)
        {
            case SelectionMode.HEX:
                foreach (Hex h in hexes)
                {
                    h.mainHexCollider.enabled = true;
                }
                break;
            case SelectionMode.SEGMENT:
                foreach (Hex h in hexes)
                {
                    h.mainHexCollider.enabled = false;
                }
                break;
        }
    }


    void Update()
    {
        if (generateNewGrid)
        {
            foreach (Hex h in hexes)
            {
                DestroyImmediate(h.gameObject);
            }
            hexes.Clear();

            generateHexGrid();
            assignNeighbors(hexes);
            generateNewGrid = false;
        }

    }


}
