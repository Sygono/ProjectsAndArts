using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
    public Vector3 position;
    public BlockType blockType;

    public GridCell(Vector3 position, BlockType blockType = BlockType.Empty)
    {
        this.position = position;
        this.blockType = blockType;
    }

    public int GetCost() {
        switch (blockType)
        {
            case BlockType.Solid:
                return 99999; // Modify this cost as needed
            case BlockType.Slow:
                return 10; // Modify this cost as needed
            case BlockType.Lazer:
                return 20; // Modify this cost as needed
            default:
                return 1; // Default cost
        }
    }

    public Color GetColor()
    {
        switch (blockType)
        {
            case BlockType.Solid:
                return Color.red;
            case BlockType.Slow:
                return Color.white;
            case BlockType.Lazer:
                // Return the color for Lazer if needed.
                return Color.yellow;
            default:
                return Color.clear; // Handle other cases as needed.
        }
    }

    public bool isSolid
    {
        get { return blockType == BlockType.Solid; }
    }
}

[DefaultExecutionOrder(-1)]
public class Level : MonoBehaviour
{
    [Range(1, 100)]
    public int gridSizeX = 1;
    [Range(1, 100)]
    public int gridSizeY = 1;
    [Range(1, 100)]
    public int gridSizeZ = 1;
    public float cellSize = 1f;
    public GridCell[,,] grid;
    public LayerMask layerMask;

    public List<Drone> drones;
    public List<Vector3Int> checkpoints = new List<Vector3Int>();

    public List<Vector3Int> droneCoordinates = new List<Vector3Int>();

    private PositionSearchProblem problem;

    private List<Direction> actions = new List<Direction>();

    public void Start()
    {
        InitializeGrid();
        // Other initialization logic here
    }

    public void InitializeGrid()
    {
        grid = new GridCell[gridSizeX, gridSizeY, gridSizeZ];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 cellPosition = new Vector3(x * cellSize, y * cellSize, z * cellSize);
                    GridCell cell = new GridCell(cellPosition);
                    grid[x, y, z] = cell;
                    Vector3 cellCenter = GetCellCenter(x, y, z);
                    Collider[] overlaps = Physics.OverlapBox(cellCenter, Vector3.one * (cellSize / 2), Quaternion.identity, layerMask);
                    foreach(Collider collider in overlaps) {
                        Block block = collider.gameObject.GetComponent<Block>();
                        if (block != null) {
                            cell.blockType = block.blockType;
                        }
                    }
                }
            }
        }
    }

    void Update() {
        UpdateDroneCoordinates();
        //TestSearching();
    }

    public void TestSearching() {
        problem = new PositionSearchProblem(droneCoordinates[0], checkpoints[0], this);
        actions = SearchProblem.AStarSearch(problem);
    }

    public GridCell GetCell(Vector3Int pos)
    {
        if (IsWithinBounds(pos))
        {
            return grid[pos.x, pos.y, pos.z];
        }
        else
        {
            return null; // Example: returning null for out-of-bounds
        }
    }

    public bool IsWithinBounds(Vector3Int pos)
    {
        return pos.x >= 0 && pos.x < grid.GetLength(0) &&
            pos.y >= 0 && pos.y < grid.GetLength(1) &&
            pos.z >= 0 && pos.z < grid.GetLength(2);
    }

    public Vector3 GetCellCenter(int x, int y, int z)
    {
        // Calculate the cell's center based on its position in the grid
        float centerX = x * cellSize + cellSize / 2;
        float centerY = y * cellSize + cellSize / 2;
        float centerZ = z * cellSize + cellSize / 2;

        return transform.position+new Vector3(centerX, centerY, centerZ);
    }

    public void UpdateDroneCoordinates()
    {
        droneCoordinates = new List<Vector3Int>();

        foreach (Drone drone in drones)
        {
            Vector3 dronePosition = drone.transform.position;
            Vector3 localPosition = dronePosition - transform.position; // Convert to local coordinates

            int cellX = Mathf.FloorToInt(localPosition.x / cellSize);
            int cellY = Mathf.FloorToInt(localPosition.y / cellSize);
            int cellZ = Mathf.FloorToInt(localPosition.z / cellSize);

            Vector3Int cellCoordinate = new Vector3Int(cellX, cellY, cellZ);
            droneCoordinates.Add(cellCoordinate);
        }
    }

    public List<Vector3Int> GetSuccessors(Vector3Int pos)
    {
        List<Vector3Int> legalMoves = new List<Vector3Int>();

        foreach (Direction direction in Directions.GetAllDirections())
        {
            Vector3Int newPosition = pos + Directions.DirectionToVector(direction);
            if (IsWithinBounds(newPosition))
            {
                legalMoves.Add(newPosition);
            }
        }
        return legalMoves;
    }

    public Vector3Int GetDronePos(Drone drone) {
        return droneCoordinates[drones.IndexOf(drone)];
    }


    public void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            InitializeGrid();
            UpdateDroneCoordinates();
        }

        Gizmos.color = Color.blue; // Set the color for grid visualization

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    GridCell cell = grid[x, y, z];
                    Vector3 cellCenter = GetCellCenter(x, y, z);
                    
                    Gizmos.color = cell.GetColor();
                    Gizmos.DrawCube(cellCenter, Vector3.one * cellSize * 0.8f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, cellSize, cellSize));
                }
            }
        }

        foreach (Vector3Int pos in checkpoints) {
            Vector3 cellCenter = GetCellCenter(pos.x, pos.y, pos.z);
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(cellCenter, Vector3.one * cellSize * 0.8f);
        }

        foreach (Vector3Int pos in droneCoordinates) {
            Vector3 cellCenter = GetCellCenter(pos.x, pos.y, pos.z);
            Gizmos.color = Color.yellow;
            //Gizmos.DrawCube(cellCenter, Vector3.one * cellSize * 0.8f);
        }

        Vector3Int probe = droneCoordinates[0];
        for (int i=0; i<actions.Count; i++) {
            Vector3 cellCenter = GetCellCenter(probe.x, probe.y, probe.z);
            Gizmos.color = Color.green;
            Gizmos.DrawCube(cellCenter, Vector3.one * cellSize * 0.8f);
            Direction action = actions[i];
            probe += Directions.DirectionToVector(action);

        }
    }
}

