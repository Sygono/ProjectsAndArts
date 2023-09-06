using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridGenerator : MonoBehaviour
{
    public int radius = 6;

    public float cellSize = 1f;

    public int layers = 1;

    public GridHex grid;

    public bool procedural;
    int gizmosIndex;

    public void OnDrawGizmos(){
		Gizmos.color = Color.white;
        Handles.color = Color.blue;
		Gizmos.matrix = transform.localToWorldMatrix;
        Handles.matrix = transform.localToWorldMatrix;
        if (grid != null) {       
            int number = 0;
            foreach (VertexHex vertex in grid.vertices) {
                Handles.color = Color.blue;
                Handles.Label(vertex.localPos, number.ToString());
                number++;
            }

            foreach (Quad quad in grid.subQuads) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(quad.a.localPos, quad.b.localPos);
                Gizmos.DrawLine(quad.b.localPos, quad.c.localPos);
                Gizmos.DrawLine(quad.c.localPos, quad.d.localPos);
                Gizmos.DrawLine(quad.d.localPos, quad.a.localPos);
            }

            Debug.Log(grid.triangles.Count);
            Debug.Log(grid.quads.Count);
            Debug.Log(grid.edges.Count);
        }
	}

    

    void Awake() {
        CreateGrid();
    }

    public void CreateGrid() {
        grid = new GridHex(radius, cellSize, layers);
    }

    public void Dissolve() {
        grid.RandomDissolve();
    }

    public void Subdivide() {
        grid.Subdivide();
    }

    public void Relax(int iterations = 100) {
        grid.Relax(iterations);
    }

    
}

#if UNITY_EDITOR
[CustomEditor(typeof(GridGenerator))]
public class GridEditor : Editor {

	public override void OnInspectorGUI () {
		GridGenerator grid = (GridGenerator)target;
        if (GUILayout.Button("Setup")) {  
			grid.CreateGrid();
            grid.Dissolve();
            grid.Subdivide();
            grid.Relax();
            SceneView.RepaintAll();
		}

        if (GUILayout.Button("Relax")) {  
            grid.Relax(5);
            SceneView.RepaintAll();
		}

		DrawDefaultInspector();
    }
}
#endif
