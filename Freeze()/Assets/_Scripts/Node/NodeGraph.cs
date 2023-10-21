using System.Collections.Generic;
using UnityEngine;

public class NodeGraph
{
    public List<Node> nodes = new List<Node>();
    public List<List<int>> nodeToChildIndices = new List<List<int>>();
    public List<Vector2> buttonPositions = new List<Vector2>();
    public List<ConditionNode> conditionNodes = new List<ConditionNode>(); // List of condition nodes.
    public List<FunctionNode> functionNodes = new List<FunctionNode>();

    public void AddNode(Node node, Vector2 position)
    {
        nodes.Add(node);
        node.nodeGraph = this;
        nodeToChildIndices.Add(new List<int>());

        // Store the position of the node.
        buttonPositions.Add(position);

        if (node is ConditionNode || node.GetType().IsSubclassOf(typeof(ConditionNode)))
        {
            conditionNodes.Add((ConditionNode)node);
        }
        if (node is FunctionNode || node.GetType().IsSubclassOf(typeof(FunctionNode)))
        {
            functionNodes.Add((FunctionNode)node);
        }
    }

    public void Connect(int parentIndex, int childIndex)
    {
        if (parentIndex < 0 || parentIndex >= nodes.Count || childIndex < 0 || childIndex >= nodes.Count)
        {
            // Check for valid indices to avoid out-of-range errors.
            Debug.LogWarning("Invalid parent or child indices.");
            return;
        }
        if (!nodeToChildIndices[parentIndex].Contains(childIndex)) {
            nodeToChildIndices[parentIndex].Add(childIndex);
            Debug.Log("Connected: Parent Index " + parentIndex + " to Child Index " + childIndex);
        }
        nodes[childIndex].Traverse();
    }

    public void Disconnect(int parentIndex, int childIndex)
    {
        if (parentIndex < 0 || parentIndex >= nodes.Count || childIndex < 0 || childIndex >= nodes.Count)
        {
            // Check for valid indices to avoid out-of-range errors.
            Debug.LogWarning("Invalid parent or child indices.");
            return;
        }
        if (nodeToChildIndices[parentIndex].Contains(childIndex)) {
            nodeToChildIndices[parentIndex].Remove(childIndex);
            Debug.Log("Disconnected: Parent Index " + parentIndex + " from Child Index " + childIndex);
        }
        nodes[childIndex].Traverse();
    }


    public List<int> GetChildNodes(Node targetNode)
    {
        if (!nodes.Contains(targetNode)) {
            // Check for valid indices to avoid out-of-range errors.
            Debug.LogWarning("Invalid parent indices.");
            return null;
        }
        int parentIndex = nodes.IndexOf(targetNode);
        return nodeToChildIndices[parentIndex];
    }

    public List<int> GetParentNodes(Node targetNode)
    {
        if (!nodes.Contains(targetNode)) {
            // Check for valid indices to avoid out-of-range errors.
            Debug.LogWarning("Invalid parent indices.");
            return null;
        }
        List<int> parentIndices = new List<int>();
        foreach (List<int> lst in nodeToChildIndices)
        {
            if (lst.Contains(nodes.IndexOf(targetNode)))
            {
                parentIndices.Add(nodeToChildIndices.IndexOf(lst));
            }
        }

        return parentIndices;
    }

    public Node GetNode(int index) {
        return nodes[index];
    }

    public Vector2 GetNodePosition(int index) {
        return buttonPositions[index];
    }


    // You can add other methods for managing the node graph as needed.
}
