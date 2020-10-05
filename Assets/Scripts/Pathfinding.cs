using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public Transform seeker, target;

    private Grid grid;

    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void Update()
    {
        FindPath(seeker.position, target.position);
    }

    private void FindPath(Vector3 startPos, Vector3 endPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node endNode = grid.NodeFromWorldPoint(endPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            //current node is the resting node
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                //c
                //unoptimised if
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    currentNode = openSet[i];
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == endNode)
            {
                RetracePath(startNode, endNode);
                return;
            }

            foreach (Node neighbor in grid.GetNeighbourNodes(currentNode))
            {
                if (closedSet.Contains(neighbor) || !neighbor.isWalkable)
                    continue;

                int newMovementCostToNeighbor = currentNode.gCost + GetNodeDistance(neighbor, currentNode);

                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetNodeDistance(neighbor, endNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
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

    private void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode.parent);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        grid.path = path;
    }
}