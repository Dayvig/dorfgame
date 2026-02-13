using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;
using static ModelGame;
using static UnityEditor.ObjectChangeEventStream;
using static UnityEngine.GraphicsBuffer;

public class DorfManager : MonoBehaviour
{
    public static DorfManager instance
    {
        get; private set;
    }

    public List<Dorf> dorves = new List<Dorf>();
    public List<DorfTaskInProgress> allCurrentTasks = new List<DorfTaskInProgress>();
    public List<string> tasksInProgressDisplay = new List<string>();

    public List<DorfTaskInProgress> taskQueue = new List<DorfTaskInProgress>();
    public List<DorfTaskInProgress> tasksToRemove = new List<DorfTaskInProgress>();

    public List<WorldResource> clutter = new List<WorldResource>();

    public int globalTaskIDs = 0;
    public float taskAssignTimer = 0.0f;
    public float taskAssignDelay = 0.2f;

    private void Start()
    {
        instance = this;
    }

    private void Update()
    {
        taskAssignTimer += Time.deltaTime;

        if (taskAssignTimer > taskAssignDelay)
        {
            tasksInProgressDisplay.Clear();
            foreach (DorfTaskInProgress t in allCurrentTasks)
            {
                tasksInProgressDisplay.Add(t.type.ToString());
            }

            List<Dorf> idleDorves = new List<Dorf>();
            foreach (Dorf d in dorves)
            {
                if (d.currentState.Equals(Dorf.DorfState.IDLE))
                {
                    idleDorves.Add(d);
                }
            }
            if (idleDorves.Count > 0)
            {
                distributeDorves(idleDorves);
            }

            taskAssignTimer -= taskAssignDelay;
        }

        foreach (DorfTaskInProgress task in allCurrentTasks)
        {
            foreach (Dorf d in task.assignedDorves)
            {
                if (d.currentState == Dorf.DorfState.PERFORMINGTASK)
                {
                    if (d.currentTask.Equals(DorfTask.WORKBUILDING))
                    {
                        d.taskInProgress.targetBuilding.assignedDorves[d.taskInProgress.targetBuildingSlot] = d;
                    }
                    else
                    {
                        task.completionCtr += Time.deltaTime * d.workRate;
                        task.progressBar.sizeDelta = new Vector2((task.completionCtr / task.timeForTask) * task.maxTaskBarWidth, 0.3f);
                    }
                }
            }
            if (!task.type.Equals(DorfTask.WORKBUILDING) && task.completionCtr > task.timeForTask)
            {
                task.complete();
            }
        }
        foreach (DorfTaskInProgress t in taskQueue)
        {
            allCurrentTasks.Add(t);
        }
        taskQueue.Clear();

        foreach (DorfTaskInProgress t in tasksToRemove)
        {
            allCurrentTasks.Remove(t);
        }
        tasksToRemove.Clear();
    }

