using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public BlockType blockType;

}

public enum BlockType {
    Empty,
    Solid,
    Slow,
    Lazer,
}
