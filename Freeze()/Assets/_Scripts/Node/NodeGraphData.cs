using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;


[CreateAssetMenu(fileName = "NewNodeGraph", menuName = "Node Graph")]
public class NodeGraphSO : ScriptableObject
{
    public List<NodeData> nodeDataList = new List<NodeData>();
    // Serialized list of node-to-child indices
    public List<NodeChildIndices> nodeToChildIndices = new List<NodeChildIndices>();
}

[Serializable]
public class NodeChildIndices
{
    public int nodeIndex;
    public List<int> childIndices;

    public NodeChildIndices(int index)
    {
        nodeIndex = index;
        childIndices = new List<int>();
    }
}

public enum NodeType
{
    Node,
    And,
    Not,
    Function,
    Condition,
    True,
    Move,        
    Chase,       
    Retreat,     
    Stay,        
    Attack,      
    StopAttack,  
    SendSignal,  
    CheckDistance,
    CheckHp,      
    CheckSignal,  
    CheckAlly,    
    CheckEnemy    
}


[System.Serializable]
public class NodeData
{
    public NodeType nodeType; // Store the type of the node.
    public Vector2 position;  // Store the position of the node on the graph canvas.
    public int[] ai;          // Additional parameters if needed.

    // Store the function name or identifier for FunctionNode.
    public string functionName;
}


[CustomEditor(typeof(NodeGraphSO))] // Replace with the actual ScriptableObject type.
public class NodeDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NodeGraphSO so = (NodeGraphSO)target;

        for (int i = 0; i < so.nodeDataList.Count; i++)
        {
            NodeData nodeData = so.nodeDataList[i];
            if (nodeData.nodeType == NodeType.Move)
            {
                nodeData.functionName = "Move";
            }
            else if (nodeData.nodeType == NodeType.Chase)
            {
                nodeData.functionName = "Chase";
            }
            else if (nodeData.nodeType == NodeType.Retreat)
            {
                nodeData.functionName = "Retreat";
            }
            else if (nodeData.nodeType == NodeType.Stay)
            {
                nodeData.functionName = "Stay";
            }
            else if (nodeData.nodeType == NodeType.Attack)
            {
                nodeData.functionName = "Attack";
            }
            else if (nodeData.nodeType == NodeType.StopAttack)
            {
                nodeData.functionName = "StopAttack";
            }
            else if (nodeData.nodeType == NodeType.SendSignal)
            {
                nodeData.functionName = "SendSignal";
            }
            else if (nodeData.nodeType == NodeType.CheckDistance)
            {
                nodeData.functionName = "CheckDistance";
            }
            else if (nodeData.nodeType == NodeType.CheckHp)
            {
                nodeData.functionName = "CheckHp";
            }
            else if (nodeData.nodeType == NodeType.CheckSignal)
            {
                nodeData.functionName = "CheckSignal";
            }
            else if (nodeData.nodeType == NodeType.CheckAlly)
            {
                nodeData.functionName = "CheckAlly";
            }
            else if (nodeData.nodeType == NodeType.CheckEnemy)
            {
                nodeData.functionName = "CheckEnemy";
            }


        }
        DrawDefaultInspector();
    }
}