    void distributeDorves(List<Dorf> available)
    {
        //figure out how many dorves are idle
        List<Dorf> idleDorves = available;

        List<WorldResource> tmp = storableClutter();

        //assigns all idle dorves to tasks
        while (idleDorves.Count > 0)
        {
            BasicHouse house = null;
            //find house
            foreach (Hex h in HexManager.instance.hexes)
            {
                foreach (Building b in h.activeBuildings)
                {
                    if (b is BasicHouse)
                    {
                        house = (BasicHouse)b;
                    }
                }
            }

            //if starving, eat
            for (int i = 0; i < idleDorves.Count; i++)
            {
                Dorf d = idleDorves[i];
                if (d.currentFood <= d.maxFood * 0.25f)
                {
                    if (gatherFood(d, false, house))
                    {
                        Debug.Log("Eating food, starving");
                        idleDorves.Remove(d);
                    }
                }
            }
            if (idleDorves.Count <= 0)
            {
                return;
            }


            //pickup clutter
            if (tmp.Count != 0)
            {
                foreach (WorldResource clutter in tmp)
                {
                    createNewPickupTask(idleDorves[0], clutter);
                    idleDorves.Remove(idleDorves[0]);
                    if (idleDorves.Count == 0)
                    {
                        return;
                    }
                }
            }

            //find task with least dorves assigned
            int least = -1;
            DorfTaskInProgress targetTask = null;
            foreach (DorfTaskInProgress task in allCurrentTasks)
            {
                if ((least == -1 || task.assignedDorves.Count < least) && (task.assignedDorves.Count < task.maxDorves || task.maxDorves == -1))
                {
                    least = task.assignedDorves.Count;
                    targetTask = task;
                }
            }
            //assign dorf to task
            if (targetTask != null)
            {
                if (targetTask.assignedDorves.Count == 0 && !targetTask.type.Equals(DorfTask.WORKBUILDING))
                {
                    targetTask.start();
                }
                assignDorfToTask(idleDorves[0], targetTask);
                idleDorves.Remove(idleDorves[0]);
            }
            //no pressing tasks to attend to
            else
            {
                //eat food
                for (int i = 0; i < idleDorves.Count; i++)
                {
                    Dorf d = idleDorves[i];
                    if (d.currentFood <= d.maxFood * 0.75f)
                    {
                        if (gatherFood(d, true, house))
                        {
                            idleDorves.Remove(d);
                        }
                    }
                }
                return;
            }
        }
    }
    List<WorldResource> storableClutter()
    {
        List<WorldResource> result = new List<WorldResource>();
        if (clutter.Count == 0 || ResourceManager.instance.storageBuildings.Count == 0)
        {
            return result;
        }
        List<Building> matchedBuildings = new List<Building>();
        foreach (WorldResource w in clutter)
        {
            if (w.toBePickedUp)
            {
                continue;
            }
            foreach (Building b in ResourceManager.instance.storageBuildings)
            {
                foreach (Building.StorageSlot s in b.storage)
                {
                    if (s.type == w.type && s.occupiedStorage + w.value <= s.maxStorage)
                    {
                        matchedBuildings.Add(b);
                    }
                }
            }
            if (matchedBuildings.Count == 0)
            {
                continue;
            }
            result.Add(w);
        }
        return result;
    }

    void createNewPickupTask(Dorf assignee, WorldResource r)
    {
        Debug.Log("Creating new pickup task");
        DorfManager.DorfTaskInProgress thisTask;

        thisTask = new DorfManager.DorfTaskInProgress(0.1f, DorfTask.HAUL,
            () => { },
            r.gameObject.transform.position, r.thisHex);
        thisTask = thisTask.setMaxDorves(thisTask, 1).setResult(thisTask, () =>
        {
            Dorf currentDorf = assignee;
            currentDorf.heldResources.Add(r);
            currentDorf.currentHaul += r.weight;
            if (currentDorf.currentHaul >= currentDorf.carryingCapacity)
            {
                createNewStorageTask(currentDorf);
            }
            else
            {
                WorldResource closestResource = closestResourceToPickup(currentDorf.gameObject.transform.position, currentDorf.heldResources[0].type, true);
                if (closestResource != null)
                {
                    createNewPickupTask(currentDorf, closestResource);
                }
                else
                {
                     createNewStorageTask(currentDorf);
                }
            }
        });
        r.toBePickedUp = true;
        assignDorfToTask(assignee, thisTask);
        DorfManager.instance.taskQueue.Add(thisTask);
    }

