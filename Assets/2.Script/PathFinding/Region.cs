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
    #region Variables

    [SerializeField] private ERegion[] adjRegion = null;

    [Header("Vector variables for setting the region area")]
    private Vector2Int position = Vector2Int.zero;
    private Vector2Int topRightPos = Vector2Int.zero;
    private Vector2Int bottomLeftPos = Vector2Int.zero;

    /// <summary>
    /// The estimated cost required to reach the target region.
    /// </summary>
    [HideInInspector] public int G = -1;

    /// <summary>
    /// The cost used to move to the current region.
    /// </summary>
    [HideInInspector] public int H = -1;

    /// <summary>
    /// The parent region in the path to reach the current region.
    /// </summary>
    [HideInInspector] public Region parentRegion = null;

    #endregion Variables

    #region Properties

    /// <summary>
    /// The array containing the indices of adjacent regions.
    /// The elements are directly set in the Editor based on the map layout 
    /// </summary>
    public ERegion[] AdjRegion => adjRegion;

    /// <summary>
    /// The position of the region.
    /// Used in path finding algorithm between the regions.
    /// </summary>
    public Vector2Int Position => position;

    /// <summary>
    /// The sum of G and H value.
    /// Used as a reference for exploring the next region.
    /// </summary>
    public int F => G + H;

    #endregion Properties

    #region Methods

    /// <summary>
    /// Initialize the variables of the region.
    /// Convert the top-right and bottom-left position of the region to the array coordinate.
    /// The top-right position is rounded up, and the bottom-left position is rounded down.
    /// </summary>
    /// <param name="basePosition">The origin position in the array in the array coordinate</param>
    /// <param name="inverseInterval">The inverse of the interval</param>
    public void InitRegion(Vector3 basePosition, float inverseInterval)
    {
        Transform regionTransform = GetComponent<Transform>();
        Transform topRight = regionTransform.GetChild(0);
        Transform bottomLeft = regionTransform.GetChild(1);

        position.x = Mathf.RoundToInt((regionTransform.position.x - basePosition.x) * inverseInterval);
        position.y = Mathf.RoundToInt((regionTransform.position.y - basePosition.y) * inverseInterval);
        topRightPos.x = Mathf.FloorToInt((topRight.position.x - basePosition.x) * inverseInterval);
        topRightPos.y = Mathf.FloorToInt((topRight.position.y - basePosition.y) * inverseInterval);
        bottomLeftPos.x = Mathf.CeilToInt((bottomLeft.position.x - basePosition.x) * inverseInterval);
        bottomLeftPos.y = Mathf.CeilToInt((bottomLeft.position.y - basePosition.y) * inverseInterval);
    }

    /// <summary>
    /// Check whether the given position is inside the region.
    /// </summary>
    /// <param name="pos">The position in the array coordinate to check</param>
    /// <returns>true if the given position is inside the region</returns>
    public bool CheckInRegion(Vector2Int pos)
    {
        return topRightPos.x >= pos.x && topRightPos.y >= pos.y
            && bottomLeftPos.x <= pos.x && bottomLeftPos.y <= pos.y;
    }

    #endregion Methods

    #region IComparable Interface 

    public int CompareTo(Region other)
    {
        return F == other.F ? 0 : F - other.F;
    }

    #endregion IComparable Interface 
}
