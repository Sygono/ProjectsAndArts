using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public class NodeGraphVisualizer : MonoBehaviour
{   
    public NodeGraphPlayer player;
    public SpriteLoader spriteLoader;
    public GameObject nodeButtonPrefab;
    public GameObject linePrefab;
    public GameObject slotPrefab;
    public NodeGraph nodeGraph;
    public float lineWidth = 1.0f;
    public float offset = 20f;

    private ConnectionLine currentLine;
    private List<Slot> slots = new List<Slot>();
    private int startNode = -1;
    private int endNode = -1;

    // Other visualizer and interaction logic...

    public void Update() {
        if (Input.GetMouseButtonDown(0)) {
            if (currentLine!=null) {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                bool hasSlot = FindClickedSlot(mousePos);
                if (!hasSlot) {
                    EndBuildConnection(true);
                }
            }
        }
        if (currentLine != null) {
            GameObject obj = currentLine.gameObject;
            Vector2 mousePos = Input.mousePosition;
            Vector2 v0 = new Vector2(0,0);
            Vector2 v1 = new Vector2(0,0);
            if (startNode == -1) {
                v0 = mousePos;
                v1 = nodeGraph.GetNodePosition(endNode)-new Vector2(offset, 0);
                Vector2 normal = (v1 - v0).normalized;
                v0 += normal;
                SetLineTransform(obj, v0, v1);
            } else {
                v0 = nodeGraph.GetNodePosition(startNode)+new Vector2(offset, 0);
                v1 = mousePos;
                Vector2 normal = (v1 - v0).normalized;
                v1 -= normal;
                SetLineTransform(obj, v0, v1);
            }
        }
    }

    public void StartVisualization() {
        nodeGraph = player.nodeGraph;
        for(int i=0; i<nodeGraph.nodes.Count; i++) {
            VisualizeNode(i);
            VisualizeSlots(i);
        }
        for(int i=0; i<nodeGraph.nodeToChildIndices.Count; i++) {
            foreach(int j in nodeGraph.nodeToChildIndices[i]) {
                VisualizeConnection(i, j);
            }
        }
    }

    public void VisualizeNode(int index) {
        GameObject button = Instantiate(nodeButtonPrefab, transform);
        button.transform.position = nodeGraph.GetNodePosition(index);
        NodeButton node = button.GetComponent<NodeButton>();
        node.Initialize(index, null, DetailedNode);
    }

    public void VisualizeSlots(int index) {
        Vector2 v0 = nodeGraph.GetNodePosition(index);
        bool b = true;
        for (int i=0; i<2; i++) {
            GameObject button = Instantiate(slotPrefab, transform);
            button.transform.position = v0 + new Vector2(offset * ((b)?1:-1), 0);
            Slot slot = button.GetComponent<Slot>();
            slot.Initialize(b, index, TryBuildConnection);
            slots.Add(slot);
            b = false;
        }
    }

    public void VisualizeConnection(int start, int end) {
        Vector2 v0 = nodeGraph.GetNodePosition(start);
        Vector2 v1 = nodeGraph.GetNodePosition(end);
        v0.x += offset;
        v1.x -= offset;

        GameObject line = Instantiate(linePrefab, transform);
        ConnectionLine connection = line.GetComponent<ConnectionLine>();
        int parentNodeIndex = start;// Set the parent node index.
        int childNodeIndex = end;  // Set the child node index.
        connection.Initialize(parentNodeIndex, childNodeIndex, HandleConnectionLineClick);
        SetLineTransform(line, v0, v1);
    }

    private void SetLineTransform(GameObject line, Vector2 start, Vector2 end) {
        Vector2 direction = end - start;
        line.transform.position = start + direction * 0.5f;
        line.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
        line.GetComponent<RectTransform>().sizeDelta = new Vector2(direction.magnitude, lineWidth);
    }

    private void DetailedNode(int index) {

    }

    private void HandleConnectionLineClick(int parentNodeIndex, int childNodeIndex)
    {
        // Handle the click on the connection line and disconnect nodes in the NodeGraph.
        nodeGraph.Disconnect(parentNodeIndex, childNodeIndex);
    }

    private void TryBuildConnection(bool starter, int index)
    {
        if (startNode==-1&&endNode==-1) {
            GameObject line = Instantiate(linePrefab, transform);
            currentLine = line.GetComponent<ConnectionLine>();
            if (starter) {
                startNode = index;
            } else {
                endNode = index;
            }
            foreach (Slot slot in slots) {
                if (slot.starter==starter||slot.index==index) {
                    slot.button.interactable = false;
                }
            }
            return;
        } else if (starter&&startNode==-1) {
            startNode = index;
        } else if (!starter&&endNode==-1) {
            endNode = index;
        } else {
            return;
        }
        nodeGraph.Connect(startNode, endNode);
        EndBuildConnection(false);
    }

    private void EndBuildConnection(bool destroy) {
        if (destroy) {
            Destroy(currentLine.gameObject);
        } else {
            Vector2 v1 = nodeGraph.GetNodePosition(endNode)-new Vector2(offset, 0);
            Vector2 v0 = nodeGraph.GetNodePosition(startNode)+new Vector2(offset, 0);
            SetLineTransform(currentLine.gameObject, v0, v1);
            currentLine.Initialize(startNode, endNode, HandleConnectionLineClick);
        }
        foreach (Slot slot in slots) {
            slot.button.interactable = true;
        }
        currentLine = null;
        startNode=-1;
        endNode=-1;
    }

    private bool FindClickedSlot(Vector2 mousePos)
    {
        foreach (Slot slot in slots)
        {
            Vector2 slotScreenPos = RectTransformUtility.WorldToScreenPoint(null, slot.transform.position);
            float distance = Vector2.Distance(slotScreenPos, Input.mousePosition);

            // Check if the distance is within the click radius and if the slot is active
            if (distance <= 10f && slot.gameObject.activeSelf && slot.button.interactable)
            {
                return true; // Return the clicked slot
            }
        }

        return false; // No slot was clicked
    }

    public void EndVisualization()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        slots = new List<Slot>();
        currentLine = null;
        startNode=-1;
        endNode=-1;
    }
}

[CustomEditor(typeof(NodeGraphVisualizer))] // Replace with the actual ScriptableObject type.
public class VisualizeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NodeGraphVisualizer visualizer = (NodeGraphVisualizer)target;
        if (GUILayout.Button("Start Visualization"))
        {
            // Call the method to start visualization.
            visualizer.StartVisualization();
        }
        if (GUILayout.Button("End Visualization"))
        {
            // Call the method to start visualization.
            visualizer.EndVisualization();
        }
        DrawDefaultInspector();
    }
}
