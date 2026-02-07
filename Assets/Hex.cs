using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public RectTransform progressBar;
    public Canvas taskbarCanvas;

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

    private void Start()
    {
    }

    private void Update()
    {
        cleanUp();
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

    public bool hasFeature(string feature)
    {
        foreach (Feature test in activeFeatures)
        {
            if (test.name.Equals(feature))
            {
                return true;
            }
        }
        return false;
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
    
    public List<Vector2> miningPoints()
    {
        List<HexTileCoordinate> points = new List<HexTileCoordinate>();
        List<Vector2> vector2s = new List<Vector2>();

        for (int i = 0; i < neighbors.Length; i++)
        {
            Hex current = neighbors[i];
            if (current != null && !current.hasFeature("Stone")){
                switch (i)
                {
                    case 0:
                        points.Add(NavigationPoints.GetValueOrDefault("Top"));
                        points.Add(NavigationPoints.GetValueOrDefault("NE Corner"));
                        points.Add(NavigationPoints.GetValueOrDefault("NW Corner"));
                        break;
                    case 1:
                        points.Add(NavigationPoints.GetValueOrDefault("NE Corner"));
                        points.Add(NavigationPoints.GetValueOrDefault("NE Edge"));
                        points.Add(NavigationPoints.GetValueOrDefault("Right"));
                        break;
                    case 2:
                        points.Add(NavigationPoints.GetValueOrDefault("SE Corner"));
                        points.Add(NavigationPoints.GetValueOrDefault("SE Edge"));
                        points.Add(NavigationPoints.GetValueOrDefault("Right"));
                        break;
                    case 3:
                        points.Add(NavigationPoints.GetValueOrDefault("SE Corner"));
                        points.Add(NavigationPoints.GetValueOrDefault("Bottom"));
                        points.Add(NavigationPoints.GetValueOrDefault("SW Corner"));
                        break;
                    case 4:
                        points.Add(NavigationPoints.GetValueOrDefault("SW Edge"));
                        points.Add(NavigationPoints.GetValueOrDefault("Left"));
                        points.Add(NavigationPoints.GetValueOrDefault("SW Corner"));
                        break;
                    case 5:
                        points.Add(NavigationPoints.GetValueOrDefault("NW Edge"));
                        points.Add(NavigationPoints.GetValueOrDefault("Left"));
                        points.Add(NavigationPoints.GetValueOrDefault("NW Corner"));
                        break;
                }
            }
        }
        foreach (HexTileCoordinate tile in points)
        {
            vector2s.Add(tile.absoluteLoc());
        }
        return vector2s.Distinct().ToList();
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
