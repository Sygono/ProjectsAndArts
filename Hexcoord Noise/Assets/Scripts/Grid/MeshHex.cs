using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(GridGenerator))]
public class MeshHex : MonoBehaviour
{
    Mesh mesh;

    GridHex grid;

    List<Vector3> vertices = new List<Vector3>();

    int[] triangles;

    public void CreateMesh() {
        mesh = new Mesh();
        grid = GetComponent<GridGenerator>().grid;
        triangles = new int[grid.subQuads.Count*2*3];
        for(int i=0; i<grid.subQuads.Count; i++) {
            Quad quad = grid.subQuads[i];
            foreach (Vertex vertex in quad.vertices) {
                if (!vertices.Contains(vertex.localPos)) {vertices.Add(vertex.localPos);}
            }
            int n = i*2*3;
            triangles[n+0] = vertices.IndexOf(quad.vertices[0].localPos);
            triangles[n+1] = vertices.IndexOf(quad.vertices[1].localPos);
            triangles[n+2] = vertices.IndexOf(quad.vertices[3].localPos);

            triangles[n+3] = vertices.IndexOf(quad.vertices[1].localPos);
            triangles[n+4] = vertices.IndexOf(quad.vertices[2].localPos);
            triangles[n+5] = vertices.IndexOf(quad.vertices[3].localPos);

        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MeshHex))]
public class MeshHexEditor : Editor {

	public override void OnInspectorGUI () {
		MeshHex meshHex = (MeshHex)target;
        if (GUILayout.Button("Setup")) {  
			meshHex.CreateMesh();
            SceneView.RepaintAll();
		}

		DrawDefaultInspector();
    }
}
#endif
