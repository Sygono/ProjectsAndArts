using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[RequireComponent(typeof(GridGenerator))]
public class NoiseHex : MonoBehaviour
{
    GridHex grid;

    public float highest;
    public float lowest;

    public float xOrg;
    public float yOrg;

    public float scale = 1.0F;

    public float maxHeight = 1f;

    public void CalculateNoise() {
        GridGenerator generator = GetComponent<GridGenerator>();
        grid = generator.grid;
        if (grid == null) return;
        int index = 0;
        highest = -1f;
        lowest = 9999f;
        for (float i = 0; i<grid.radius; i++) {
            float xCoord = xOrg + i / grid.radius * scale;
            List<VertexHex> ring = VertexHex.GetRing((int)i, grid.vertices);
            for (float j = 0; j<ring.Count; j++) {
                float yCoord = yOrg + j / ring.Count * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                grid.vertices[index].localPos = new Vector3(grid.vertices[index].localPos.x,sample*maxHeight,grid.vertices[index].localPos.z);
                index++;
                highest = Mathf.Max(highest, grid.vertices[index].localPos.y);
                lowest = Mathf.Min(lowest, grid.vertices[index].localPos.y);
            }
        }
        
        foreach (Edge edge in grid.edges) {
            Vertex[] sides = edge.vertices.ToArray();
            Vector3 midPos = (sides[0].localPos + sides[1].localPos)/2;
            edge.midPoint.localPos = new Vector3(edge.midPoint.localPos.x, midPos.y, edge.midPoint.localPos.z);
        }

        foreach (Triangle shape in grid.triangles) {
            Vertex[] sides = shape.vertices;
            Vector3 midPos = (sides[0].localPos + sides[1].localPos + sides[2].localPos)/3;
            shape.center.localPos = new Vector3(shape.center.localPos.x, midPos.y, shape.center.localPos.z);
        }

        foreach (Quad shape in grid.quads) {
            Vertex[] sides = shape.vertices;
            Vector3 midPos = (sides[0].localPos + sides[1].localPos + sides[2].localPos + sides[3].localPos)/4;
            shape.center.localPos = new Vector3(shape.center.localPos.x, midPos.y, shape.center.localPos.z);
        }
    }

    void OnValidate() {
        CalculateNoise();
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(NoiseHex))]
public class NoiseEditor : Editor {

	public override void OnInspectorGUI () {
		NoiseHex noise = (NoiseHex)target;
        if (GUILayout.Button("Setup")) {  
			noise.CalculateNoise();
            SceneView.RepaintAll();
		}

		DrawDefaultInspector();
    }
}
#endif

