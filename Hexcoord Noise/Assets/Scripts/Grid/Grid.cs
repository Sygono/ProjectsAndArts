using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridHex
{
    public int radius;

    public float cellSize;

    public int layers;

    public List<VertexHex> vertices = new List<VertexHex>();
    public List<Edge> edges = new List<Edge>();
    public List<Triangle> triangles = new List<Triangle>();
    public List<Quad> quads = new List<Quad>();
    public List<Edge> subEdges = new List<Edge>();
    public List<Quad> subQuads = new List<Quad>();

    public List<Quad>[] cellToSubQuads; 

    public GridHex(int radius, float cellSize, int layers) {
        this.radius = radius;
        this.cellSize = cellSize;
        this.layers = layers;
        VertexHex.Hex(vertices, radius, cellSize);
        Triangle.Hex(triangles, edges, vertices, radius);
    }

    public void RandomDissolve() {
        List<Edge> notMerged = new List<Edge>();
        foreach(Edge edge in this.edges) {
            if (edge.neighbors.Count != 2) continue;
            foreach (Shape shape in edge.neighbors) {
                if ((Triangle)shape == null) continue;
            }
            notMerged.Add(edge);
        }
        while(notMerged.Count>0) {
            int randomIndex = UnityEngine.Random.Range(0,notMerged.Count);
            Edge randomEdge = notMerged[randomIndex];
            randomEdge.Dissolve(quads,triangles,edges,notMerged);
        }
    }

    public void Subdivide() {
        cellToSubQuads = new List<Quad>[vertices.Count];
        foreach (Triangle triangle in triangles) {
            triangle.Subdivide(subEdges,subQuads,cellToSubQuads,vertices);
        }
        foreach (Quad quad in quads) {
            quad.Subdivide(subEdges,subQuads,cellToSubQuads,vertices);
        }
    }

    public void Relax(int iterations) {
        while (iterations>0) {
            foreach (Quad quad in subQuads) {
                quad.Relax();
            }
            iterations-=1;
        }
    }

    

}