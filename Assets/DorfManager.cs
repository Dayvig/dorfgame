using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static ModelGame;
using static UnityEditor.ObjectChangeEventStream;

public class DorfManager : MonoBehaviour
{
    public static DorfManager instance
    {
        get; private set;
    }

    public List<Dorf> dorves = new List<Dorf>();
    public List<Dorf> dorfGraveyard = new List<Dorf>();

    public List<DorfTaskInProgress> allCurrentTasks = new List<DorfTaskInProgress>();
    public List<string> tasksInProgressDisplay = new List<string>();

    public List<DorfTaskInProgress> taskQueue = new List<DorfTaskInProgress>();
    public List<DorfTaskInProgress> tasksToRemove = new List<DorfTaskInProgress>();

    public List<WorldResource> clutter = new List<WorldResource>();

    public int globalTaskIDs = 0;
    public float taskAssignTimer = 0.0f;
    public float taskAssignDelay = 0.2f;

    public GameObject DorfRef;

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

            //kill dorves
            foreach (Dorf d in dorfGraveyard)
            {
                d.gameObject.SetActive(false);
                dorves.Remove(d);
            }


            //house dorves
            foreach (Building b in ResourceManager.instance.housing)
            {
                BasicHouse house = (BasicHouse)b;
                if (house.owners.Count >= house.capacity) { continue; }
                foreach (Dorf d in dorves)
                {
                    if (d.home == null)
                    {
                        d.home = house;
                    }
                }
            }

