using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool isWalkable;
    public Vector3 worldPos;

    public Node(bool _walkable, Vector3 _worldPos)
    {
        isWalkable = _walkable;
        worldPos = _worldPos;
    }
}
