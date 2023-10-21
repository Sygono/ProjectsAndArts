using System.Collections.Generic;
using System;
using UnityEngine;

public class Node
{
    public int[] ai;

    public bool isActive = false; // A flag to determine if the node is active.
    public NodeGraph nodeGraph;

    public Node() {}

    public virtual bool Evaluate()
    {
        // An OR node evaluates to true if at least one of its parents is active.
        foreach (int parentIndex in nodeGraph.GetParentNodes(this))
        {
            Node parentNode = nodeGraph.GetNode(parentIndex);
            if (parentNode.isActive)
            {
                return true;
            }
        }
        return false;
    }

    public void Traverse()
    {
        bool evaluate = Evaluate();
        if (evaluate==isActive) return;
        isActive = evaluate;
        foreach (int childIndex in nodeGraph.GetChildNodes(this))
        {
            nodeGraph.GetNode(childIndex).Traverse(); // Evaluate child nodes.
        }
    }
}

public class AndNode : Node
{
    public override bool Evaluate()
    {
        List<int> parents = nodeGraph.GetParentNodes(this);
        if (parents.Count==0) return false;
        foreach (int parentIndex in parents) {
            Node parentNode = nodeGraph.GetNode(parentIndex);
            if (!parentNode.isActive)
            {
                return false;
            }
        }
        return true;
    }
}

public class NotNode : Node
{

    public override bool Evaluate()
    {
        List<int> parents = nodeGraph.GetParentNodes(this);
        if (parents.Count==0) return false;
        foreach (int parentIndex in parents) {
            Node parentNode = nodeGraph.GetNode(parentIndex);
            if (!parentNode.isActive)
            {
                return false;
            }
        }
        return true;
    }
}

public class FunctionNode : Node
{
    public System.Action<int[]> FunctionToCall;

    public FunctionNode(System.Action<int[]> function = null) : base()
    {
        FunctionToCall = function;
    }

    public void ExecuteFunction()
    {
        if (!isActive) return;
        // Call the associated function with the parameter
        FunctionToCall?.Invoke(ai);
    }
}

public class ConditionNode : Node
{
    public Func<int[], bool> FunctionToCall;

    public ConditionNode(Func<int[], bool> function = null) : base()
    {
        FunctionToCall = function;
    }

    public override bool Evaluate()
    {
        // Implement your custom condition-checking logic here.
        if (FunctionToCall == null)
        {
            return false; // Node is not active or the function is not set.
        }
        // Call the function and return the result.
        return FunctionToCall(ai);
    }
}


public class TrueNode : ConditionNode
{
    public TrueNode(Func<int[], bool> function = null) : base(function) {}

    public override bool Evaluate()
    {
        return true;
    }
}

public class MoveNode : FunctionNode
{
    public MoveNode(System.Action<int[]> function) : base(function){}
}

public class CheckDistanceNode : ConditionNode
{
    public CheckDistanceNode(Func<int[], bool> function) : base(function){}
}

public class ChaseNode : FunctionNode
{
    public ChaseNode(System.Action<int[]> function) : base(function) { }
}

public class RetreatNode : FunctionNode
{
    public RetreatNode(System.Action<int[]> function) : base(function) { }
}

public class StayNode : FunctionNode
{
    public StayNode(System.Action<int[]> function) : base(function) { }
}

public class AttackNode : FunctionNode
{
    public AttackNode(System.Action<int[]> function) : base(function) { }
}

public class StopAttackNode : FunctionNode
{
    public StopAttackNode(System.Action<int[]> function) : base(function) { }
}

public class SendSignalNode : FunctionNode
{
    public SendSignalNode(System.Action<int[]> function) : base(function) { }
}

public class CheckHpNode : ConditionNode
{
    public CheckHpNode(Func<int[], bool> function) : base(function) { }
}

public class CheckSignalNode : ConditionNode
{
    public CheckSignalNode(Func<int[], bool> function) : base(function) { }
}

public class CheckAllyNode : ConditionNode
{
    public CheckAllyNode(Func<int[], bool> function) : base(function) { }
}

public class CheckEnemyNode : ConditionNode
{
    public CheckEnemyNode(Func<int[], bool> function) : base(function) { }
}












