using UnityEngine;
using System;
using System.Collections.Generic;

public class NodeGraphPlayer : MonoBehaviour
{
    // You may have an array or list of condition nodes.
    public NodeGraphSO nodeGraphSO;
    public NodeGraph nodeGraph;
    public Dictionary<string, Func<int[], bool>> conditions = new Dictionary<string, Func<int[], bool>>();
    public Dictionary<string, System.Action<int[]>> actions = new Dictionary<string, System.Action<int[]>>();

    protected virtual void Awake() {

    }

    protected virtual void Start() {
        nodeGraph = NodeGraphCompiler.Compile(nodeGraphSO, this);
    }

    // This is where you would perform your custom traversal logic.
    protected void UpdateConditionNodes()
    {
        foreach (ConditionNode conditionNode in nodeGraph.conditionNodes)
        {
            // Perform traversal or processing on each condition node.
            conditionNode.Traverse();
        }
    }

    protected void CallFunctionNodes()
    {
        foreach (FunctionNode functionNode in nodeGraph.functionNodes)
        {
            if (!functionNode.isActive) continue;
            functionNode.ExecuteFunction();
        }
    }

    // You can choose the appropriate Unity event function to call your traversal logic.
    // For example, you could call TraverseNodes in Update if you want it to run every frame.
    protected virtual void Update()
    {
        UpdateConditionNodes();
        CallFunctionNodes();
    }
}
