using System;
using UnityEngine;

public enum ERegion { NONE = -1, 
    ADMIN, CAFETERIA, COMMUNICATION, ELECTRICAL, UPPERENGINE, LOWERENGINE, 
    MEDBAY, NAVIGATION, WEAPON, STORAGE, SECURITY, SHIELDS, 
    REACTOR, O2, HALLWAY1, HALLWAY2, HALLWAY3, HALLWAY4, 
    HALLWAY5, HALLWAY6, HALLWAY7 
}

public class Region : MonoBehaviour, IComparable<Region>
{
    [SerializeField] private ERegion[] adjRegion = null;

    private Vector2Int position = Vector2Int.zero;
    private Vector2Int topRightPos = Vector2Int.zero;
    private Vector2Int bottomLeftPos = Vector2Int.zero;

    public Vector2Int Position => position;

    public int G = -1, H = -1;
    public int F => G + H;

    public Region parentRegion = null;

    public ERegion[] AdjRegion => adjRegion;

    public void InitRegion(Vector3 basePosition, float inverseOffset)
    {
        Transform regionTransform = GetComponent<Transform>();
        Transform topRight = regionTransform.GetChild(0);
        Transform bottomLeft = regionTransform.GetChild(1);

        position.x = Mathf.RoundToInt((regionTransform.position.x - basePosition.x) * inverseOffset);
        position.y = Mathf.RoundToInt((regionTransform.position.y - basePosition.y) * inverseOffset);
        topRightPos.x = Mathf.FloorToInt((topRight.position.x - basePosition.x) * inverseOffset);
        topRightPos.y = Mathf.FloorToInt((topRight.position.y - basePosition.y) * inverseOffset);
        bottomLeftPos.x = Mathf.CeilToInt((bottomLeft.position.x - basePosition.x) * inverseOffset);
        bottomLeftPos.y = Mathf.CeilToInt((bottomLeft.position.y - basePosition.y) * inverseOffset);
    }

    public bool CheckInRegion(Vector2Int pos)
    {
        return topRightPos.x >= pos.x && topRightPos.y >= pos.y
            && bottomLeftPos.x <= pos.x && bottomLeftPos.y <= pos.y;
    }

    #region IComparable Interface 

    public int CompareTo(Region other)
    {
        return F == other.F ? 0 : F - other.F;
    }

    #endregion IComparable Interface 
}