    void createNewStorageTask(Dorf targetDorf)
    {
        Debug.Log("New Storage Task Created");
        if (targetDorf.heldResources[0] == null)
        {
            Debug.Log("Something went wrong, no resources held");
            return;
        }
        if (!hasValidStorageBuilding(targetDorf.heldResources[0].type, targetDorf.heldResources[0].value)){
            Debug.Log("Dropping Resource");
            foreach (WorldResource w in targetDorf.heldResources)
            {
                w.toBePickedUp = false;
            }
            targetDorf.currentHaul = 0;
            targetDorf.heldResources.Clear();
            return;
        }
        DorfManager.DorfTaskInProgress newTask;
        Building close = closestStorageBuilding(targetDorf.transform.position, targetDorf.heldResources[0].type, targetDorf.heldResources[0].value, true);
        Debug.Log("Depositing in closest storage Building" + close.name);

        newTask = new DorfManager.DorfTaskInProgress(0.1f, DorfTask.HAUL, close.transform.position, close);
        newTask = newTask.setMaxDorves(newTask, 1).setResult(newTask, () =>
        {
            if (targetDorf.heldResources.Count == 0)
            {
                Debug.Log("Something went wrong - no resources to deposit");
                return;
            }
            Debug.Log("Depositing...");
            foreach (Building.StorageSlot s in newTask.targetBuilding.storage)
            {
                if (s.type == targetDorf.heldResources[0].type)
                {

                    foreach (WorldResource w in targetDorf.heldResources)
                    {
                        if (s.occupiedStorage + w.value <= s.maxStorage)
                        {
                            s.occupiedStorage += w.value;
                            ResourceManager.instance.stowResource(w.type, (int)w.value);
                            ResourceManager.instance.toBeDestroyed.Add(w);
                            UIManager.instance.updateCounterDisplay();
                        }
                        else
                        {
                            w.toBePickedUp = false;
                        }
                    }
                }
            }
            targetDorf.currentHaul = 0;
            targetDorf.heldResources.Clear();
        });
        assignDorfToTask(targetDorf, newTask);
        DorfManager.instance.taskQueue.Add(newTask);
        
    }

    Building closestStorageBuilding(Vector2 position, ResourceManager.ResourceType resource, float value, bool dropOff)
    {
        Building closest = null;
        float least = -1;
        foreach (Building b in ResourceManager.instance.storageBuildings)
        {
            foreach (Building.StorageSlot s in b.storage)
            {
                if (s.type.Equals(resource) && ((dropOff && s.occupiedStorage + value <= s.maxStorage) || (!dropOff && s.occupiedStorage >= value)))
                {
                    float dist = Vector2.Distance(position, b.transform.position);
                    if (least == -1 || dist < least)
                    {
                        closest = b;
                        least = dist;
                    }
                }
            }
        }
        return closest;
    }

    bool hasValidStorageBuilding(ResourceManager.ResourceType resource, float value)
    {
        Building validBuilding = null;
        foreach (Building b in ResourceManager.instance.storageBuildings)
        {
            foreach (Building.StorageSlot s in b.storage)
            {
                if (s.type.Equals(resource) && s.occupiedStorage + value <= s.maxStorage)
                {
                    validBuilding = b;
                }
            }
        }
        return validBuilding != null;
    }


    WorldResource closestResourceToPickup(Vector2 position, ResourceManager.ResourceType resource, bool storableOnly)
    {
        WorldResource closest = null;
        float least = -1;
        List<WorldResource> available = storableOnly ? storableClutter() : clutter;
        foreach (WorldResource w in available)
        {
            if (w.type.Equals(resource) && !w.toBePickedUp)
            {
                float dist = Vector2.Distance(position, w.gameObject.transform.position);
                if (least == -1 || dist < least)
                {
                    closest = w;
                    least = dist;
                }
            }
        }
        return closest;
    }

