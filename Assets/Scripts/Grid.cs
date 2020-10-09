using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool displayGizmoCubes = true;

    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;

    public int obstacleProximityPenalty;
    [Header("Don't assign one terrainMask more than one layer")]
    public TerrainType[] walkableLayers;

    private Node[,] grid;

    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    private LayerMask walkableMask;
    private Dictionary<int, int> walkableLayersDictionary = new Dictionary<int, int>();
    private int minPenalty = int.MaxValue, maxPenalty = int.MinValue;

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

                if (!isWalkable)
                    movementPenalty += obstacleProximityPenalty;

                grid[x, y] = new Node(isWalkable, worldPoint, x, y, movementPenalty);
            }
        }

        BlurPenaltyMap(3);
    }

    private void BlurPenaltyMap(int blurSize)
    {
        int kernelSize = blurSize * 2 + 1;
        int kernelExtents = blurSize;

        //holds results
        int[,] horizontalPassPenalties = new int[gridSizeX, gridSizeY];
        int[,] verticalPassPenalties = new int[gridSizeX, gridSizeY];

        for (int y = 0; y < gridSizeY; y++)
        {
            //horizontal pass
            //
            //0th element of ALL rows
            for(int x = -kernelExtents; x <= kernelExtents; x++)
            {
                int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                horizontalPassPenalties[0, y] += grid[sampleX, y].movementPenalty;
            }
            //for other elements
            for(int x = 1; x < gridSizeX; x++)
            {
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

                horizontalPassPenalties[x, y] = horizontalPassPenalties[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
            }
        }

        //vertical pass
        for (int x = 0; x < gridSizeX; x++)
        {            
            for (int y = -kernelExtents; y <= kernelExtents; y++)
            {
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                verticalPassPenalties[x, 0] += horizontalPassPenalties[x, sampleY];
            }

            int blurredPenalty = Mathf.RoundToInt((float)verticalPassPenalties[x, 0] / (kernelSize * kernelSize));
            grid[x, 0].movementPenalty = blurredPenalty;

            for (int y = 1; y < gridSizeY; y++)
            {
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeX - 1);

                verticalPassPenalties[x, y] = verticalPassPenalties[x, y - 1] - horizontalPassPenalties[x, removeIndex] + horizontalPassPenalties[x, addIndex];

                blurredPenalty = Mathf.RoundToInt((float)verticalPassPenalties[x, y] / (kernelSize * kernelSize));
                grid[x, y].movementPenalty = blurredPenalty;

                if (blurredPenalty > maxPenalty)
                    maxPenalty = blurredPenalty;
                if (blurredPenalty < minPenalty)
                    minPenalty = blurredPenalty;
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
                    Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(minPenalty, maxPenalty, node.movementPenalty));
                    Gizmos.color = (node.isWalkable) ? Gizmos.color : Color.red;
                    Gizmos.DrawCube(node.worldPos, Vector3.one * (nodeDiameter));
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