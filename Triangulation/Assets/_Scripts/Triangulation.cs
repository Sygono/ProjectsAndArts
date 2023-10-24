using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleGrid
{
    public List<Triangle> triangles;
    public List<Edge> edges;
    public List<Vector2> points;

    public void AddPoints(List<Vector2> pointList) {
        // Iterate through each point in pointList.
        foreach (Vector2 point in pointList)
        {
            List<Triangle> badTriangles = new List<Triangle>();

            // Find all triangles that are no longer valid due to the insertion of the point.
            foreach (Triangle triangle in triangles)
            {
                if (triangle.IsPointInsideCircumcircle(point))
                {
                    badTriangles.Add(triangle);
                }
            }

            List<Edge> polygon = new List<Edge>();

            // Find the boundary of the polygonal hole.
            foreach (Triangle triangle in badTriangles)
            {
                foreach (Edge edge in triangle.Edges)
                {
                    Triangle neighbor = triangle.neighbors[triangle.Edges.IndexOf(edge)];
                    if (neighbor==null) {
                        polygon.Add(edge);
                        continue;
                    }
                    if (!badTriangles.Contains(neighbor)) {
                        int thisIndex = neighbor.Edges.IndexOf(edge);
                        neighbor.neighbors[thisIndex] = null;
                        polygon.Add(edge);
                    }
                }
            }

            // Remove the bad triangles from the triangulation.
            foreach (Triangle triangle in badTriangles)
            {
                triangles.Remove(triangle);
            }

            // Re-triangulate the polygonal hole.
            foreach (Edge edge in polygon)
            {
                new Triangle(edge.Point1, edge.Point2, point, ref edges, ref triangles);
            }
        }
    }

    public void RemovePoints(List<Vector2> pointList) {

    }

    public TriangleGrid(List<Vector2> pointList) {
        triangles = new List<Triangle>();
        edges = new List<Edge>();
        points = new List<Vector2>();
        // Create a super-triangle large enough to contain all points and add it to the triangulation.
        AddSuperTriangle(pointList);
        
        AddPoints(pointList);
    }

    private void AddSuperTriangle(List<Vector2> pointList)
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        // Find the bounds of the point set.
        foreach (Vector2 point in pointList)
        {
            if (point.x < minX)
                minX = point.x;
            if (point.x > maxX)
                maxX = point.x;
            if (point.y < minY)
                minY = point.y;
            if (point.y > maxY)
                maxY = point.y;
        }

        // Define the four vertices of the rectangle (two large triangles).
        Vector2 p1 = new Vector2(minX , minY );           // Bottom-left
        Vector2 p2 = new Vector2(maxX , minY );           // Bottom-right
        Vector2 p3 = new Vector2(minX , maxY );           // Top-left
        Vector2 p4 = new Vector2(maxX , maxY );           // Top-right

        // Create the two large triangles (rectangle) by combining the vertices.
        new Triangle(p1, p2, p3,  ref edges, ref triangles);
        new Triangle(p2, p4, p3,  ref edges, ref triangles);
    }

    public Triangle GetTriangle(Vector2 point) {
        foreach (Triangle triangle in triangles) {
            if (triangle.IsPointInside(point)) return triangle;
        }
        return null;
    }
}

public class Triangle
{
    public Vector2 Point1, Point2, Point3;
    public List<Edge> Edges;
    public List<Triangle> neighbors;

    public Triangle(Vector2 p1, Vector2 p2, Vector2 p3, ref List<Edge> edges, ref List<Triangle> triangles)
    {
        Point1 = p1;
        Point2 = p2;
        Point3 = p3;

        Edges = new List<Edge>();
        neighbors = new List<Triangle>();

        AddUniqueEdge(Point1, Point2, ref edges, triangles);
        AddUniqueEdge(Point2, Point3, ref edges, triangles);
        AddUniqueEdge(Point3, Point1, ref edges, triangles);

        triangles.Add(this);
    }

    public Vector2 GetCenter()
    {
        float centerX = (Point1.x + Point2.x + Point3.x) / 3.0f;
        float centerY = (Point1.y + Point2.y + Point3.y) / 3.0f;
        return new Vector2(centerX, centerY);
    }

    public bool IsPointInside(Vector2 point)
    {
        Vector2 v0 = Point2 - Point1;
        Vector2 v1 = Point3 - Point1;
        Vector2 v2 = point - Point1;

        float dot00 = Vector2.Dot(v0, v0);
        float dot01 = Vector2.Dot(v0, v1);
        float dot02 = Vector2.Dot(v0, v2);
        float dot11 = Vector2.Dot(v1, v1);
        float dot12 = Vector2.Dot(v1, v2);

        // Compute barycentric coordinates
        float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        // Check if the point is inside the triangle
        return (u >= 0) && (v >= 0) && (u + v <= 1);
    }

    public bool IsPointInsideCircumcircle(Vector2 point)
    {
        // Get the vertices of the triangle.
        Vector2 p1 = Point1;
        Vector2 p2 = Point2;
        Vector2 p3 = Point3;

        // Calculate the coordinates of the circumcenter (centerX, centerY).
        float d = 2 * (p1.x * (p2.y - p3.y) + p2.x * (p3.y - p1.y) + p3.x * (p1.y - p2.y));
        float centerX = ((p1.x * p1.x + p1.y * p1.y) * (p2.y - p3.y) + (p2.x * p2.x + p2.y * p2.y) * (p3.y - p1.y) + (p3.x * p3.x + p3.y * p3.y) * (p1.y - p2.y)) / d;
        float centerY = ((p1.x * p1.x + p1.y * p1.y) * (p3.x - p2.x) + (p2.x * p2.x + p2.y * p2.y) * (p1.x - p3.x) + (p3.x * p3.x + p3.y * p3.y) * (p2.x - p1.x)) / d;

        // Calculate the circumradius.
        float circumradius = Mathf.Sqrt((p1.x - centerX) * (p1.x - centerX) + (p1.y - centerY) * (p1.y - centerY));

        // Calculate the distance between the circumcenter and the test point.
        float distance = Mathf.Sqrt((point.x - centerX) * (point.x - centerX) + (point.y - centerY) * (point.y - centerY));

        // Check if the distance is less than the circumradius.
        return distance < circumradius;
    }

    private void AddUniqueEdge(Vector2 p1, Vector2 p2, ref List<Edge> edges, List<Triangle> triangles)
    {
        Edge newEdge = new Edge(p1, p2);

        foreach (Triangle triangle in triangles) {
            foreach (Edge edge in triangle.Edges) {
                if (newEdge.Equals(edge)) {
                    Edges.Add(edge);
                    int index = triangle.Edges.IndexOf(edge);
                    triangle.neighbors[index] = this;
                    neighbors.Add(triangle);
                    return;
                }
            }
        }
        Edges.Add(newEdge);
        edges.Add(newEdge);
        neighbors.Add(null);
    }
}

public class Edge
{
    public Vector2 Point1, Point2;

    public Edge(Vector2 p1, Vector2 p2)
    {
        Point1 = p1;
        Point2 = p2;
    }

    public bool Equals(Edge other)
    {
        return (Point1 == other.Point1 && Point2 == other.Point2) || (Point1 == other.Point2 && Point2 == other.Point1);
    }
}