    public bool gatherFood(Dorf hungry, bool comfortable, Building home)
    {
        DorfManager.DorfTaskInProgress thisTask;
        //if food exists
        bool foodExists = false;
        foreach (WorldResource w in clutter)
        {
            if (w.type == ResourceManager.ResourceType.FOOD)
            {
                foodExists = true;
                break;
            }
        }
        foreach (Building b in ResourceManager.instance.storageBuildings)
        {
            foreach (Building.StorageSlot s in b.storage)
            {
                if (s.type == ResourceManager.ResourceType.FOOD && s.occupiedStorage != 0)
                {
                    foodExists = true;
                    break;
                }
            }
            if (foodExists) { break; }
        }
        if (!foodExists)
        {
            return false;
        }

        //if at max capacity
        if (hungry.currentHaul >= hungry.carryingCapacity)
        {
            eatFood(hungry, comfortable, home);
            return true;
        }

        //find next loose food item
        WorldResource r = closestResourceToPickup(hungry.gameObject.transform.position, ResourceManager.ResourceType.FOOD, false);

        //if none exist and storage exists, go to storage and pickup to max
        if (r == null)
        {

            Building close = closestStorageBuilding(hungry.gameObject.transform.position, ResourceManager.ResourceType.FOOD, 1, false);
            if (close != null)
            {
                thisTask = new DorfManager.DorfTaskInProgress(0.1f, DorfTask.HAUL,
                () => { },
                close.gameObject.transform.position, close.parentHex);
                thisTask = thisTask.setMaxDorves(thisTask, 1).setResult(thisTask, () =>
                {
                    WorldResource newResource = null;
                    foreach (Building.StorageSlot slot in close.storage)
                    {
                        if (slot.type == ResourceManager.ResourceType.FOOD)
                        {
                            newResource = ResourceManager.instance.createNewWorldResource(close.parentHex, ResourceManager.ResourceType.FOOD, this.gameObject.transform.position, 1.0f);
                            float valueToWeight = newResource.value / newResource.weight;
                            newResource.weight = hungry.carryingCapacity - hungry.currentHaul;
                            newResource.value = valueToWeight * newResource.weight;

                            if (newResource.value > slot.occupiedStorage)
                            {
                                newResource.value = slot.occupiedStorage;
                                newResource.weight = (1/valueToWeight) * newResource.weight;
                            }
                            slot.occupiedStorage -= newResource.value;
                            UIManager.instance.updateCounterDisplay();
                        }
                    }
                    if (newResource != null)
                    {
                        hungry.heldResources.Add(newResource);
                        eatFood(hungry, comfortable, home);
                    }
                });
                assignDorfToTask(hungry, thisTask);
                DorfManager.instance.taskQueue.Add(thisTask);
                return true;
            }
        }
        else
        {
            thisTask = new DorfManager.DorfTaskInProgress(0.1f, DorfTask.HAUL,
            () => { },
            r.gameObject.transform.position, r.thisHex);
            thisTask = thisTask.setMaxDorves(thisTask, 1).setResult(thisTask, () =>
            {
                Dorf currentDorf = hungry;
                currentDorf.heldResources.Add(r);
                currentDorf.currentHaul += r.weight;
                ResourceManager.instance.stowResource(ResourceManager.ResourceType.FOOD, (int)r.value);
                if (comfortable)
                {
                    gatherFood(hungry, comfortable, home);
                }
                else
                {
                    eatFood(hungry, comfortable, home);
                }
            });
            assignDorfToTask(hungry, thisTask);
            DorfManager.instance.taskQueue.Add(thisTask);
            return true;
        }
        return false;
    }
    public bool setConstructionSite(Building toConstruct)
    {
        DorfManager.DorfTaskInProgress thisTask;

        thisTask = new DorfManager.DorfTaskInProgress(0.1f, DorfTask.BUILD,
        () => { },
        toConstruct.gameObject.transform.position, toConstruct.parentHex);
        thisTask = thisTask.setResult(thisTask, () =>
        {
            foreach (Dorf d in thisTask.wereAssigned)
            {
                Debug.Log("Dorf");
                gatherBuildingResource(d, toConstruct);
            }
        });
        DorfManager.instance.taskQueue.Add(thisTask);
        return true;
    }

