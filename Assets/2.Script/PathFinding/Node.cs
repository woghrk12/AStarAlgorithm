using System;
using UnityEngine;

public class Node : IComparable<Node>
{
    public Node(int x, int y, float posX, float posY, bool isWall)
    {
        X = x;
        Y = y;
        PosX = posX;
        PosY = posY;
        IsWall = isWall;
    }

    #region Path Finding

    /// <summary>
    /// The x component of the position in the array coordinate.
    /// </summary>
    public int X;

    /// <summary>
    /// The y component of the position in the array coordinate.
    /// </summary>
    public int Y;

    /// <summary>
    /// The cost used to move to the current node.
    /// </summary>
    public int H = -1;

    /// <summary>
    /// The estimated cost required to reach the target node.
    /// </summary>
    public int G = -1;

    /// <summary>
    /// Whether the node is not reachable by the character.
    /// True if the node is set as the wall.
    /// </summary>
    public bool IsWall;

    /// <summary>
    /// The parent node in the path to reach the current node.
    /// </summary>
    public Node parentNode;

    /// <summary>
    /// The sum of G and H value.
    /// Used as a reference for exploring the next node.
    /// </summary>
    public int F => G + H;

    /// <summary>
    /// The x component of the position in the world coordinate.
    /// </summary>
    public float PosX;

    /// <summary>
    /// The y component of the position in the world coordinate.
    /// </summary>
    public float PosY;

    /// <summary>
    /// The position in the array coordinate.
    /// </summary>
    public Vector2Int IndexPos => new Vector2Int(X, Y);

    /// <summary>
    /// The position in the world coordinate.
    /// </summary>
    public Vector2 Pos => new Vector2(PosX, PosY);

    #endregion Path Finding

    #region IComparable Interface

    public int CompareTo(Node other)
    {
        return F == other.F ? 0 : F - other.F;
    }

    #endregion IComparable Interface
}
