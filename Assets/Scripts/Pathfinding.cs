using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

//Slower and inefficient method for pathfinding
public class Pathfinding : MonoBehaviour
{
    public Transform startPos, targetPos;

    Grid grid;

    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void Update()
    {
        if(Input.GetButtonDown("Jump"))
        {
            FindPath(startPos.position, targetPos.position);
        }
      
    }

    void FindPath(Vector3 startPos,Vector3 targetPos)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();  //set of nodes to be evaluated
        HashSet<Node> closedSet = new HashSet<Node>();   //set of nodes already evaluated

        openSet.Add(startNode);

        while(openSet.Count>0)
        {
            #region Slowest approach
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if(openSet[i].fCost<currentNode.fCost || (openSet[i].fCost==currentNode.fCost && openSet[i].hCost<currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }
            openSet.Remove(currentNode);
            #endregion

            closedSet.Add(currentNode);

            //if path has been found
            if (currentNode == targetNode)
            {
                sw.Stop();
                print("Path found: " + sw.ElapsedMilliseconds + " ms");
                RetracePath(startNode, targetNode);
                return;
            }
              

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;

                int newMoveCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                if(newMoveCostToNeighbour<neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMoveCostToNeighbour;
                    neighbour.hCost = GetDistance(currentNode, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
            
        }
    }

    void RetracePath(Node startNode,Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode!=startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        grid.path = path;
    }

    //gets the shortest distance between A and B
    private int GetDistance(Node nodeA,Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);

        return 14 * distX + 10 * (distY - distX);
    }
}
