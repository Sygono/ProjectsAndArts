using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class SearchProblem
{
    // Returns the start state for the search problem.
    public abstract Vector3Int GetStartState();

    // Returns true if and only if the state is a valid goal state.
    public abstract bool IsGoalState(Vector3Int node);

    // For a given state, this should return a list of triples (successor, action, stepCost).
    public abstract List<Tuple<Vector3Int, Direction, int>> GetSuccessors(Vector3Int node);

    // public abstract int GetCostOfState(Vector3Int state);

    // Returns the total cost of a particular sequence of actions.
    public abstract int GetCostOfActions(List<Direction> actions);

    public abstract int costFn(Vector3Int pos);

    public static List<Direction> DepthFirstSearch(SearchProblem problem)
    {
        Stack<Tuple<Vector3Int, List<Direction>>> states = new Stack<Tuple<Vector3Int, List<Direction>>>();
        states.Push(new Tuple<Vector3Int, List<Direction>>(problem.GetStartState(), new List<Direction>()));

        List<Vector3Int> visited = new List<Vector3Int>();

        while (states.Count > 0)
        {
            var stateTuple = states.Pop();
            Vector3Int currentNode = stateTuple.Item1;
            List<Direction> totalAction = stateTuple.Item2;

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
                    Vector3Int node = successor.Item1;
                    Direction action = successor.Item2;
                    states.Push(new Tuple<Vector3Int, List<Direction>>(node, new List<Direction>(totalAction) { action }));
                }
            }
        }

        return new List<Direction>();
    }

    public static List<Direction> BreadthFirstSearch(SearchProblem problem)
    {   
        Queue<Tuple<Vector3Int, List<Direction>>> states = new Queue<Tuple<Vector3Int, List<Direction>>>();
        states.Enqueue(new Tuple<Vector3Int, List<Direction>>(problem.GetStartState(), new List<Direction>()));

        List<Vector3Int> visited = new List<Vector3Int>();

        while (states.Count > 0)
        {
            var stateTuple = states.Dequeue();
            Vector3Int currentNode = stateTuple.Item1;
            List<Direction> totalAction = stateTuple.Item2;

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
                    Vector3Int node = successor.Item1;
                    Direction action = successor.Item2;

                    states.Enqueue(new Tuple<Vector3Int, List<Direction>>(node, new List<Direction>(totalAction) { action }));
                }
            }
        }

        return new List<Direction>();
    }

    public static List<Direction> UniformCostSearch(SearchProblem problem)
    {
        Dictionary<Vector3Int, int> dict = new Dictionary<Vector3Int, int>();
        PriorityQueue<Tuple<Vector3Int, List<Direction>>> states = new PriorityQueue<Tuple<Vector3Int, List<Direction>>>();

        dict[problem.GetStartState()] = 0;
        states.Enqueue(new Tuple<Vector3Int, List<Direction>>(problem.GetStartState(), new List<Direction>()), 0);


        List<Vector3Int> visited = new List<Vector3Int>();

        while (!states.IsEmpty())
        {
            var stateTuple = states.Dequeue();
            Vector3Int currentNode = stateTuple.Item1;
            List<Direction> totalAction = stateTuple.Item2;

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
                    Vector3Int node = successor.Item1;
                    Direction action = successor.Item2;
                    int cost = successor.Item3;

                    dict[node] = dict[currentNode] + cost;
                    states.Enqueue(new Tuple<Vector3Int, List<Direction>>(node, new List<Direction>(totalAction) { action }), dict[node]);
                }
            }
        }

        return new List<Direction>();
    }

    public static List<Direction> AStarSearch(SearchProblem problem, Func<Vector3Int, SearchProblem, int> heuristic = null)
    {
        Dictionary<Vector3Int, int> dict = new Dictionary<Vector3Int, int>();
        PriorityQueue<Tuple<Vector3Int, List<Direction>, int>> states = new PriorityQueue<Tuple<Vector3Int, List<Direction>, int>>();
    
        Vector3Int startState = problem.GetStartState();
        dict[startState] = 0 + (heuristic != null ? heuristic(startState, problem) : 0);
        states.Enqueue(new Tuple<Vector3Int, List<Direction>, int>(startState, new List<Direction>(), 0), dict[startState]);
        
        var visited = new List<Vector3Int>();
        
        while (!states.IsEmpty())
        {
            var currentState = states.Dequeue();
            Vector3Int currentNode = currentState.Item1;
            List<Direction> totalAction = currentState.Item2;
            int totalCost = currentState.Item3;
            
            if (problem.IsGoalState(currentNode))
            {
                return totalAction;
            }
            
            if (!visited.Contains(currentNode))
            {
                visited.Add(currentNode);
                foreach (var successor in problem.GetSuccessors(currentNode))
                {
                    Vector3Int node = successor.Item1;
                    Direction action = successor.Item2;
                    int cost = successor.Item3;
                    int newTotalCost = totalCost + cost;
                    dict[node] = newTotalCost + (heuristic != null ? heuristic(node, problem) : 0);
                    states.Enqueue(new Tuple<Vector3Int, List<Direction>, int>(node, new List<Direction>(totalAction) { action }, newTotalCost), dict[node]);
                }
            }
        }
        return new List<Direction>();
    }
}


public class PositionSearchProblem : SearchProblem
{
    private Level level;
    private Vector3Int goal;
    private Vector3Int start;

    public PositionSearchProblem(Vector3Int start, Vector3Int goal, Level l) {
        this.goal = goal;
        this.start = start;
        this.level = l;
    }

    public override Vector3Int GetStartState() {
        return start;
    }

    // Returns true if and only if the state is a valid goal state.
    public override bool IsGoalState(Vector3Int node) {
        return node==goal;
    }

    // For a given state, this should return a list of triples (successor, action, stepCost).
    public override List<Tuple<Vector3Int, Direction, int>> GetSuccessors(Vector3Int state) {
        var successors = new List<Tuple<Vector3Int, Direction, int>>();
        foreach (Direction direction in Directions.GetAllDirections())
        {
            Vector3Int vector = Directions.DirectionToVector(direction);
            Vector3Int nextPos  = state + vector;
            GridCell cell = level.GetCell(nextPos);
            if (cell==null) continue;
            if (!cell.isSolid)
            {
                successors.Add(new Tuple<Vector3Int, Direction, int>(nextPos, direction, cell.GetCost()));
            }
        }
        return successors;
    }

    // Returns the total cost of a particular sequence of actions.
    public override int GetCostOfActions(List<Direction> actions) {
        if (actions == null) return 999999;
        Vector3Int pos = GetStartState();
        int cost = 0;
        foreach(Direction action in actions) {
            Vector3Int vector =  Directions.DirectionToVector(action);
            Vector3Int nextPos  = pos + vector;
            GridCell cell = level.GetCell(nextPos);
            if (cell.isSolid)
            {
                return 999999;
            }
            cost += costFn(nextPos);
        }
        return cost;
    }

    public override int costFn(Vector3Int pos) {
        return 1;
    }
}