using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
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

    #region Operator Overrideing

    public static bool operator <(Node a, Node b)
    {
        return a.F < b.F;
    }

    public static bool operator >(Node a, Node b)
    {
        return a.F > b.F;
    }

    public static bool operator ==(Node a, Node b)
    {
        return a.F == b.F;
    }

    public static bool operator !=(Node a, Node b)
    {
        return a.F != b.F;
    }

    public static bool operator >=(Node a, Node b)
    {
        return a.F >= b.F;
    }

    public static bool operator <=(Node a, Node b)
    {
        return a.F <= b.F;
    }

    public override bool Equals(object obj)
    {
        return obj is Node node &&
               X == node.X &&
               Y == node.Y &&
               G == node.G &&
               H == node.H &&
               IsWall == node.IsWall &&
               EqualityComparer<Node>.Default.Equals(parentNode, node.parentNode) &&
               F == node.F &&
               PosX == node.PosX &&
               PosY == node.PosY &&
               Pos.Equals(node.Pos);
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(X);
        hash.Add(Y);
        hash.Add(G);
        hash.Add(H);
        hash.Add(IsWall);
        hash.Add(parentNode);
        hash.Add(F);
        hash.Add(PosX);
        hash.Add(PosY);
        hash.Add(Pos);
        return hash.ToHashCode();
    }

    #endregion Operator Overriding
}