            //starving dorves abandon tasks and eat
            for (int i = 0; i < dorves.Count; i++)
            {
                Dorf d = dorves[i];

                if (d.currentFood <= d.maxFood * 0.25f && resourceExists(ResourceManager.ResourceType.FOOD, 1) && !d.modifiers.Contains(Dorf.DorfModifier.STARVING))
                {
                    if (d.taskInProgress != null)
                    {
                        d.taskInProgress.abandon(d);
                    }
                    d.modifiers.Add(Dorf.DorfModifier.STARVING);
                }
                //clear starving from non starving dorves
                else if (d.currentFood > d.maxFood * 0.25f)
                {
                    d.modifiers.Remove(Dorf.DorfModifier.STARVING);
                }
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
            task.run();
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
                        if (task.progressBar != null)
                        {
                            task.progressBar.sizeDelta = new Vector2((task.completionCtr / task.timeForTask) * task.maxTaskBarWidth, 0.3f);
                        }
                    }
                }
            }
            if (!task.type.Equals(DorfTask.WORKBUILDING) && !task.onGoing && task.completionCtr > task.timeForTask)
            {
                task.complete();
            }
        }
        foreach (DorfTaskInProgress t in taskQueue)
        {
            Debug.Log("Adding task " + t.type);
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
            foreach (Building b in ResourceManager.instance.housing)
            {
                house = (BasicHouse)b;
                break;
            }

            //send starving dorves to eat
            for (int j = 0; j < idleDorves.Count; j++)
            {
                if (idleDorves[j].modifiers.Contains(Dorf.DorfModifier.STARVING) && resourceExists(ResourceManager.ResourceType.FOOD, 1))
                {
                    gatherFood(idleDorves[j], false, null);
                    idleDorves.Remove(idleDorves[j]);
                }
            }

            if (idleDorves.Count <= 0) { return; }
            //pickup clutter
            if (tmp.Count != 0)
            {
                foreach (WorldResource clutter in tmp)
                {
                    if (!clutter.toBePickedUp)
                    {
                        pickupAndStoreTask(idleDorves[0], clutter);
                        idleDorves.Remove(idleDorves[0]);
                    }
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
                    if (d.currentFood <= d.maxFood * 0.75f && d.fullness >= d.mealInterval && resourceExists(ResourceManager.ResourceType.FOOD, 1))
                    {
                        Debug.Log("Sending Dorves to eat");
                        gatherFood(d, true, d.home == null ? house : d.home);
                        idleDorves.Remove(d);
                    }
                }

                //brew beer
                for (int i = 0; i < idleDorves.Count; i++)
                {
                    foreach (Building b in ResourceManager.instance.activatableBuildings)
                    {
                        if (b.canBeActivated())
                        {
                            b.activate();
                        }
                    }
                }

            }
            //reproduce
            for (int i = 0; i < idleDorves.Count; i++)
            {
                Dorf d = idleDorves[i];
                Dorf d2 = null;
                if (d.currentFood > d.maxFood * 0.75f && d.horniness >= d.sexInterval)
                {
                    //find next dorf that meets requirements
                    foreach (Dorf test in idleDorves)
                    {
                        if (test.currentFood > test.maxFood * 0.75f && test.horniness >= test.sexInterval && !test.Equals(d))
                        {
                            if (test.spouse == null)
                            {
                                d2 = test;
                                break;
                            }
                            else if (test.spouse.Equals(d))
                            {
                                d2 = test;
                                break;
                            }
                        }
                    }
                    if (d2 != null)
                    {
                        if (reproduce(d, d2))
                        {
                            idleDorves.Remove(d);
                            idleDorves.Remove(d2);
                        }
                    }
                }
            }
            return;
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

    void pickupAndStoreTask(Dorf assignee, WorldResource r)
    {
        if (!hasValidStorageBuilding(r.type, r.value)) { return; }

        moveToAndPickupResource(assignee, r, r.type, r.value, false, () =>
        {
            if (assignee.currentHaul <= assignee.carryingCapacity)
            {
                foreach (WorldResource w in clutter)
                {
                    if (w.type.Equals(r.type) && assignee.currentHaul + w.weight <= assignee.carryingCapacity && !w.toBePickedUp)
                    {
                        pickupAndStoreTask(assignee, w);
                        return;
                    }
                }
            }
            dropOffResource(assignee, r, true, () => { });
        });
    }

    void createNewStorageTask(Dorf targetDorf)
    {
        Debug.Log("New Storage Task Created");

        if (targetDorf.heldResources[0] == null)
        {
            return;
        }
        if (!hasValidStorageBuilding(targetDorf.heldResources[0].type, targetDorf.heldResources[0].value))
        {
            dropAllResources(targetDorf);
            return;
        }
        DorfManager.DorfTaskInProgress newTask;
        Building close = closestStorageBuilding(targetDorf.transform.position, targetDorf.heldResources[0].type, targetDorf.heldResources[0].value, false, true);

        if (close == null) { return; }
        newTask = new DorfManager.DorfTaskInProgress(0.1f, DorfTask.HAUL, close.transform.position, close);
        newTask = newTask.setMaxDorves(newTask, 1).setResult(newTask, () =>
        {
            if (targetDorf.heldResources.Count == 0)
            {
                Debug.Log("Something went wrong - no resources to deposit");
                return;
            }
            foreach (Building.StorageSlot s in newTask.targetBuilding.storage)
            {
                if (s.type == targetDorf.heldResources[0].type)
                {
                    foreach (WorldResource w in targetDorf.heldResources)
                    {
                        if (targetDorf.storeWorldResource(w, s))
                        {
                            ResourceManager.instance.toBeDestroyed.Add(w);
                        };
                    }
                }
            }
            dropAllResources(targetDorf);
            UIManager.instance.updateCounterDisplay();
        });
        assignDorfToTask(targetDorf, newTask);
        DorfManager.instance.taskQueue.Add(newTask);

    }

    public Building closestStorageBuilding(Vector2 position, ResourceManager.ResourceType resource, float value, bool exactAmount, bool dropOff)
    {
        Building closest = null;
        float least = -1;
        foreach (Building b in ResourceManager.instance.storageBuildings)
        {
            foreach (Building.StorageSlot s in b.storage)
            {
                if (s.type.Equals(resource) && ((dropOff && s.occupiedStorage + value <= s.maxStorage) || (!dropOff && exactAmount && s.occupiedStorage >= value) || (!dropOff && !exactAmount && s.occupiedStorage > 0)))
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

    public bool hasValidStorageBuilding(ResourceManager.ResourceType resource, float value)
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

    public bool resourceExists(ResourceManager.ResourceType type, float amount)
    {
        //if food exists
        bool resExists = false;
        int total = 0;
        foreach (WorldResource w in clutter)
        {
            if (w.type == type)
            {
                total += (int)w.value;
                if (total > amount) { resExists = true; break; }
            }
        }
        foreach (Building b in ResourceManager.instance.storageBuildings)
        {
            foreach (Building.StorageSlot s in b.storage)
            {
                if (s.type == type && s.occupiedStorage != 0)
                {
                    resExists = true;
                    break;
                }
            }
            if (resExists) { break; }
        }
        return resExists;
    }

    public (bool, Building, Building.StorageSlot) moveToResource(Dorf d, WorldResource r, ResourceManager.ResourceType type, float amount, bool exactAmount, Action nextStep)
    {
        DorfTaskInProgress thisTask;
        if (r == null)
        {
            r = closestResourceToPickup(d.gameObject.transform.position, type, false);
        }
        if (r == null)
        {
            Building close = closestStorageBuilding(d.gameObject.transform.position, type, amount, false, false);
            if (close != null)
            {
                Building.StorageSlot slot = null;
                foreach (Building.StorageSlot s in close.storage)
                {
                    if (s.type == type)
                    {
                        slot = s;
                        break;
                    }
                }
                thisTask = new DorfManager.DorfTaskInProgress(0.1f, DorfTask.HAUL,
                nextStep,
                close.gameObject.transform.position, close.parentHex);
                thisTask = thisTask.setMaxDorves(thisTask, 1);
                assignDorfToTask(d, thisTask);
                DorfManager.instance.taskQueue.Add(thisTask);
                return (true, close, slot);
            }
        }
        else
        {
            thisTask = new DorfManager.DorfTaskInProgress(0.1f, DorfTask.HAUL,
            nextStep,
            r.gameObject.transform.position, r.thisHex);
            thisTask = thisTask.setMaxDorves(thisTask, 1);
            assignDorfToTask(d, thisTask);
            DorfManager.instance.taskQueue.Add(thisTask);
            return (true, null, null);
        }
        return (false, null, null);
    }

    public bool moveToBuilding(Dorf d, Building b, Action nextStep)
    {
        DorfTaskInProgress thisTask;
        if (b == null) { return false; }
        thisTask = new DorfManager.DorfTaskInProgress(0.1f, DorfTask.HAUL,
        nextStep,
        b.gameObject.transform.position, b.parentHex);
        thisTask = thisTask.setMaxDorves(thisTask, 1);
        assignDorfToTask(d, thisTask);
        DorfManager.instance.taskQueue.Add(thisTask);
        return true;
    }

    public bool dropOffResource(Dorf d, WorldResource worldRes, bool all, Action nextStep)
    {
        Building close = closestStorageBuilding(d.gameObject.transform.position, worldRes.type, worldRes.value, false, true);
        DorfTaskInProgress thisTask;
        if (close == null) { dropAllResources(d);  return false; }

        thisTask = new DorfManager.DorfTaskInProgress(0.1f, DorfTask.HAUL,
        () =>
        {
            Building.StorageSlot tmp = null;

            if (all)
            {
                List<WorldResource> toBeStored = new List<WorldResource>();
                List<WorldResource> toBeDropped = new List<WorldResource>();

                foreach (WorldResource res in d.heldResources)
                {
                    foreach (Building.StorageSlot s in close.storage)
                    {
                        if (s.type == res.type)
                        {
                            if (s.occupiedStorage + res.value <= s.maxStorage)
                            {
                                toBeStored.Add(res);
                                tmp = s;
                                break;
                            }
                            else
                            {
                                toBeDropped.Add(res);
                                break;
                            }
                        }
                    }
                }
                if (tmp == null) { Debug.Log("Attempted to Store into incorrect building"); return; }
                foreach (WorldResource toStore in toBeStored)
                {
                    storeResource(d, toStore, close, tmp);
                }
                foreach (WorldResource toDrop in toBeDropped)
                {
                    drop(d, toDrop);
                }
            }
            else
            {
                foreach (Building.StorageSlot s in close.storage)
                {
                    if (s.type == worldRes.type)
                    {
                        if (s.occupiedStorage + worldRes.value <= s.maxStorage)
                        {
                            Debug.Log("Storing");
                            storeResource(d, worldRes, close, s);
                            break;
                        }
                        else
                        {
                            drop(d, worldRes);
                            break;
                        }
                    }
                }
            }
        }
        + nextStep,
        close.gameObject.transform.position, close.parentHex);
        thisTask = thisTask.setMaxDorves(thisTask, 1);
        assignDorfToTask(d, thisTask);
        DorfManager.instance.taskQueue.Add(thisTask);
        return true;
    }

    public bool bringResourceToConstructionSite(Dorf d, WorldResource worldRes, ResourceManager.ResourceType type, float amount, Building constructionSite, Action nextStep)
    {
        Debug.Log("Bringing Resources to Site");
        bool finalSuccess = false;

        moveToAndPickupResource(d, worldRes, type, amount, true, constructionSite, () =>
        {
            bool success = false;
            success = moveToBuilding(d, constructionSite, () =>
            {
                finalSuccess = success;
                if (!success) { return; };
                finalSuccess = addResourcesToConstruction(d, constructionSite);
                nextStep();
            });
        });
        return finalSuccess;
    }

    public bool addResourcesToConstruction(Dorf d, Building constructionSite)
    {
        List<WorldResource> toRemove = new List<WorldResource>();
        foreach (WorldResource w in d.heldResources)
        {
            foreach (Building.BuildingCost gathered in constructionSite.gatheredBuildingResources)
            {
                if (gathered.type == w.type)
                {
                    gathered.numericalCost += (int)w.value;
                    ResourceManager.instance.toBeDestroyed.Add(w);
                    toRemove.Add(w);
                }
            }
            if (constructionSite.ResourcesInTransit.Contains(w))
            {
                constructionSite.ResourcesInTransit.Remove(w);
            }
        }
        foreach (WorldResource res in toRemove)
        {
            drop(d, res);
        }
        return true;
    }

    public void moveToAndPickupResource(Dorf d, WorldResource worldRes, ResourceManager.ResourceType type, float amount, bool exactAmount, Action nextStep)
    {
        (bool, Building, Building.StorageSlot) result = (false, null, null);
        if (worldRes != null) { worldRes.toBePickedUp = true;
            d.resourcesToPickUp.Add(worldRes);
        }
        result = moveToResource(d, worldRes, type, amount, exactAmount, () =>
        {
            if (result.Item1 == false) { return; }
            else if (result.Item2 == null) { pickupResource(d, worldRes, type, amount, exactAmount, null, null); }
            else { pickupResource(d, worldRes, type, amount, exactAmount, result.Item2, result.Item3); }
            nextStep();
        });
    }

    public void moveToAndPickupResource(Dorf d, WorldResource worldRes, ResourceManager.ResourceType type, float amount, bool exactAmount, Building constructionTarget, Action nextStep)
    {
        WorldResource tmp = null;

        (bool, Building, Building.StorageSlot) result = (false, null, null);
        if (worldRes != null) { worldRes.toBePickedUp = true; }
        result = moveToResource(d, worldRes, type, amount, exactAmount, () =>
        {
            if (result.Item1 == false) { return; }
            else if (result.Item2 == null) {
                tmp = pickupResource(d, worldRes, type, amount, exactAmount, null, null);
                constructionTarget.ResourcesInTransit.Add(tmp);
            }
            else { tmp = pickupResource(d, worldRes, type, amount, exactAmount, result.Item2, result.Item3);
                constructionTarget.ResourcesInTransit.Add(tmp);
            }
            nextStep();
        });
    }

    public WorldResource pickupResource(Dorf d, WorldResource worldRes, ResourceManager.ResourceType type, float amount, bool exactAmount, Building storage, Building.StorageSlot slot)
    {
        if (worldRes == null)
        {
            if (storage == null) { return null; }
            else
            {
                WorldResource newResource = ResourceManager.instance.createNewWorldResource(storage.parentHex, type, storage.gameObject.transform.position, 1.0f, false);

                float targetValue = amount;
                float valueToWeight = newResource.value / newResource.weight;

                if (targetValue > slot.occupiedStorage)
                {
                    if (exactAmount) { return null; }
                    targetValue = slot.occupiedStorage;
                }
                if ((targetValue * valueToWeight) + d.currentHaul > d.carryingCapacity)
                {
                    newResource.weight = d.carryingCapacity - d.currentHaul;
                    targetValue = valueToWeight * newResource.weight;
                }

                newResource.value = targetValue;
                newResource.weight = targetValue * (1 / valueToWeight);
                slot.occupiedStorage -= newResource.value;
                UIManager.instance.updateCounterDisplay();
                d.pickupWorldResource(newResource);
                return newResource;
            }
        }
        else
        {
            d.pickupWorldResource(worldRes);
            UIManager.instance.updateCounterDisplay();
            return worldRes;
        }
    }
    public bool storeResource(Dorf d, WorldResource worldRes, Building storage, Building.StorageSlot slot)
    {
        if (storage == null) { return false; }
        else
        {
            if (d.storeWorldResource(worldRes, slot))
            {
                Debug.Log("Deleting Resource");
                drop(d, worldRes);
                ResourceManager.instance.toBeDestroyed.Add(worldRes);
            };
            return true;
        }
    }

    public void gatherFood(Dorf hungry, bool comfortable, Building home)
    {
        if (!resourceExists(ResourceManager.ResourceType.FOOD, 1) || hungry.currentHaul >= hungry.carryingCapacity)
        {
            if (comfortable && home != null)
            {
                moveToBuilding(hungry, home, () =>
                {
                    eatFood(hungry, home.parentHex);
                });
            }
            else
            {
                eatFood(hungry, HexManager.instance.closestHexToLoc(hungry.transform.position));
            }
            return;
        }
        moveToAndPickupResource(hungry, null, ResourceManager.ResourceType.FOOD, hungry.carryingCapacity - hungry.currentHaul, false, () =>
        {
            if (!comfortable)
            {
                eatFood(hungry, HexManager.instance.closestHexToLoc(hungry.transform.position));
            }
            else
            {
                gatherFood(hungry, comfortable, home);
            }
        });
        
    }

    public void setConstructionSite(Building toConstruct)
    {
        DorfTaskInProgress thisTask;

        thisTask = new DorfManager.DorfTaskInProgress(0.1f, DorfTask.NONE,
        () => { });
        thisTask = thisTask.setRunMethod(thisTask, () =>
        {
            if (toConstruct.isActive)
            {
                thisTask.complete();
            }
            List<Dorf> toRemove = new List<Dorf>();
            foreach (Dorf d in thisTask.assignedDorves)
            {

                bool holdingNeededResource = false;
                foreach (WorldResource w in toConstruct.ResourcesInTransit)
                {
                    if (d.heldResources.Contains(w))
                    {
                        Debug.Log(d.name + "Has needed resource");
                        holdingNeededResource = true;
                        break;
                    }
                } 
                if (hasEnoughMaterials(toConstruct) && !holdingNeededResource && toConstruct.ResourcesInTransit.Count != 0)
                {
                    toRemove.Add(d);
                }
            }
            foreach (Dorf d in toRemove) {
                Debug.Log(d.name + "Dropping task");
                dropAllResources(d);
                d.taskInProgress.remove();
                thisTask.assignedDorves.Remove(d);
            }
        }).setOnAssignMethod(thisTask, (d) =>
        {
            constructionTask(d, toConstruct);
        });
        thisTask.onGoing = true;

        Debug.Log("Setting construction site: " + toConstruct.name);
        DorfManager.instance.taskQueue.Add(thisTask);
    }

    public bool constructionTask(Dorf d, Building toConstruct)
    {
        if (toConstruct.costs.Count == 0 || hasEnoughMaterials(toConstruct))
        {
            if (toConstruct.isActive) { return false; }
            Debug.Log("Constructing Building");
            bool success = false;
            success = moveToBuilding(d, toConstruct, () =>
            {
                if (!success) { return; };
                constructBuilding(d, toConstruct);
            });
            return success;
        }

        foreach (Building.BuildingCost cost in toConstruct.costs)
        {
            foreach (Building.BuildingCost gathered in toConstruct.gatheredBuildingResources)
            {
                if (gathered.type.Equals(cost.type) && gathered.numericalCost < cost.numericalCost)
                {
                    int diff = cost.numericalCost - gathered.numericalCost;
                    foreach (WorldResource carried in toConstruct.ResourcesInTransit)
                    {
                        if (carried.type.Equals(cost.type))
                        {
                            diff -= (int)carried.value;
                        }
                    }
                    if (diff > 0 && resourceExists(cost.type, diff))
                    {
                        bringResourceToConstructionSite(d, null, cost.type, diff, toConstruct, () =>
                        {
                        if (hasEnoughMaterials(toConstruct))
                        {
                            constructBuilding(d, toConstruct);
                        }
                        else
                        {
                            constructionTask(d, toConstruct);
                        }
                        });
                     };
                        return true;
                    }
                }
            }
        return false;
    }

    public DorfTaskInProgress newWaitTask(Dorf d, Action run)
    {
        DorfTaskInProgress thisTask;

        thisTask = new DorfManager.DorfTaskInProgress(0.1f, DorfTask.NONE,
        () => { });
        thisTask = thisTask.setRunMethod(thisTask, () =>
        {
            run();
        }).setMaxDorves(thisTask, 1);
        thisTask.onGoing = true;
        assignDorfToTask(d, thisTask);
        DorfManager.instance.taskQueue.Add(thisTask);
        return thisTask;
    }

    public void dropAllResources(Dorf d)
    {
        foreach (WorldResource w in d.heldResources)
        {
            w.toBePickedUp = false;
        }
        foreach (WorldResource w in d.resourcesToPickUp)
        {
            w.toBePickedUp = false;
        }
        d.resourcesToPickUp.Clear();
        d.heldResources.Clear();
        d.currentHaul = 0;
    }

    public void drop(Dorf d, WorldResource r)
    {
        foreach (WorldResource w in d.heldResources)
        {
            if (w.Equals(r))
            {
                w.toBePickedUp = false;
            }
        }
        if (d.resourcesToPickUp.Contains(r))
        {
            d.resourcesToPickUp.Remove(r);
        }
        d.heldResources.Remove(r);
        d.currentHaul -= r.weight;
    }

    public bool hasEnoughMaterials(Building toConstruct)
    {
        bool allReqMet = true;
        if (toConstruct.costs.Count == 0)
        {
            return true;
        }
        foreach (Building.BuildingCost tmp in toConstruct.gatheredBuildingResources)
        {
            foreach (Building.BuildingCost cost in toConstruct.costs)
            {
                if (cost.type == tmp.type)
                {
                    int projected = tmp.numericalCost;
                    foreach (WorldResource w in toConstruct.ResourcesInTransit)
                    {
                        if (w.type.Equals(tmp.type))
                        {
                            projected += (int)w.value;
                        }
                    }

                    if (!(projected >= cost.numericalCost))
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
        thisTask = thisTask.setMaxDorves(thisTask, 4).setResult(thisTask, () =>
        {
            constructBuilding(builder, toConstruct);
        });
        assignDorfToTask(builder, thisTask);
        DorfManager.instance.taskQueue.Add(thisTask);
    }

    public void constructBuilding(Dorf builder, Building toConstruct)
    {
        Debug.Log("Constructing Building");

        DorfManager.DorfTaskInProgress thisTask;
        foreach (WorldResource r in builder.heldResources)
        {
            ResourceManager.instance.toBeDestroyed.Add(r);
            if (r.isClutter)
            {
                clutter.Remove(r);
                ResourceManager.instance.stowResource(r.type, (int)r.value);
            }
        }
        dropAllResources(builder);
        UIManager.instance.updateCounterDisplay();
        Debug.Log("Setting Final Construction");

        thisTask = new DorfManager.DorfTaskInProgress(toConstruct.constructionTime, DorfTask.BUILD,
        () => { },
        toConstruct.gameObject.transform.position, toConstruct.parentHex);
        thisTask = thisTask.setMaxDorves(thisTask, 4).setResult(thisTask, () =>
        {
            Debug.Log("Finalizing Construction");
            if (toConstruct.isActive) { return; }
            toConstruct.onPlace(builder);
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
        assignDorfToTask(builder, thisTask);
        DorfManager.instance.taskQueue.Add(thisTask);
    }

    public void eatFood(Dorf hungry, Hex currentHex)
    {
        float manureValue = 0.0f;
        foreach (WorldResource r in hungry.heldResources)
        {
            hungry.currentFood += r.value;
            manureValue += r.value;
            ResourceManager.instance.toBeDestroyed.Add(r);
            ResourceManager.instance.consumeResource(ResourceManager.ResourceType.FOOD, (int)r.value, r.isClutter);
            if (r.isClutter)
            {
                clutter.Remove(r);
            }
            hungry.fullness = 0.0f;
        }
        dropAllResources(hungry);
        while (manureValue > 0)
        {
            WorldResource newManure = ResourceManager.instance.createNewWorldResource(currentHex, ResourceManager.ResourceType.MANURE, hungry.gameObject.transform.position, 0.3f, true);
            newManure.value = manureValue > 20 ? 20 : manureValue;
            newManure.weight = newManure.value / 2;
            manureValue -= newManure.value;
            ResourceManager.instance.addResource(ResourceManager.ResourceType.MANURE, (int)newManure.value, true);
        }
        UIManager.instance.updateCounterDisplay();
    }

    public bool reproduce(Dorf primary, Dorf secondary)
    {
        Building targetBuilding = primary.home != null ? primary.home : secondary.home != null ? secondary.home : null;
        DorfManager.DorfTaskInProgress thisTask;
        Vector2 randcircle = UnityEngine.Random.insideUnitCircle.normalized * 0.2f;

        if (targetBuilding == null) { return false; }

        if (targetBuilding.isBig)
        {
            thisTask = new DorfManager.DorfTaskInProgress(10.0f, DorfTask.REPRODUCE,
            () => { },
            targetBuilding.gameObject.transform.position + (Vector3)randcircle, targetBuilding.parentHex);
            thisTask = thisTask.setMaxDorves(thisTask, 2).setResult(thisTask, () =>
            {
                GameObject newDorf = Instantiate(DorfRef, targetBuilding.gameObject.transform.position, Quaternion.identity);
                Dorf newDorfScript = newDorf.GetComponent<Dorf>();
                newDorfScript.init();
                foreach (Dorf d in thisTask.wereAssigned)
                {
                    d.horniness = 0.0f;
                }
            });
        }
        else
        {
            SegmentBuilding seg = targetBuilding as SegmentBuilding;
            thisTask = new DorfManager.DorfTaskInProgress(10.0f, DorfTask.REPRODUCE,
            () => { },
            seg.gameObject.transform.position + (Vector3)randcircle, seg.parentSegment);
            thisTask = thisTask.setMaxDorves(thisTask, 2).setResult(thisTask, () =>
            {
                GameObject newDorf = Instantiate(DorfRef, targetBuilding.gameObject.transform.position, Quaternion.identity);
                Dorf newDorfScript = newDorf.GetComponent<Dorf>();
                newDorfScript.init();
                foreach (Dorf d in thisTask.wereAssigned)
                {
                    d.horniness = 0.0f;
                }
                primary.spouse = secondary;
                secondary.spouse = primary;
            });
        }
        assignDorfToTask(primary, thisTask);

        thisTask.taskLocations.Clear();
        thisTask.taskLocations.Add(targetBuilding.gameObject.transform.position - (Vector3)randcircle);

        assignDorfToTask(secondary, thisTask);
        DorfManager.instance.taskQueue.Add(thisTask);
        return true;

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

    public void assignDorfToTask(Dorf d, DorfTaskInProgress task)
    {

        if (task.assignedDorves.Count == 0)
        {
            task.start();
        }

        d.taskInProgress = task;
        d.currentTask = task.type;
        task.assignedDorves.Add(d);
        if (!task.doesNotRequireLocation)
        {
            Vector2 randomTargetLoc = task.taskLocations[UnityEngine.Random.Range(0, task.taskLocations.Count - 1)];
            d.addWaypoints(randomTargetLoc, task.target);
            d.currentTaskTargetPos = randomTargetLoc;
        }
        else
        {
            d.currentTaskTargetPos = d.transform.position;
        }
        d.currentState = Dorf.DorfState.WALKING;
        Debug.Log("Assigning Dorf to Task " + task.type);

        task.onAssign(d);
    }

    public class DorfTaskInProgress
    {
        public float completionCtr;
        public float timeForTask;
        public float miscCtr = 0.0f;

        public DorfTask type;
        public Action result;
        public Action runMethod;
        public Action<Dorf> assignMethod;
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
        public bool doesNotRequireLocation;
        public bool onGoing;

        public DorfTaskInProgress(float timeToComplete, DorfTask type, Action value, List<Vector2> locations, Hex targetHex)
        {
            this.type = type;
            this.timeForTask = timeToComplete;
            result = value;
            taskLocations.AddRange(locations);
            target = targetHex;
            setTaskBar(true);
        }
        public DorfTaskInProgress(float timeToComplete, DorfTask type, Action value, List<Vector2> locations, Segment targetSegment)
        {
            this.type = type;
            this.timeForTask = timeToComplete;
            result = value;
            taskLocations.AddRange(locations);
            this.targetSegment = targetSegment;
            target = targetSegment.parentHex;
            setTaskBar(false);
        }
        public DorfTaskInProgress(float timeToComplete, DorfTask type, Action value, Vector2 location, Segment targetSegment)
        {
            this.type = type;
            this.timeForTask = timeToComplete;
            result = value;
            taskLocations.Add(location);
            this.targetSegment = targetSegment;
            target = targetSegment.parentHex;
            setTaskBar(false);
        }

        public DorfTaskInProgress(float timeToComplete, DorfTask type, Action value, Vector2 location, Hex targetHex)
        {
            this.type = type;
            this.timeForTask = timeToComplete;
            result = value;
            taskLocations.Add(location);
            target = targetHex;
            setTaskBar(true);
        }

        public DorfTaskInProgress(DorfTask type, Vector2 location, Building targetBuilding, int slot)
        {
            this.type = type;
            this.timeForTask = -1f;
            taskLocations.Add(location);
            this.targetBuilding = targetBuilding;
            this.targetBuildingSlot = slot;
            target = targetBuilding.parentHex;
            setTaskBar(targetBuilding.isBig);
        }

        public DorfTaskInProgress(float timeToComplete, DorfTask type, Vector2 location, Building targetBuilding)
        {
            this.targetBuilding = targetBuilding;
            this.type = type;
            this.timeForTask = timeToComplete;
            taskLocations.Add(location);
            this.target = targetBuilding.parentHex;
            if (!targetBuilding.isBig)
            {
                SegmentBuilding seg = (SegmentBuilding)targetBuilding;
                targetSegment = seg.parentSegment;
            }
            setTaskBar(targetBuilding.isBig);
        }
        public DorfTaskInProgress(float timeToComplete, DorfTask type, Action value)
        {
            this.type = type;
            this.timeForTask = timeToComplete;
            result = value;
            doesNotRequireLocation = true;
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

        public DorfTaskInProgress setOnAssignMethod(DorfTaskInProgress task, Action<Dorf> value)
        {
            assignMethod = value;
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

        public void setTaskBar(bool isBig)
        {
            if (isBig)
            {
                taskBarCanvas = target.taskbarCanvas;
                progressBar = target.progressBar;
                maxTaskBarWidth = 5;
            }
            else
            {
                taskBarCanvas = targetSegment.taskbarCanvas;
                progressBar = targetSegment.progressBar;
                maxTaskBarWidth = 2;
            }

        }

        public void start()
        {
            if (taskBarCanvas != null)
            {
                taskBarCanvas.gameObject.SetActive(true);
                progressBar.sizeDelta = new Vector2(0f, 0.3f);
            }
        }

        public void onAssign(Dorf d)
        {
            if (assignMethod != null)
            {
                assignMethod(d);
            }
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
                d.targetBuilding = null;
            }
            DorfManager.instance.tasksToRemove.Add(this);
            assignedDorves.Clear();

            if (this.taskBarCanvas != null)
            {
                this.taskBarCanvas.gameObject.SetActive(false);
            }

            if (result != null) { result.Invoke(); }
            wereAssigned.Clear();
        }

        public void remove()
        {
            wereAssigned.Clear();
            foreach (Dorf d in assignedDorves)
            {
                wereAssigned.Add(d);
                d.taskInProgress = null;
                d.waypoints.Clear();
                d.currentState = Dorf.DorfState.IDLE;
                d.currentTask = DorfTask.NONE;
                d.targetBuilding = null;
                DorfManager.instance.dropAllResources(d);
            }
            DorfManager.instance.tasksToRemove.Add(this);
            assignedDorves.Clear();

            if (this.taskBarCanvas != null)
            {
                this.taskBarCanvas.gameObject.SetActive(false);
            }
        }

        public void abandon(Dorf d)
        {
            d.taskInProgress.assignedDorves.Remove(d);
            d.taskInProgress = null;
            d.waypoints.Clear();
            d.currentState = Dorf.DorfState.IDLE;
            d.currentTask = DorfTask.NONE;
            DorfManager.instance.dropAllResources(d);
        }

        public void run()
        {
            if (runMethod != null)
            {
                runMethod.Invoke();
            }
        }
    }
}
