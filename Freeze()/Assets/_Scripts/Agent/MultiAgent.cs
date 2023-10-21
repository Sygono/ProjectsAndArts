using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent
{
    public abstract Direction GetAction(Vector3Int pos);

    public abstract float EvaluationFunction(Vector3Int pos, Direction action);
}
