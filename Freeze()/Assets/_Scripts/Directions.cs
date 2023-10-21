using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum Direction
{
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West,
    NorthWest,
    Forward,
    Back,
    Stop
}

public class Directions
{

    public static Direction GetOppositeDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return Direction.South;
            case Direction.NorthEast:
                return Direction.SouthWest;
            case Direction.East:
                return Direction.West;
            case Direction.SouthEast:
                return Direction.NorthWest;
            case Direction.South:
                return Direction.North;
            case Direction.SouthWest:
                return Direction.NorthEast;
            case Direction.West:
                return Direction.East;
            case Direction.NorthWest:
                return Direction.SouthEast;
            case Direction.Forward:
                return Direction.Back;
            case Direction.Back:
                return Direction.Forward;
            default:
                return Direction.Stop;
        }
    }

    public static Vector3Int DirectionToVector(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return new Vector3Int(0, 1, 0);
            case Direction.NorthEast:
                return new Vector3Int(1, 1, 0);
            case Direction.East:
                return new Vector3Int(1, 0, 0);
            case Direction.SouthEast:
                return new Vector3Int(1, -1, 0);
            case Direction.South:
                return new Vector3Int(0, -1, 0);
            case Direction.SouthWest:
                return new Vector3Int(-1, -1, 0);
            case Direction.West:
                return new Vector3Int(-1, 0, 0);
            case Direction.NorthWest:
                return new Vector3Int(-1, 1, 0);
            case Direction.Forward:
                return new Vector3Int(0, 0, 1);
            case Direction.Back:
                return new Vector3Int(0, 0, -1);
            default:
                return Vector3Int.zero;
        }
    }

    public static List<Direction> GetAllDirections()
    {
        List<Direction> directions = new List<Direction>
        {
            Direction.North,
            Direction.NorthEast,
            Direction.East,
            Direction.SouthEast,
            Direction.South,
            Direction.SouthWest,
            Direction.West,
            Direction.NorthWest,
            Direction.Forward,
            Direction.Back
        };
        return directions;
    }
}
