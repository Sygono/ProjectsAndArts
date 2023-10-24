using UnityEngine;
using System.Collections.Generic;

public class GridCell
{
    public BlockType blockType;
    public bool isSolid() {
        return blockType==BlockType.Solid;
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
}

public class Main : MonoBehaviour
{
    [Range(1, 100)]
    public int gridSizeX = 1;
    [Range(1, 100)]
    public int gridSizeY = 1;
    public float cellSize = 1f;
    public GridCell[,] grid;
    public LayerMask layerMask;
    public Transform agent;
    public Transform goal;

    private List<Vector2> points;
    private TriangleGrid trig;

    // The triangulation algorithm goes here (e.g., using the DelaunayTriangulation class).
    public void Awake()
    {
        InitializeGrid();
        // Other initialization logic here
        FindPath();
    }

    private void FindPath() {
        if (agent==null||goal==null) return;
        Triangle start = trig.GetTriangle(new Vector2(agent.position.x, agent.position.y));
        Triangle end = trig.GetTriangle(new Vector2(goal.position.x, goal.position.y));
        if (start==null||end==null) return;
        TriangleSearch trigSearch = new TriangleSearch(trig, start, end);
        List<object> path = SearchProblem.BreadthFirstSearch(trigSearch);
        List<Edge> pathEdges = new List<Edge>();
        foreach(object obj in path) {
            pathEdges.Add((Edge)obj);
        }
        List<Triangle> pathTriangles = new List<Triangle>();
        //pathTriangles.Add(start);
        for (int i=0; i<pathEdges.Count; i++) {
            Triangle prev = start;
            Edge edge = (Edge)pathEdges[i];
            start = start.neighbors[start.Edges.IndexOf(edge)];
            DebugShapes.DrawLine(prev.GetCenter(), start.GetCenter(), Color.yellow);
            DebugShapes.DrawLine(edge.Point1, edge.Point2, Color.yellow);
            pathTriangles.Add(start);
        }
        
        List<Vector2> pathPoints = Funnel.GetPath(new Vector2(agent.position.x, agent.position.y), pathEdges, pathTriangles, 1);
        foreach (Vector2 v in pathPoints) {
            DebugShapes.DrawSphere(new Vector3(v.x,v.y,0), 0.5f, Color.red);
        }
        //Debug.Log(pathPoints.Count);
        
    }

    public void InitializeGrid()
    {
        grid = new GridCell[gridSizeX, gridSizeY];
        points = new List<Vector2>(){new Vector2(0,0), new Vector2(gridSizeX,0), new Vector2(0,gridSizeY), new Vector2(gridSizeX,gridSizeY)};

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                GridCell cell = new GridCell();
                grid[x, y] = cell;
                Vector3 cellCenter = GetCellCenter(x, y);
                Collider[] overlaps = Physics.OverlapBox(cellCenter, Vector3.one * (cellSize / 2), Quaternion.identity, layerMask);
                foreach(Collider collider in overlaps) {
                    Block block = collider.gameObject.GetComponent<Block>();
                    if (block != null) {
                        cell.blockType = block.blockType;
                    }
                }
                if (cell.isSolid()) {
                    AddToPoints(new Vector2(x,y));
                    AddToPoints(new Vector2(x+1,y));
                    AddToPoints(new Vector2(x,y+1));
                    AddToPoints(new Vector2(x+1,y+1));
                }
            }
        }

        trig = new TriangleGrid(points);
        foreach (Triangle triangle in trig.triangles)
        {
            foreach (Edge edge in triangle.Edges) {
                DebugShapes.DrawLine(edge.Point1, edge.Point2, new Color(0f,1f,0f,0.1f));
            }
        }
    }

    private void AddToPoints(Vector2 point) {
        if (points.Contains(point)) {
            points.Remove(point);
        } else {
            points.Add(point);
        }
    }

    public Vector3 GetCellCenter(int x, int y) {
        return GetCellCenter(x, y, 0);
    }

    public Vector3 GetCellCenter(int x, int y, int z)
    {
        // Calculate the cell's center based on its position in the grid
        float centerX = x * cellSize + cellSize / 2;
        float centerY = y * cellSize + cellSize / 2;
        float centerZ = z * cellSize + cellSize / 2;

        return transform.position+new Vector3(centerX, centerY, centerZ);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Awake();
        }
        Gizmos.color = Color.yellow;
        foreach (Vector2 point in points)
        {
            Gizmos.DrawSphere(new Vector3(point.x, point.y, 0f)+transform.position, 0.1f);
        }
        // Draw points as red spheres.
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                GridCell cell = grid[x, y];
                Vector3 cellCenter = GetCellCenter(x, y);
                Gizmos.color = cell.GetColor();
                Gizmos.DrawCube(cellCenter, Vector3.one * cellSize * 0.8f);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(cellCenter, Vector3.one * cellSize);
            }
        }

        
    }
}
