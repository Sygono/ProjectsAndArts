using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCoord
{
    public int q;
    public int r;
    public int s;

    public CubeCoord(int q = 0, int r = 0, int s = 0) {
        this.q = q;
        this.r = r;
        this.s = s;
    }

    public Vector3 CoordPos(float scale = 1f) {
        return new Vector3(q * Mathf.Sqrt(3) / 2, 0, -(float)r -(float)q/2)*2*scale;
    }

    public static CubeCoord[] directions = new CubeCoord[] {
        new CubeCoord(0, 1, -1),
        new CubeCoord(-1, 1, 0),
        new CubeCoord(-1, 0, 1),
        new CubeCoord(0, -1, 1),
        new CubeCoord(1, -1, 0),
        new CubeCoord(1, 0, -1)
    };

    public static CubeCoord Direction(int index) {
        return CubeCoord.directions[index];
    }

    public CubeCoord Add(CubeCoord coord) {
        return new CubeCoord(q + coord.q, r + coord.r, s + coord.s);
    }

    public CubeCoord Neighbor(int index) {
        return Add(Direction(index));
    }

    public CubeCoord Scale(int n) {
        return new CubeCoord(q*n, r*n, s*n);
    }

    public static List<CubeCoord> CoordRing(int radius) {
        List<CubeCoord> result = new List<CubeCoord>();
        if (radius == 0) {
            result.Add(new CubeCoord(0,0,0));
        } else {
            CubeCoord coord = CubeCoord.Direction(4).Scale(radius);
            for (int i=0; i<6; i++) {
                for (int j=0; j<radius; j++) {
                    result.Add(coord);
                    coord = coord.Neighbor(i);
                }
            }
        }
        return result;
    }

    public static List<CubeCoord> CoordHex(int radius) {

        List<CubeCoord> result = new List<CubeCoord>();
        for (int i=0; i<=radius; i++) {
            result.AddRange(CoordRing(i));
        }
        return result;
    }
}


public class Vertex {
    public Vector3 localPos;
    public Vertex() {
    }

    public static Vertex MidPoint(Vertex[] vertices) {
        Vector3 result = Vector3.zero;
        int count = 0;
        foreach(Vertex vertex in vertices) {
            result += vertex.localPos;
            count += 1;
        }
        if (count ==0) return null;
        result.x = result.x/count;
        result.y = result.y/count;
        result.z = result.z/count;
        Vertex mid = new Vertex();
        mid.localPos = result;
        return mid;
    }
}

public class VertexHex : Vertex {
    public CubeCoord coord;

    public VertexHex(CubeCoord coord, float scale = 1f) {
        this.coord = coord;
        this.localPos = coord.CoordPos(scale); 
    }

    public static void Hex(List<VertexHex> vertices, int radius, float cellSize) {
        foreach(CubeCoord coord in CubeCoord.CoordHex(radius)) {
            vertices.Add(new VertexHex(coord, cellSize));
        }
    }

    public static List<VertexHex> GetRing(int radius, List<VertexHex> vertices) {
        if (radius == 0) return vertices.GetRange(0,1);
        return vertices.GetRange(radius * (radius-1) * 3 + 1, radius * 6);
    }


}