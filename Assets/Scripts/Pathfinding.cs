using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour
{
    private Grid grid;
    private PathRequestManager requestManager;

    private void Awake()
    {
        grid = GetComponent<Grid>();
        requestManager = GetComponent<PathRequestManager>();
    }

public void StartFindingPath(Vector3 pathStart, Vector3 pathEnd)
    {
        StartCoroutine(FindPath(pathStart, pathEnd));
    }

    private IEnumerator FindPath(Vector3 startPos, Vector3 endPos)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node endNode = grid.NodeFromWorldPoint(endPos);

        if (startNode.isWalkable && endNode.isWalkable)
        {
            //List<Node> openSet = new List<Node>(); //4ms
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize); //0-4ms
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                //current node is the resting node
                #region legacy slower method
                /*List method
                Node currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    //c
                    //unoptimised if
                    if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                        currentNode = openSet[i];
                }

                openSet.Remove(currentNode);*/
                #endregion

                //heap method
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == endNode)
                {
                    sw.Stop();
                    print("Path found in " + sw.ElapsedMilliseconds + " ms.");

                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbor in grid.GetNeighbourNodes(currentNode))
                {
                    if (closedSet.Contains(neighbor) || !neighbor.isWalkable)
                        continue;

                    int newMovementCostToNeighbor = currentNode.gCost + GetNodeDistance(neighbor, currentNode) + currentNode.movementPenalty;

                    if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetNodeDistance(neighbor, endNode);
                        neighbor.parent = currentNode;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                        else
                            openSet.UpdateItem(neighbor);
                    }
                }
            }
        }
        //to wait for one frame before returning
        yield return null;

        if (pathSuccess)
            waypoints = RetracePath(startNode, endNode);

        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    private int GetNodeDistance(Node a, Node b)
    {
        #region bad description
        //in a nodegrid, the shortest distance from (1, 1) to (6, 3)
        //would be calculated by finding out which axis has lesser difference in points
        //make only diagonal moves "(int)smallerAxisDifference" times in "largerAxis" direction
        //after that is done, now only (largerDifference - smallerDifference) units need to be traversed in the largerAxis direction
        #endregion

        int distX = Mathf.Abs(a.gridX - b.gridX);
        int distY = Mathf.Abs(a.gridY - b.gridY);

        if (distX < distY)
            return 14 * distX + 10 * (distY - distX);

        return 14 * distY + 10 * (distX - distY);
    }

    private Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode.parent);
            currentNode = currentNode.parent;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);

        return waypoints;
    }

    private Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2 directionNew = new Vector2(path[i].gridX - path[i + 1].gridX, path[i].gridY - path[i + 1].gridY);
            if (directionNew != directionOld)
                waypoints.Add(path[i].worldPos);
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }
}