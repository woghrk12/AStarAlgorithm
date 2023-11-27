using System.Collections.Generic;
using UnityEngine;

public enum EPathFindingState { NONE = -1, CALCULATING, COMPLETE, NOTFOUND, END }
public enum ECalculateMode { NONE = -1, HEAP, ACCELATEDHEAP, END }

public class PathFindingController : MonoBehaviour
{
    [Header("Transform objects for specifying the range")]
    [SerializeField] private Transform topRight = null;
    [SerializeField] private Transform bottomLeft = null;

    [Header("Offset value between moving points")]
    [SerializeField] private float offset = 0.1f;
    private float inverseOffset = 0f;

    [Header("Map data")]
    private Node[,] board = null;
    private int width = 0;
    private int height = 0;

    [Header("Circle object for debugging the map data")]
    [SerializeField] private GameObject blockPrefab = null;
    private GameObject[,] blockObjectBoard = null;

    [Header("Transform objects for nodes")]
    [SerializeField] private Transform startTransform = null;
    [SerializeField] private Transform targetTransform = null;

    [Header("Variables for calculating the path")]
    private int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
    private int[] dy = { 1, 1, 0, -1, -1, -1, 0, 1 };
    private Node startNode = null;
    private Node targetNode = null;
    private List<Node> finalNodeList = new();

    [Header("Variables for evaluating performance")]
    private int visitedNodeCount = 0;
    private long startTick = 0;
    private long endTick = 0;

    private void Awake()
    {
        inverseOffset = 1 / offset;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            InitializeBoard(topRight, bottomLeft);
        }
    }

    #region Path Finding

    private void InitializeBoard(Transform topRight, Transform bottomLeft)
    {
        Vector2Int topRightPos = new Vector2Int(Mathf.CeilToInt(topRight.position.x * 10f), Mathf.CeilToInt(topRight.position.y * 10f));
        Vector2Int bottomLeftPos = new Vector2Int(Mathf.FloorToInt(bottomLeft.position.x * 10f), Mathf.FloorToInt(bottomLeft.position.y * 10f));

        width = Mathf.CeilToInt((topRightPos.x - bottomLeftPos.x) * inverseOffset);
        height = Mathf.CeilToInt((topRightPos.y - bottomLeftPos.y) * inverseOffset);

        board = new Node[width, height];
        blockObjectBoard = new GameObject[width, height];
        
        float xPos = bottomLeft.position.x;
        for (int i = 0; i < width; i++)
        {
            float yPos = bottomLeft.position.y;
            
            for (int j = 0; j < height; j++)
            {
                bool isWall = false;

                if (Physics2D.OverlapCircle(new Vector2(xPos, yPos), 0.1f, 1 << TagAndLayer.Layer.WallCollider))
                {
                    isWall = true;

                    // Debugging object for checking the area that the character can reach
                    GameObject blockObject = Instantiate(blockPrefab, new Vector3(xPos, yPos, 0f), Quaternion.identity);
                    blockObjectBoard[i, j] = blockObject;
                }

                board[i, j] = new Node(i, j, xPos, yPos, isWall);

                yPos += offset;
            }

            xPos += offset;
        }
    }

    private void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                board[i, j].G = -1;
                board[i, j].H = -1;
                board[i, j].parentNode = null;
            }
        }

        startNode = null;
        targetNode = null;
        finalNodeList.Clear();

        Debug.Log("# nodes of being visted : " + visitedNodeCount);
        visitedNodeCount = 0;
    }

    #endregion Path Finding

    #region Helper Methods

    private int CalculateDistance(Vector2Int a, Vector2Int b)
    {
        int dx = a.x > b.x ? a.x - b.x : b.x - a.x;
        int dy = a.y > b.y ? a.y - b.y : b.y - a.y;
        int e1 = dx > dy ? dx - dy : dy - dx;
        int e2 = dx < dy ? dx : dy;

        return e1 * 10 + e2 * 14;
    }

    private void DrawFinalNodeList()
    {
        for (int i = 0; i < finalNodeList.Count - 1; i++)
        {
            Debug.DrawLine(finalNodeList[i].Pos, finalNodeList[i + 1].Pos, Color.green, 10);
        }

        Debug.Log("# nodes of path : " + finalNodeList.Count);
    }

    #endregion Helper Methods
}
