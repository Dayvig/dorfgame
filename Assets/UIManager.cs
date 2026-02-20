using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static ModelGame;

public class UIManager : MonoBehaviour
{
    public float tickCtr = 0.0f;
    public float interval = 1.0f;

    public Building currentlySelectedBuilding = null;
    public Building currentActiveBuildingChangingProperties = null;
    public DorfTask currentTask = DorfTask.NONE;

    public static UIManager instance
    {
        get; private set;
    }

    public TextMeshProUGUI foodCounter;
    public TextMeshProUGUI housingCounter;
    public TextMeshProUGUI rocksCounter;
    public TextMeshProUGUI rockDustCounter;
    public TextMeshProUGUI manureCounter;

    public TextMeshProUGUI foodClutterCounter;
    public TextMeshProUGUI rocksClutterCounter;
    public TextMeshProUGUI rockDustClutterCounter;
    public TextMeshProUGUI manureClutterCounter;

    public List<TextMeshProUGUI> buildingCostTexts = new List<TextMeshProUGUI>();
    public List<Building> allBuildings = new List<Building>();
    private void Start()
    {
        instance = this;
    }

    private void Awake()
    {
    }

    private void Update()
    {
        updateCosts();

        if (tickCtr < 0.0f)
        {
            updateCounterDisplay();
            tickCtr += interval;
        }
        else
        {
            tickCtr -= Time.deltaTime;
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (currentActiveBuildingChangingProperties != null)
            {
                currentActiveBuildingChangingProperties.deselect();
            }
            currentActiveBuildingChangingProperties = null;
            currentlySelectedBuilding = null;
            HexManager.instance.changeMode(HexManager.SelectionMode.HEX);
        }
    }

    public void updateCounterDisplay()
    {
        foodCounter.text = "" + ResourceManager.instance.Food;
        housingCounter.text = "0 / " + ResourceManager.instance.Housing;
        rocksCounter.text = "" + ResourceManager.instance.Rocks;
        rockDustCounter.text = "" + ResourceManager.instance.RockDust;
        manureCounter.text = "" + ResourceManager.instance.Manure;

        foodClutterCounter.text = "" + ResourceManager.instance.FoodClutter;
        rocksClutterCounter.text = "" + ResourceManager.instance.RockClutter;
        manureClutterCounter.text = "" + ResourceManager.instance.ManureClutter;
        //rockDustClutterCounter.text = "" + ResourceManager.instance.RockDustClutter;

    }

    void updateCosts()
    {
        for (int i = 0; i < buildingCostTexts.Count; i++)
        {
            string CostString = "Cost:" + "\n";
            foreach (Building.BuildingCost b in allBuildings[i].costs)
            {
                CostString += b.type + " " + b.numericalCost + "\n";
            }
            buildingCostTexts[i].text = CostString;
        }
    }
}
