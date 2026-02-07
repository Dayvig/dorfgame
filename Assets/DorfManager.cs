using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;
using static ModelGame;
using static UnityEngine.GraphicsBuffer;

public class DorfManager : MonoBehaviour
{
    public static DorfManager instance
    {
        get; private set;
    }

    public List<Dorf> dorves = new List<Dorf>();
    public List<DorfTaskInProgress> allCurrentTasks = new List<DorfTaskInProgress>();
    public List<DorfTaskInProgress> tasksToRemove = new List<DorfTaskInProgress>();

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
            distributeDorves();
            taskAssignTimer -= taskAssignDelay;
        }


        foreach (DorfTaskInProgress task in allCurrentTasks)
        {
            task.target.taskbarCanvas.gameObject.SetActive(true);
            task.target.progressBar.sizeDelta = new Vector2((task.completionCtr / task.timeForTask) * 5f, 0.3f);

            foreach (Dorf d in task.assignedDorves)
            {
                if (d.currentState == Dorf.DorfState.PERFORMINGTASK)
                {
                    task.completionCtr += Time.deltaTime * d.workRate;
                }
            }
            if (task.completionCtr > task.timeForTask)
            {
                task.complete();
            }
        }

        foreach (DorfTaskInProgress t in tasksToRemove)
        {
            allCurrentTasks.Remove(t);
        }
    }

    void distributeDorves()
    {
        //figure out how many dorves are idle
        List<Dorf> idleDorves =  new List<Dorf>();
        foreach (Dorf d in dorves)
        {
            if (d.currentState.Equals(Dorf.DorfState.IDLE))
            {
                idleDorves.Add(d);
            }
        }

        //assigns all idle dorves to tasks
        while (idleDorves.Count > 0)
        {
            //find task with least dorves assigned
            int least = -1;
            DorfTaskInProgress targetTask = null;
            foreach (DorfTaskInProgress task in allCurrentTasks)
            {
                if (least == -1 || task.assignedDorves.Count < least)
                {
                    least = task.assignedDorves.Count;
                    targetTask = task;
                }
            }
            //assign dorf to task
            if (targetTask != null)
            {
                if (targetTask.assignedDorves.Count == 0)
                {
                    targetTask.target.taskbarCanvas.gameObject.SetActive(true);
                    targetTask.target.progressBar.sizeDelta = new Vector2(0f, 0.3f);
                }
                assignDorfToTask(idleDorves[0], targetTask);
            }
            idleDorves.Remove(idleDorves[0]);
        }
    }

        public void assignTask(DorfTaskInProgress task)
    {
        task.target.taskbarCanvas.gameObject.SetActive(true);
        task.target.progressBar.sizeDelta = new Vector2(0f, 0.3f);
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

        public DorfTask type;
        public Action result;
        public List<Dorf> assignedDorves = new List<Dorf>();
        public List<Vector2> taskLocations = new List<Vector2>();
        public Hex target;
        public Segment targetSegment;

        public int id;

        public DorfTaskInProgress(float timeToComplete, DorfTask type, Action value, List<Vector2> locations, Hex targetHex)
        {
            this.type = type;
            this.timeForTask = timeToComplete;
            result = value;
            taskLocations.AddRange(locations);
            target = targetHex;
        }
        public DorfTaskInProgress(float timeToComplete, DorfTask type, Action value, List<Vector2> locations, Segment targetSegment)
        {
            this.type = type;
            this.timeForTask = timeToComplete;
            result = value;
            taskLocations.AddRange(locations);
            this.targetSegment = targetSegment;
            target = targetSegment.parentHex;
        }
        public DorfTaskInProgress(float timeToComplete, DorfTask type, Action value, Vector2 location, Segment targetSegment)
        {
            this.type = type;
            this.timeForTask = timeToComplete;
            result = value;
            taskLocations.Add(location);
            this.targetSegment = targetSegment;
            target = targetSegment.parentHex;
        }
        public void complete()
        {
            foreach (Dorf d in assignedDorves)
            {
                d.taskInProgress = null;
                d.waypoints.Clear();
                d.currentState = Dorf.DorfState.IDLE;
            }
            DorfManager.instance.tasksToRemove.Add(this);
            assignedDorves.Clear();
            target.taskbarCanvas.gameObject.SetActive(false);
            result.Invoke();
        }
    }
}
