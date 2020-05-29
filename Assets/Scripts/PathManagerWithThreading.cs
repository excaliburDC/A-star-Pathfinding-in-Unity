using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PathManagerWithThreading : MonoBehaviour
{
    Queue<PathResult> results = new Queue<PathResult>();

    private static PathManagerWithThreading m_instance;
    private PathfindingWithHeap pathfinding;

    private bool isProcessingPath;

    private void Awake()
    {
        m_instance = this;
        pathfinding = GetComponent<PathfindingWithHeap>();
    }


    private void Update()
    {
        if(results.Count>0)
        {
            int itemsInQueue = results.Count;
            lock(results)
            {
                for (int i = 0; i < itemsInQueue; i++)
                {
                    PathResult pathResult = results.Dequeue();
                    pathResult.callback(pathResult.path, pathResult.success);
                }
            }
        }
    }

    public static void RequestPath(PathRequestForThreading request)
    {
        ThreadStart threadStart = delegate { m_instance.pathfinding.FindPath(request, m_instance.FinishedProcessingPath); };

        threadStart.Invoke();
    }

    public void FinishedProcessingPath(PathResult result)
    {

        lock(results)
        {
            results.Enqueue(result);
        }
    }


}


public class PathResult
{
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool> callback;

    public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
    {
        this.path = path;
        this.success = success;
        this.callback = callback;
    }
}

public class PathRequestForThreading
{
    public Vector3 pathStart;
    public Vector3 pathEnd;
    public Action<Vector3[], bool> callback;

    public PathRequestForThreading(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback)
    {
        pathStart = _start;
        pathEnd = _end;
        callback = _callback;
    }
}