    public bool gatherBuildingResource(Dorf builder, Building toConstruct)
    {
        Debug.Log("Gathering Building Resource");
        DorfManager.DorfTaskInProgress thisTask;
        List<ResourceManager.ResourceType> resourcesLeftToTake = new List<ResourceManager.ResourceType>();

        //if at max capacity or has required resources
        if (builder.currentHaul >= builder.carryingCapacity)
        {
            moveToBuildingSite(builder, toConstruct);
            return true;
        }
        else
        {
            bool allReqMet = true;
            foreach (Building.BuildingCost gatheredRes in toConstruct.gatheredBuildingResources)
            {
                foreach (Building.BuildingCost cost in toConstruct.costs)
                {
                    if (cost.type == gatheredRes.type)
                    {
                        if (!(gatheredRes.numericalCost >= cost.numericalCost)) {
                            allReqMet = false;
                            resourcesLeftToTake.Add(gatheredRes.type);
                        }
                    }
                }
            }
            if (allReqMet)
            {
                moveToBuildingSite(builder, toConstruct);
                return true;
            }
        }

        Debug.Log("Null check, has no resources left to take: " + (resourcesLeftToTake.Count == 0));

        ResourceManager.ResourceType nextResourceType = resourcesLeftToTake[0];

        //find next loose resource item
        WorldResource r = closestResourceToPickup(builder.gameObject.transform.position, nextResourceType, false);

        //if none exist and storage exists, go to storage and pickup to max
        if (r == null)
        {

            Building close = closestStorageBuilding(builder.gameObject.transform.position, nextResourceType, 1, false);
            if (close != null)
            {
                thisTask = new DorfManager.DorfTaskInProgress(0.1f, DorfTask.HAUL,
                () => { },
                close.gameObject.transform.position, close.parentHex);
                thisTask = thisTask.setMaxDorves(thisTask, 1).setResult(thisTask, () =>
                {
                    WorldResource newResource = null;
                    foreach (Building.StorageSlot slot in close.storage)
                    {
                        if (slot.type == nextResourceType)
                        {
                            newResource = ResourceManager.instance.createNewWorldResource(close.parentHex, nextResourceType, this.gameObject.transform.position, 1.0f);

                            float targetValue = 0;
                            foreach (Building.BuildingCost costToCheck in toConstruct.costs)
                            {
                                if (newResource.type == costToCheck.type)
                                {
                                    targetValue = costToCheck.numericalCost;
                                }
                            }
                            float valueToWeight = newResource.value / newResource.weight;
                            if (((targetValue * valueToWeight) + builder.currentHaul) > builder.carryingCapacity)
                            {
                                targetValue = (builder.carryingCapacity - builder.currentHaul) * (1 / valueToWeight); 
                            }

                            newResource.weight = targetValue * valueToWeight;
                            newResource.value = targetValue;

                            if (newResource.value > slot.occupiedStorage)
                            {
                                newResource.value = slot.occupiedStorage;
                                newResource.weight = (1 / valueToWeight) * newResource.weight;
                            }
                            slot.occupiedStorage -= newResource.value;
                            UIManager.instance.updateCounterDisplay();
                        }
                    }
                    if (newResource != null)
                    {
                        builder.heldResources.Add(newResource);
                        foreach (Building.BuildingCost gathered in toConstruct.gatheredBuildingResources)
                        {
                            if (gathered.type == newResource.type)
                            {
                                gathered.numericalCost += (int)newResource.value;
                            }
                        }
                        gatherBuildingResource(builder, toConstruct);
                    }
                });
                assignDorfToTask(builder, thisTask);
                DorfManager.instance.taskQueue.Add(thisTask);
                return true;
            }
        }
        else
        {
            thisTask = new DorfManager.DorfTaskInProgress(0.1f, DorfTask.HAUL,
            () => { },
            r.gameObject.transform.position, r.thisHex);
            thisTask = thisTask.setMaxDorves(thisTask, 1).setResult(thisTask, () =>
            {
                Debug.Log("Resolving");
                Dorf currentDorf = builder;
                currentDorf.heldResources.Add(r);
                currentDorf.currentHaul += r.weight;
                ResourceManager.instance.stowResource(r.type, (int)r.value);

                foreach (Building.BuildingCost gathered in toConstruct.gatheredBuildingResources)
                {
                    if (gathered.type == r.type)
                    {
                        gathered.numericalCost += (int)r.value;
                    }
                }

                if (currentDorf.currentHaul >= currentDorf.carryingCapacity || hasEnoughMaterials(toConstruct))
                {
                    moveToBuildingSite(currentDorf, toConstruct);
                }
                else
                {
                    WorldResource closestResource = closestResourceToPickup(currentDorf.gameObject.transform.position, currentDorf.heldResources[0].type, false);
                    if (closestResource != null)
                    {
                        gatherBuildingResource(builder, toConstruct);
                    }
                    else
                    {
                        moveToBuildingSite(currentDorf, toConstruct);
                    }
                }
            });
            r.toBePickedUp = true;
            assignDorfToTask(builder, thisTask);
            DorfManager.instance.taskQueue.Add(thisTask);
            return true;
        }
        return false;
    }

