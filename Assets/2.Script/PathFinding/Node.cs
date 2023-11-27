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

    [Header("Variables for path finding")]
    public int X, Y;
    public int G = -1, H = -1;
    public bool IsWall;
    public Node parentNode;

    public int F => G + H;

    [Header("Variables for moving according to the path")]
    public float PosX, PosY;

    public Vector2 Pos => new Vector2(PosX, PosY);

    #endregion Path Finding

    #region IComparable Interface

    public int CompareTo(Node other)
    {
        return F == other.F ? 0 : F - other.F;
    }

    #endregion IComparable Interface
}
