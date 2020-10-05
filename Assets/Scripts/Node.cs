using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool isWalkable;
    public Vector3 worldPos;
    public int gridX, gridY;
    public Node parent;

    public int gCost, hCost;

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        isWalkable = _walkable;
        worldPos = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
}
