using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape
{
    public Vertex[] vertices;
    public Edge[] edges;
    public Vertex center;

    public void ReadNeighbors() {
        foreach (Edge edge in this.edges) {
            edge.AddNeighbor(this);
        }
    }

    public void Subdivide(List<Edge> subEdges, List<Quad> subQuads, List<Quad>[] cellToSubQuads, List<VertexHex> mainVertices) {
        foreach (Edge edge in this.edges) {
            edge.Subdivide(subEdges, center);
        }
        for (int i=0; i<this.edges.Length; i++) {
            Quad quad = new Quad(this.vertices[i], this.edges[i].midPoint, this.center, this.edges.PrevOf(edges[i]).midPoint, subEdges, subQuads);
            int index = mainVertices.IndexOf((VertexHex)this.vertices[i]);

            if (cellToSubQuads[index] == null) {
                cellToSubQuads[index] = new List<Quad>();
            }
            cellToSubQuads[index].Add(quad);
        }
    }
}

public class Triangle : Shape
{
    public VertexHex a;
    public VertexHex b;
    public VertexHex c;

    public Edge ab;
    public Edge bc;
    public Edge ac;

    public VertexHex[] hexes;

    public Triangle(VertexHex a, VertexHex b, VertexHex c, List<Edge> edges, List<Triangle> triangles) {
        this.a = a;
        this.b = b;
        this.c = c;

        this.ab = Edge.FindEdge(a,b,edges);
        this.bc = Edge.FindEdge(b,c,edges);
        this.ac = Edge.FindEdge(a,c,edges);

        if (ab == null) {
            this.ab = new Edge(a,b,edges);
        }
        if (bc == null) {
            this.bc = new Edge(c,b,edges);
        }
        if (ac == null) {
            this.ac = new Edge(a,c,edges);
        }

        this.hexes = new VertexHex[]{a,b,c};
        this.vertices = (Vertex[])hexes;
        this.edges = new Edge[]{ab, bc, ac};

        triangles.Add(this);
        this.center = Vertex.MidPoint(this.vertices);

        ReadNeighbors();
    }

    public static void Ring(List<Triangle> triangles, List<Edge> edges, List<VertexHex> hexes, int radius) {
        List<VertexHex> inner = VertexHex.GetRing(radius-1, hexes);
        List<VertexHex> outer = VertexHex.GetRing(radius, hexes);
        for (int i=0; i<6; i++) {
            for (int j=0; j<radius; j++) {
                VertexHex a = outer[i*radius+j];
                VertexHex b = outer[(i*radius+j+1)%outer.Count];
                VertexHex c = inner[(i*(radius-1)+j)%inner.Count];
                new Triangle(a,b,c,edges, triangles);

                if (j>0) {
                    VertexHex d = inner[i*(radius-1)+j-1];
                    new Triangle(a,c,d,edges, triangles);
                }
            }
        }
    }

    public static void Hex(List<Triangle> triangles, List<Edge> edges, List<VertexHex> hexes, int radius) {
        for (int i=1; i<=radius; i++) {
            Ring(triangles, edges, hexes, i);
        }
    }

    /*
 
    public bool isNeighbor(Triangle target) {
        HashSet<Edge> intersect = new HashSet<Edge>(edges);
        intersect.IntersectWith(target.edges);
        return intersect.Count == 1;
    }

    public List<Triangle> FindAllNeighbors(List<Triangle> triangles) {
        List<Triangle> result = new List<Triangle>();
        foreach (Triangle triangle in triangles) {
            if(this.isNeighbor(triangle)) {
                result.Add(triangle);
            }
        }
        return result;
    }

    public Edge NeighborEdge(Triangle neighbor) {
        HashSet<Edge> result = new HashSet<Edge>(this.edges);
        result.IntersectWith(neighbor.edges);
        return result.Single();
    }


    public VertexHex IdenticalVertex(Triangle neighbor) {
        HashSet<VertexHex> result = new HashSet<VertexHex>(vertices);
        result.ExceptWith(NeighborEdge(neighbor).vertices);
        return result.Single();
    }

    public void MergeWith(Triangle target, List<Edge> edges, List<Triangle> triangles, List<Quad> quads) {
        VertexHex quad_a = this.IdenticalVertex(target);
        VertexHex quad_b = this.vertices.NextOf(quad_a);
        VertexHex quad_c = target.IdenticalVertex(this);
        VertexHex quad_d = target.vertices.NextOf(quad_c);

        new Quad(quad_a, quad_b, quad_c, quad_d,edges, quads);
        edges.Remove(this.NeighborEdge(target));
        triangles.Remove(target);
        triangles.Remove(this);
    }

    public static void RandomMergeEdges(List<Edge> edges, List<Triangle> triangles, List<Quad> quads) {
        int randomIndex = UnityEngine.Random.Range(0,triangles.Count);
        List<Triangle> neighbors = triangles[randomIndex].FindAllNeighbors(triangles);
        if (neighbors.Count != 0) {
            int randomSide = UnityEngine.Random.Range(0,neighbors.Count);
            triangles[randomIndex].MergeWith(neighbors[randomSide], edges, triangles, quads);
        }
    }

    public static bool CompleteMerge(List<Triangle> triangles) {
        foreach (Triangle triangle in triangles) {
            if (triangle.FindAllNeighbors(triangles).Count > 0) return false;
        }
        return true;
    }

    public static bool CompleteMerge(List<Triangle> triangles) {
        foreach (Triangle a in triangles) {
            foreach (Triangle b in triangles) {
            if (a.isNeighbor(b)) return false;
            }
        }
        return true;
    }

    */

}

public class Quad : Shape
{
    public Vertex a;
    public Vertex b;
    public Vertex c;
    public Vertex d;

    public Edge ab;
    public Edge bc;
    public Edge cd;
    public Edge ad;


    public Quad(Vertex a, Vertex b, Vertex c, Vertex d, List<Edge> edges, List<Quad> quads) {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;

        this.ab = Edge.FindEdge(a,b,edges);
        this.bc = Edge.FindEdge(b,c,edges);
        this.cd = Edge.FindEdge(d,c,edges);
        this.ad = Edge.FindEdge(a,d,edges);

        this.vertices = new Vertex[]{a,b,c,d};
        this.edges = new Edge[]{ab,bc,cd,ad};
        quads.Add(this);
        this.edges = new Edge[]{ab, bc, cd,ad};
        this.center = Vertex.MidPoint(this.vertices);

        ReadNeighbors();
    }

    public void Relax() {
        Vector3 newA = (a.localPos+
        (Quaternion.AngleAxis(-90,Vector3.up)*(b.localPos - center.localPos) + center.localPos)+
        (Quaternion.AngleAxis(-180,Vector3.up)*(c.localPos - center.localPos) + center.localPos)+
        (Quaternion.AngleAxis(-270,Vector3.up)*(d.localPos - center.localPos) + center.localPos))/4;

        Vector3 newB = Quaternion.AngleAxis(90,Vector3.up)*(newA - center.localPos) + center.localPos;
        Vector3 newC = Quaternion.AngleAxis(180,Vector3.up)*(newA - center.localPos) + center.localPos;
        Vector3 newD = Quaternion.AngleAxis(270,Vector3.up)*(newA - center.localPos) + center.localPos;

        a.localPos += (newA-a.localPos)*0.1f;
        b.localPos += (newB-b.localPos)*0.1f;
        c.localPos += (newC-c.localPos)*0.1f;
        d.localPos += (newD-d.localPos)*0.1f;
    }

}