    public bool hasEnoughMaterials(Building toConstruct)
    {
        bool allReqMet = true;
        foreach (Building.BuildingCost tmp in toConstruct.gatheredBuildingResources)
        {
            foreach (Building.BuildingCost cost in toConstruct.costs)
            {
                if (cost.type == tmp.type)
                {
                    if (!(tmp.numericalCost >= cost.numericalCost))
                    {
                        allReqMet = false;
                        break;
                    }
                }
            }
        }
        return allReqMet;
    }

    public void moveToBuildingSite(Dorf builder, Building toConstruct)
    {
        DorfManager.DorfTaskInProgress thisTask;
        thisTask = new DorfManager.DorfTaskInProgress(0f, DorfTask.HAUL,
        () => { },
        toConstruct.gameObject.transform.position, toConstruct.parentHex);
        thisTask = thisTask.setMaxDorves(thisTask, 1).setResult(thisTask, () =>
        {
            constructBuilding(builder, toConstruct);
        });
        assignDorfToTask(builder, thisTask);
        DorfManager.instance.taskQueue.Add(thisTask);
    }

    public void constructBuilding(Dorf builder, Building toConstruct)
    {
        DorfManager.DorfTaskInProgress thisTask;

        if (hasEnoughMaterials(toConstruct))
        {
            foreach (WorldResource r in builder.heldResources)
            {
                ResourceManager.instance.toBeDestroyed.Add(r);
                ResourceManager.instance.consumeResource(r.type, (int)r.value, false);
                UIManager.instance.updateCounterDisplay();
            }
            builder.currentHaul = 0;
            builder.heldResources.Clear();

            thisTask = new DorfManager.DorfTaskInProgress(toConstruct.constructionTime, DorfTask.BUILD,
            () => { },
            toConstruct.gameObject.transform.position, toConstruct.parentHex);
            thisTask = thisTask.setMaxDorves(thisTask, 4).setResult(thisTask, () =>
            {
                toConstruct.onPlace();
                toConstruct.parentHex.activeBuildings.Add(toConstruct);
                toConstruct.visual.color = new Color(1f, 1f, 1f, 1f);
                toConstruct.visual.gameObject.SetActive(true);
                if (toConstruct is SegmentBuilding)
                {
                    SegmentBuilding segmentBuilding = (SegmentBuilding)toConstruct;
                    segmentBuilding.parentSegment.occupied = true;
                }
                else
                {
                    toConstruct.parentHex.bigBuildings.Add(toConstruct);
                    foreach (Segment s in toConstruct.parentHex.segments)
                    {
                        s.occupied = true;
                    }
                }
                toConstruct.isBuilding = false;
                toConstruct.isActive = true;
            });
            thisTask.start();
            assignDorfToTask(builder, thisTask);
            DorfManager.instance.taskQueue.Add(thisTask);
        }
        else
        {
            foreach (WorldResource r in builder.heldResources)
            {
                ResourceManager.instance.toBeDestroyed.Add(r);
                UIManager.instance.updateCounterDisplay();
            }
            builder.currentHaul = 0;
            builder.heldResources.Clear();
            gatherBuildingResource(builder, toConstruct);
        }
    }
    public void eatFood(Dorf hungry, bool comfortable, Building home)
    {
        DorfManager.DorfTaskInProgress thisTask;
        if (comfortable)
        {
            thisTask = new DorfManager.DorfTaskInProgress((hungry.currentHaul / hungry.carryingCapacity) * 2.0f, DorfTask.EAT,
            () => { },
            home.gameObject.transform.position, home.parentHex);
            thisTask = thisTask.setMaxDorves(thisTask, 1).setResult(thisTask, () =>
            {
                Dorf currentDorf = hungry;
                foreach (WorldResource r in hungry.heldResources)
                {
                    hungry.currentFood += r.value;
                    ResourceManager.instance.toBeDestroyed.Add(r);
                    ResourceManager.instance.consumeResource(ResourceManager.ResourceType.FOOD, (int)r.value, false);
                    UIManager.instance.updateCounterDisplay();
                }
                hungry.currentHaul = 0;
                hungry.heldResources.Clear();
            });
            assignDorfToTask(hungry, thisTask);
            DorfManager.instance.taskQueue.Add(thisTask);
        }
        else
        {
            thisTask = new DorfManager.DorfTaskInProgress((hungry.currentHaul / hungry.carryingCapacity) * 2.0f, DorfTask.EAT,
            () => { },
            hungry.gameObject.transform.position, HexManager.instance.closestHexToLoc(hungry.gameObject.transform.position));
            thisTask = thisTask.setMaxDorves(thisTask, 1).setResult(thisTask, () =>
            {
                Dorf currentDorf = hungry;
                foreach (WorldResource r in hungry.heldResources)
                {
                    hungry.currentFood += r.value;
                    ResourceManager.instance.toBeDestroyed.Add(r);
                    ResourceManager.instance.consumeResource(ResourceManager.ResourceType.FOOD, (int)r.value, false);
                    UIManager.instance.updateCounterDisplay();
                }
                hungry.currentHaul = 0;
                hungry.heldResources.Clear();
            });
            assignDorfToTask(hungry, thisTask);
            DorfManager.instance.taskQueue.Add(thisTask);

        }

    }

