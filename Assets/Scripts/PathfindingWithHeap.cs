using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

//Faster method for pathfinding(Uses Heap Optimization)
public class PathfindingWithHeap : MonoBehaviour
{
    public Transform startPos, targetPos;

    private Grid grid;

    //private PathManager pathManager;



    private void Awake()
    {
        grid = GetComponent<Grid>();
        //pathManager = GetComponent<PathManager>();

      
    }


    #region Pathfinding Without using Threads
    //public void BeginFindingPath(Vector3 startPos,Vector3 targetPos)
    //{
    //    StartCoroutine(FindPath(startPos, targetPos));
    //}

    //IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    //{
    //    Stopwatch sw = new Stopwatch();
    //    sw.Start();

    //    Vector3[] wayPoints = new Vector3[0];
    //    bool pathSuccess = false;

    //    Node startNode = grid.NodeFromWorldPoint(startPos);
    //    Node targetNode = grid.NodeFromWorldPoint(targetPos);

    //    if(startNode.walkable && targetNode.walkable)
    //    {
    //        Heap<Node> openSet = new Heap<Node>(grid.GetMaxSize);  //set of nodes to be evaluated
    //        HashSet<Node> closedSet = new HashSet<Node>();   //set of nodes already evaluated

    //        openSet.Add(startNode);

    //        while (openSet.Count > 0)
    //        {
    //            //Fastest and optimised approach
    //            Node currentNode = openSet.RemoveFirst();

    //            closedSet.Add(currentNode);

    //            //if path has been found
    //            if (currentNode == targetNode)
    //            {
    //                sw.Stop();
    //                print("Path found: " + sw.ElapsedMilliseconds + " ms");
    //                pathSuccess = true;
    //                break;
    //            }


    //            foreach (Node neighbour in grid.GetNeighbours(currentNode))
    //            {
    //                if (!neighbour.walkable || closedSet.Contains(neighbour))
    //                    continue;

    //                int newMoveCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.moveCost;

    //                if (newMoveCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
    //                {
    //                    neighbour.gCost = newMoveCostToNeighbour;
    //                    neighbour.hCost = GetDistance(currentNode, targetNode);
    //                    neighbour.parent = currentNode;

    //                    if (!openSet.Contains(neighbour))
    //                        openSet.Add(neighbour);
    //                    else
    //                        openSet.UpdateItem(neighbour);
    //                }
    //            }

    //        }

    //    }

    //    yield return null;

    //    if(pathSuccess)
    //    {
    //        wayPoints = RetracePath(startNode, targetNode);
    //    }
    //    pathManager.FinishedProcessingPath(wayPoints, pathSuccess);
    //}

    #endregion

    #region Pathfinding With Threading
    public void FindPath(PathRequestForThreading request, Action<PathResult> callback)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] wayPoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);

        if (startNode.walkable && targetNode.walkable)
        {
            Heap<Node> openSet = new Heap<Node>(grid.GetMaxSize);  //set of nodes to be evaluated
            HashSet<Node> closedSet = new HashSet<Node>();   //set of nodes already evaluated

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                //Fastest and optimised approach
                Node currentNode = openSet.RemoveFirst();

                closedSet.Add(currentNode);

                //if path has been found
                if (currentNode == targetNode)
                {
                    sw.Stop();
                    print("Path found: " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    break;
                }


                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                        continue;

                    int newMoveCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.moveCost;

                    if (newMoveCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMoveCostToNeighbour;
                        neighbour.hCost = GetDistance(currentNode, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }

            }

        }


        if (pathSuccess)
        {
            wayPoints = RetracePath(startNode, targetNode);
            pathSuccess = wayPoints.Length > 0;
        }

        callback(new PathResult(wayPoints, pathSuccess, request.callback));
    }
    #endregion 

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
      
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    //gets the shortest distance between A and B
    private int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);

        return 14 * distX + 10 * (distY - distX);
    }
}
