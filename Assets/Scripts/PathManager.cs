using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    PathRequest currentPathRequest;

    private static PathManager instance;
    private PathfindingWithHeap pathfinder;

    private bool isProcessingPath;

    private void Awake()
    {
        instance = this;
        pathfinder = GetComponent<PathfindingWithHeap>();
    }


    public static void RequestPath(Vector3 pathStart,Vector3 pathEnd,Action<Vector3[],bool> callback)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
        instance.pathRequestQueue.Enqueue(newRequest);
        instance.ProcessNextPath();
    }

    void ProcessNextPath()
    {
        if(!isProcessingPath && pathRequestQueue.Count>0)
        {
            currentPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;
            //pathfinder.BeginFindingPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
        }
    }

    public void FinishedProcessingPath(Vector3[] path,bool success)
    {
        currentPathRequest.callback(path, success);
        isProcessingPath = false;
        ProcessNextPath();
    }
}

public class PathRequest
{
    public Vector3 pathStart;
    public Vector3 pathEnd;
    public Action<Vector3[], bool> callback;

    public PathRequest(Vector3 _start,Vector3 _end,Action<Vector3[],bool> _callback)
    {
        pathStart = _start;
        pathEnd = _end;
        callback = _callback;
    }
}
