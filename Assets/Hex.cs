using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ModelGame;

public class Hex : MonoBehaviour
{
    public Dictionary<string, HexTileCoordinate> NavigationPoints = new Dictionary<string, HexTileCoordinate>();
    //public GameObject cursor;
    public int next = 0;

    public Hex[] neighbors = new Hex[6];

    public float f;
    public float g;
    public float h;

    public float gridCoordX; //col
    public float gridCoordY; //row

    public Hex parentHex;

    public GameObject tint;

    public List<Segment> segments = new List<Segment>();

    public PolygonCollider2D mainHexCollider;

    public List<Building> activeBuildings = new List<Building>();
    public List<Feature> features = new List<Feature>();
    public List<Feature> activeFeatures = new List<Feature>();
    public List<Feature> toRemove = new List<Feature>();

    // Start is called before the first frame update
    void Awake()
    {
        NavigationPoints.Add("Eastern Navpoint", new HexTileCoordinate(new Vector2(0.7f, 0f), this));
        NavigationPoints.Add("Western Navpoint", new HexTileCoordinate(new Vector2(0.7f, 0f), this));
        NavigationPoints.Add("Center", new HexTileCoordinate(new Vector2(0f, 0f), this));

        NavigationPoints.Add("Top", new HexTileCoordinate (new Vector2(0f, 0.875f), 1, this));
        NavigationPoints.Add("Bottom", new HexTileCoordinate(new Vector2(0f, -0.875f), 7, this));
        NavigationPoints.Add("Right", new HexTileCoordinate(new Vector2(0.975f, 0f), 4, this));
        NavigationPoints.Add("Left", new HexTileCoordinate(new Vector2(-0.975f, 0f), 10, this));

        NavigationPoints.Add("SW Corner", new HexTileCoordinate(new Vector2(-0.475f, -0.875f), 9, this).withNavPair(NavigationPoints.GetValueOrDefault("Bottom")));
        NavigationPoints.Add("SE Corner", new HexTileCoordinate(new Vector2(0.475f, -0.875f), 5, this).withNavPair(NavigationPoints.GetValueOrDefault("Bottom")));
        NavigationPoints.Add("NW Corner", new HexTileCoordinate(new Vector2(-0.475f, 0.875f), 11, this).withNavPair(NavigationPoints.GetValueOrDefault("Top")));
        NavigationPoints.Add("NE Corner", new HexTileCoordinate(new Vector2(0.475f, 0.875f), 3, this).withNavPair(NavigationPoints.GetValueOrDefault("Top")));

        NavigationPoints.Add("NE Edge", new HexTileCoordinate(new Vector2(0.7f, 0.4375f), 2, this).withNavPair(NavigationPoints.GetValueOrDefault("Eastern Navpoint")));
        NavigationPoints.Add("SE Edge", new HexTileCoordinate(new Vector2(0.7f, -0.4375f), 6, this).withNavPair(NavigationPoints.GetValueOrDefault("Eastern Navpoint")));
        NavigationPoints.Add("NW Edge", new HexTileCoordinate(new Vector2(-0.7f, 0.4375f), 12, this).withNavPair(NavigationPoints.GetValueOrDefault("Western Navpoint")));
        NavigationPoints.Add("SW Edge", new HexTileCoordinate(new Vector2(-0.7f, -0.4375f), 8, this).withNavPair(NavigationPoints.GetValueOrDefault("Western Navpoint")));

    }

    private void Update()
    {

    }

    public void ResetPathfindingValues()
    {
        f = 0;
        g = 0;
        h = 0;
        parentHex = null;
    }
    public void setFeature(Feature f)
    {
        foreach (Feature test in features)
        {
            if (test.name.Equals(f.name))
            {
                test.activate();
            }
        }
    }

    public void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        tint.gameObject.SetActive(false);
        foreach (Feature f in activeFeatures)
        {
            f.onHover();
        }
        HexManager.instance.hexHovered = this;
        cleanUp();
    }

    public void OnMouseUp()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (HexManager.instance.hexHovered == this)
        {
            foreach (Feature f in activeFeatures)
            {
                f.onClick();
            }
        }
        cleanUp();
    }

    public void OnMouseExit()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        tint.SetActive(false);
        if (HexManager.instance.hexHovered == this)
        {
            foreach (Feature f in activeFeatures)
            {
                f.onUnHover();
            }
        }
        cleanUp();
    }

    void cleanUp()
    {
        foreach (Feature f in toRemove)
        {
            f.remove();
        }
        toRemove.Clear();
    }
}

public class HexTileCoordinate
{
    public Vector2 localSpaceLocation;
    public int edgeId;
    public HexTileCoordinate navPair;
    public bool hasNavPair = false;
    public Hex parentHex;

    public HexTileCoordinate(Vector2 loc, int id, Hex parentHex)
    {
        this.localSpaceLocation = loc;
        this.edgeId = id;
        this.parentHex = parentHex;
    }

    public HexTileCoordinate(Vector2 loc, Hex parentHex)
    {
        this.localSpaceLocation = loc;
        this.edgeId = -99;
        this.parentHex = parentHex;
    }

    public HexTileCoordinate withNavPair(HexTileCoordinate h)
    {
        hasNavPair = true;
        this.navPair = h;
        return this;
    }

    public Vector2 absoluteLoc()
    {
        return (Vector2)parentHex.transform.position + (Vector2)localSpaceLocation;

    }


}