    public void assignTask(DorfTaskInProgress task)
    {
        foreach (DorfTaskInProgress t in allCurrentTasks)
        {
            if (t.type.Equals(task.type) && t.target.Equals(task.target))
            {
                if (t.targetSegment != null && t.targetSegment != null && !t.targetSegment.Equals(task.targetSegment))
                {
                    continue;
                }
                foreach (Dorf d in dorves)
                {
                    if (d.currentState.Equals(Dorf.DorfState.IDLE))
                    {
                        assignDorfToTask(d, t);
                        break;
                    }
                }
                return;
            }
        }
        allCurrentTasks.Add(task);
        foreach (Dorf d in dorves)
        {
            if (d.currentState.Equals(Dorf.DorfState.IDLE))
            {
                assignDorfToTask(d, task);
                break;
            }
        }

    }

    void assignDorfToTask(Dorf d, DorfTaskInProgress task)
    {
        d.taskInProgress = task;
        d.currentTask = task.type;
        task.assignedDorves.Add(d);
        Vector2 randomTargetLoc = task.taskLocations[UnityEngine.Random.Range(0, task.taskLocations.Count - 1)];
        d.addWaypoints(randomTargetLoc, task.target);
        d.currentTaskTargetPos = randomTargetLoc;
        d.currentState = Dorf.DorfState.WALKING;
    }

    public class DorfTaskInProgress
    {
        public float completionCtr;
        public float timeForTask;
        public float miscCtr = 0.0f;

        public DorfTask type;
        public Action result;
        public Action runMethod;
        public List<Dorf> assignedDorves = new List<Dorf>();
        public List<Dorf> wereAssigned = new List<Dorf>();
        public List<Vector2> taskLocations = new List<Vector2>();
        public Hex target;
        public Segment targetSegment;
        public Building targetBuilding;
        public Building storageBuilding;
        public List<WorldResource> heldResources;
        public int targetBuildingSlot;

        public int id;
        public int maxDorves = -1;

        public RectTransform progressBar;
        public Canvas taskBarCanvas;
        public float maxTaskBarWidth;

        public DorfTaskInProgress(float timeToComplete, DorfTask type, Action value, List<Vector2> locations, Hex targetHex)
        {
            this.type = type;
            this.timeForTask = timeToComplete;
            result = value;
            taskLocations.AddRange(locations);
            target = targetHex;
            this.taskBarCanvas = targetHex.taskbarCanvas;
            this.progressBar = targetHex.progressBar;
            maxTaskBarWidth = 5;
        }
        public DorfTaskInProgress(float timeToComplete, DorfTask type, Action value, List<Vector2> locations, Segment targetSegment)
        {
            this.type = type;
            this.timeForTask = timeToComplete;
            result = value;
            taskLocations.AddRange(locations);
            this.targetSegment = targetSegment;
            target = targetSegment.parentHex;
            this.taskBarCanvas = targetSegment.taskbarCanvas;
            this.progressBar = targetSegment.progressBar;
            maxTaskBarWidth = 2;
        }
        public DorfTaskInProgress(float timeToComplete, DorfTask type, Action value, Vector2 location, Segment targetSegment)
        {
            this.type = type;
            this.timeForTask = timeToComplete;
            result = value;
            taskLocations.Add(location);
            this.targetSegment = targetSegment;
            target = targetSegment.parentHex;
            this.taskBarCanvas = targetSegment.taskbarCanvas;
            this.progressBar = targetSegment.progressBar;
            maxTaskBarWidth = 2;
        }

