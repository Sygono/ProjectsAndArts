using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class SearchProblem
{
    public abstract object GetStartState();

    // Returns true if and only if the state is a valid goal state.
    public abstract bool IsGoalState(object state);

    // For a given state, this should return a list of triples (successor, action, stepCost).
    public abstract List<Tuple<object, object, float>> GetSuccessors(object state);

    // public abstract int GetCostOfState(object state);

    // Returns the total cost of a particular sequence of actions.
    public abstract float GetCostOfTraverse(object start, object end);

    //public abstract int costFn(object state);
    public static List<object> DepthFirstSearch(SearchProblem problem)
    {
        Stack<Tuple<object, List<object>>> states = new Stack<Tuple<object, List<object>>>();
        states.Push(new Tuple<object, List<object>>(problem.GetStartState(), new List<object>()));

        List<object> visited = new List<object>();

        while (states.Count > 0)
        {
            var stateTuple = states.Pop();
            object currentNode = stateTuple.Item1;
            List<object> totalAction = stateTuple.Item2;

            if (problem.IsGoalState(currentNode))
            {
                //Console.WriteLine(string.Join(", ", totalAction));
                return totalAction;
            }

            if (!visited.Contains(currentNode))
            {
                visited.Add(currentNode);

                foreach (var successor in problem.GetSuccessors(currentNode))
                {
                    object node = successor.Item1;
                    object action = successor.Item2;
                    states.Push(new Tuple<object, List<object>>(node, new List<object>(totalAction) { action }));
                }
            }
        }

        return new List<object>();
    }

    public static List<object> BreadthFirstSearch(SearchProblem problem)
    {   
        Queue<Tuple<object, List<object>>> states = new Queue<Tuple<object, List<object>>>();
        states.Enqueue(new Tuple<object, List<object>>(problem.GetStartState(), new List<object>()));

        List<object> visited = new List<object>();

        while (states.Count > 0)
        {
            var stateTuple = states.Dequeue();
            object currentNode = stateTuple.Item1;
            List<object> totalAction = stateTuple.Item2;

            if (problem.IsGoalState(currentNode))
            {
                Console.WriteLine(string.Join(", ", totalAction));
                return totalAction;
            }

            if (!visited.Contains(currentNode))
            {
                visited.Add(currentNode);

                foreach (var successor in problem.GetSuccessors(currentNode))
                {
                    object node = successor.Item1;
                    object action = successor.Item2;

                    states.Enqueue(new Tuple<object, List<object>>(node, new List<object>(totalAction) { action }));
                }
            }
        }

        return new List<object>();
    }

    public static List<object> UniformCostSearch(SearchProblem problem)
    {
        Dictionary<object, float> dict = new Dictionary<object, float>();
        PriorityQueue<Tuple<object, List<object>>> states = new PriorityQueue<Tuple<object, List<object>>>();

        dict[problem.GetStartState()] = 0;
        states.Enqueue(new Tuple<object, List<object>>(problem.GetStartState(), new List<object>()), 0);


        List<object> visited = new List<object>();

        while (!states.IsEmpty())
        {
            var stateTuple = states.Dequeue();
            object currentNode = stateTuple.Item1;
            List<object> totalAction = stateTuple.Item2;

            if (problem.IsGoalState(currentNode))
            {
                //Console.WriteLine(string.Join(", ", totalAction));
                return totalAction;
            }

            if (!visited.Contains(currentNode))
            {
                visited.Add(currentNode);

                foreach (var successor in problem.GetSuccessors(currentNode))
                {
                    object node = successor.Item1;
                    object action = successor.Item2;
                    float cost = successor.Item3;

                    dict[node] = dict[currentNode] + cost;
                    states.Enqueue(new Tuple<object, List<object>>(node, new List<object>(totalAction) { action }), (int)dict[node]);
                }
            }
        }

        return new List<object>();
    }
}

public class TriangleSearch : SearchProblem {
    TriangleGrid trig;
    Triangle start;
    Triangle end;

    public TriangleSearch(TriangleGrid trig, Triangle start, Triangle end) {
        this.trig = trig;
        this.start = start;
        this.end = end;
    }

    public override object GetStartState() {
        return start;
    }

    public override bool IsGoalState(object state) {
        return (Triangle)state==end;
    }

    public override float GetCostOfTraverse(object start, object end) {
        return Vector2.Distance(((Triangle)start).GetCenter(), ((Triangle)end).GetCenter());
    }

    public override List<Tuple<object, object, float>> GetSuccessors(object state) {
        Triangle trig = (Triangle)state;
        List<Tuple<object, object, float>> lst = new List<Tuple<object, object, float>>();
        for (int i=0; i<trig.Edges.Count; i++) {
            Triangle neighbor = trig.neighbors[i];
            if (neighbor!=null) {
                lst.Add(new Tuple<object, object, float>(neighbor, trig.Edges[i], GetCostOfTraverse(state, neighbor)));
            }
        }
        return lst;
    }
}
