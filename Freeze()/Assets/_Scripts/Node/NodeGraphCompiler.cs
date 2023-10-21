using System.Collections.Generic;
using UnityEngine;

public class NodeGraphCompiler
{
    public static NodeGraph Compile(NodeGraphSO nodeGraphSO, NodeGraphPlayer nodeGraphPlayer)
    {
        NodeGraph nodeGraph = new NodeGraph();

        foreach (NodeData nodeData in nodeGraphSO.nodeDataList)
        {
            Node node;

            // Use the enum to create the corresponding node type.
            switch (nodeData.nodeType)
            {
                case NodeType.And:
                    node = new AndNode();
                    break;
                case NodeType.Not:
                    node = new NotNode();
                    break;
                case NodeType.Function:
                    node = new FunctionNode(nodeGraphPlayer.actions[nodeData.functionName]);
                    break;
                case NodeType.Condition:
                    node = new ConditionNode(nodeGraphPlayer.conditions[nodeData.functionName]);
                    break;
                case NodeType.True:
                    node = new TrueNode();
                    break;
                case NodeType.Move:
                    node = new MoveNode(nodeGraphPlayer.actions["Move"]);
                    break;
                case NodeType.Chase:
                    node = new ChaseNode(nodeGraphPlayer.actions["Chase"]);
                    break;
                case NodeType.Retreat:
                    node = new RetreatNode(nodeGraphPlayer.actions["Retreat"]);
                    break;
                case NodeType.Stay:
                    node = new StayNode(nodeGraphPlayer.actions["Stay"]);
                    break;
                case NodeType.Attack:
                    node = new AttackNode(nodeGraphPlayer.actions["Attack"]);
                    break;
                case NodeType.StopAttack:
                    node = new StopAttackNode(nodeGraphPlayer.actions["StopAttack"]);
                    break;
                case NodeType.SendSignal:
                    node = new SendSignalNode(nodeGraphPlayer.actions["SendSignal"]);
                    break;
                case NodeType.CheckDistance:
                    node = new CheckDistanceNode(nodeGraphPlayer.conditions["CheckDistance"]);
                    break;
                case NodeType.CheckHp:
                    node = new CheckHpNode(nodeGraphPlayer.conditions["CheckHp"]);
                    break;
                case NodeType.CheckSignal:
                    node = new CheckSignalNode(nodeGraphPlayer.conditions["CheckSignal"]);
                    break;
                case NodeType.CheckAlly:
                    node = new CheckAllyNode(nodeGraphPlayer.conditions["CheckAlly"]);
                    break;
                case NodeType.CheckEnemy:
                    node = new CheckEnemyNode(nodeGraphPlayer.conditions["CheckEnemy"]);
                    break;
                // Add more cases for other node types as needed.
                default:
                    // Handle other node types as needed.
                    node = new Node();
                    break;
            }

            node.ai = nodeData.ai; // Set additional parameters.
            node.nodeGraph = nodeGraph;
            nodeGraph.AddNode(node, nodeData.position);
        }

        foreach (NodeChildIndices nodeChildIndices in nodeGraphSO.nodeToChildIndices) {
            foreach (int childIndex in nodeChildIndices.childIndices) {
                nodeGraph.Connect(nodeChildIndices.nodeIndex, childIndex);
            }
        }
        return nodeGraph;
    }
}
