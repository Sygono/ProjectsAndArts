using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Edge
{
    public HashSet<Vertex> vertices;

    public Vertex midPoint;

    public List<Shape> neighbors = new List<Shape>();

    public Edge(Vertex a, Vertex b, List<Edge> edges) {
        vertices = new HashSet<Vertex> {a,b};
        edges.Add(this);
    }

    public static Edge FindEdge(Vertex a, Vertex b, List<Edge> edges) {
        foreach (Edge edge in edges) {
            if (edge.vertices.Contains(a) && edge.vertices.Contains(b)) return edge;
        }
        return null;
    }

    public void AddNeighbor(Shape shape) {
        neighbors.Add(shape);
    }

    public void Dissolve(List<Quad> quads, List<Triangle> triangles, List<Edge> edges, List<Edge> notMerged) {
        Shape firstTrig = neighbors[0];
        Shape secondTrig = neighbors[1];
        HashSet<Vertex> result = new HashSet<Vertex>(firstTrig.vertices);
        result.ExceptWith(vertices);
        Vertex a = result.Single();
        Vertex b = firstTrig.vertices.NextOf(a);
        Vertex c = secondTrig.vertices.NextOf(b);
        Vertex d = secondTrig.vertices.NextOf(c);
        foreach (Shape neighbor in neighbors.ToList()) {
            foreach (Edge otherEdge in neighbor.edges.ToList()) {
                if (otherEdge.neighbors.Contains(firstTrig)) {otherEdge.neighbors.Remove(firstTrig);notMerged.Remove(otherEdge);}
                if (otherEdge.neighbors.Contains(secondTrig)) {otherEdge.neighbors.Remove(secondTrig);notMerged.Remove(otherEdge);}
            }
        }
        Quad q = new Quad(a,b,c,d,edges,quads);
        triangles.Remove((Triangle)firstTrig);
        triangles.Remove((Triangle)secondTrig);
        edges.Remove(this);
        notMerged.Remove(this);
    }

    public void Subdivide(List<Edge> subEdges, Vertex center) {
        if (midPoint == null) {
            Vertex[] array = this.vertices.ToArray();
            midPoint = Vertex.MidPoint(array);
            new Edge(midPoint, array[0], subEdges);
            new Edge(midPoint, array[1], subEdges);
        } 
        new Edge(midPoint, center, subEdges);
    }
}