        public DorfTaskInProgress(float timeToComplete, DorfTask type, Action value, Vector2 location, Hex targetHex)
        {
            this.type = type;
            this.timeForTask = timeToComplete;
            result = value;
            taskLocations.Add(location);
            target = targetHex;
            this.taskBarCanvas = targetHex.taskbarCanvas;
            this.progressBar = targetHex.progressBar;
            maxTaskBarWidth = 5;

        }

        public DorfTaskInProgress(DorfTask type, Vector2 location, Building targetBuilding, int slot)
        {
            this.type = type;
            this.timeForTask = -1f;
            taskLocations.Add(location);
            this.targetBuilding = targetBuilding;
            this.targetBuildingSlot = slot;
            target = targetBuilding.parentHex;
            if (targetBuilding.isBig)
            {
                this.taskBarCanvas = targetBuilding.parentHex.taskbarCanvas;
                this.progressBar = targetBuilding.parentHex.progressBar;
                maxTaskBarWidth = 5;
            }
            else
            {
                SegmentBuilding thisBuilding = (SegmentBuilding)targetBuilding;
                this.taskBarCanvas = thisBuilding.parentSegment.taskbarCanvas;
                this.progressBar = thisBuilding.parentSegment.progressBar;
                maxTaskBarWidth = 2;
            }
        }

        public DorfTaskInProgress(float timeToComplete, DorfTask type, Vector2 location, Building targetBuilding)
        {
            this.targetBuilding = targetBuilding;
            this.type = type;
            this.timeForTask = timeToComplete;
            taskLocations.Add(location);
            this.target = targetBuilding.parentHex;
            if (targetBuilding.isBig)
            {
                this.taskBarCanvas = targetBuilding.parentHex.taskbarCanvas;
                this.progressBar = targetBuilding.parentHex.progressBar;
                maxTaskBarWidth = 5;
            }
            else
            {
                SegmentBuilding thisBuilding = (SegmentBuilding)targetBuilding;
                this.taskBarCanvas = thisBuilding.parentSegment.taskbarCanvas;
                this.progressBar = thisBuilding.parentSegment.progressBar;
                maxTaskBarWidth = 2;
            }
        }

        public DorfTaskInProgress setMaxDorves(DorfTaskInProgress task, int maxDorves)
        {
            task.maxDorves = maxDorves;
            return task;
        }

        public DorfTaskInProgress setRunMethod(DorfTaskInProgress task, Action value)
        {
            runMethod = value;
            return task;
        }

        public DorfTaskInProgress setResult(DorfTaskInProgress task, Action value)
        {
            result = value;
            return task;
        }
        public DorfTaskInProgress setStorageBuilding(DorfTaskInProgress task, Building build)
        {
            storageBuilding = build;
            return task;
        }

        public void start()
        {
            taskBarCanvas.gameObject.SetActive(true);
            progressBar.sizeDelta = new Vector2(0f, 0.3f);
        }

        public void complete()
        {
            wereAssigned.Clear();
            foreach (Dorf d in assignedDorves)
            {
                wereAssigned.Add(d);
                d.taskInProgress = null;
                d.waypoints.Clear();
                d.currentState = Dorf.DorfState.IDLE;
                d.currentTask = DorfTask.NONE;
            }
            DorfManager.instance.tasksToRemove.Add(this);
            assignedDorves.Clear();

            if (this.taskBarCanvas.gameObject != null)
            {
                this.taskBarCanvas.gameObject.SetActive(false);
            }

            if (result != null) { result.Invoke(); }
            wereAssigned.Clear();
        }

        public void run() {
        if (runMethod != null)
            {
                runMethod.Invoke();
            }
        }
    }
}
