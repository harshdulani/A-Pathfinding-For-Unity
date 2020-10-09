using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool displayGizmoCubes = true;

    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    [Header("Don't assign one terrainMask more than one layer")]
    public TerrainType[] walkableLayers;

    private Node[,] grid;

    private float nodeDiameter;
    private int gridSizeX, gridSizeY;
    private LayerMask walkableMask;
    private Dictionary<int, int> walkableLayersDictionary = new Dictionary<int, int>();

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        foreach (var region in walkableLayers)
        {
            #region explanation
            //layermasks are stored using binary values, layer 0 is 2^0 = 1, layer 9 is 2^9 = 512 and so on
            //if we get the values of these layers from unity, it gives us values like 32, 64, 1, etc.
            //that is why to have multiple layers (for eg: layer 9 and 10) in a mask,
            //we can add the layer values manually 
            //or just to bitwise OR the layers themselves (for eg: 2^0 || 2^5 = 1 + 16)
            #endregion
            walkableMask.value |= region.terrainMask.value;
            walkableLayersDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.layerPenalty);
        }

        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint =
                    worldBottomLeft +
                    Vector3.right * (x * nodeDiameter + nodeRadius) +
                    Vector3.forward * (y * nodeDiameter + nodeRadius);

                bool isWalkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                int movementPenalty = 0;

                //raycast code
                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit, 100, walkableMask))
                {
                    walkableLayersDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                }

                grid[x, y] = new Node(isWalkable, worldPoint, x, y, movementPenalty);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1f, gridWorldSize.y));

        if (displayGizmoCubes)
            if (grid != null)
            {
                foreach (Node node in grid)
                {
                    Gizmos.color = (node.isWalkable) ? Color.white : Color.red;
                    Gizmos.DrawCube(node.worldPos, Vector3.one * (nodeDiameter - 0.1f));
                }
            }
    }

    public Node NodeFromWorldPoint(Vector3 worldPos)
    {
        float percentX = (worldPos.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPos.z + gridWorldSize.y / 2) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    public List<Node> GetNeighbourNodes(Node root)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = root.gridX + x;
                int checkY = root.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    neighbors.Add(grid[checkX, checkY]);
            }
        }

        return neighbors;
    }

    [System.Serializable]
    public class TerrainType
    {
        [Header("Even if they have same penalty values")]
        public LayerMask terrainMask;
        public int layerPenalty;
    }
}