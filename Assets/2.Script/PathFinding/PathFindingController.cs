using System.Collections.Generic;
using UnityEngine;

public enum EPathFindingState { NONE = -1, CALCULATING, COMPLETE, NOTFOUND, END }
public enum EFindingMode { NONE = -1, BASIC, REGION, END }

public class PathFindingController : MonoBehaviour
{
    [Header("Transform objects for specifying the range")]
    [SerializeField] private Transform topRight = null;
    [SerializeField] private Transform bottomLeft = null;

    [Header("Interval value between moving points")]
    [SerializeField] private float interval = 0.1f;
    private float inverseInterval = 0f;

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
    private EPathFindingState state = EPathFindingState.NONE;
    private EFindingMode mode = EFindingMode.NONE;
    private int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
    private int[] dy = { 1, 1, 0, -1, -1, -1, 0, 1 };
    private Node startNode = null;
    private Node targetNode = null;
    private Node curNode = null;
    private PriorityQueue<Node> nodePriorityQueue = new();
    private List<Node> finalNodeList = new();

    [Header("Variables for regions")]
    [SerializeField] private Transform regionParent = null;
    private List<Region> regionList = new();
    private PriorityQueue<Region> regionPriorityQueue = new();
    private List<Region> finalRegionList = new();
    private int regionIndex = 0;
    private List<Node> tempNodeList = new();
    private Node tempStartNode = null;
    private Node tempTargetNode = null;

    [Header("Variables for evaluating performance")]
    private int visitedNodeCount = 0;

    private void Awake()
    {
        inverseInterval = 1 / interval;

        foreach (Transform child in regionParent)
        {
            Region region = child.GetComponent<Region>();
            region.InitRegion(bottomLeft.position, inverseInterval);
            regionList.Add(region);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            InitializeBoard(topRight, bottomLeft);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            StartPathFinding();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            StartPathFindingWithRegion();
        }

        switch (state)
        {
            case EPathFindingState.CALCULATING:
                if (mode == EFindingMode.BASIC) FindPath();
                else if (mode == EFindingMode.REGION) FindPathWithRegion();
                break;

            case EPathFindingState.COMPLETE:
                DrawFinalNodeList();
                ClearBoard();
                break;

            case EPathFindingState.NOTFOUND:
                Debug.Log("Not Found");
                ClearBoard();
                break;
        }
    }

    #region Path Finding

    /// <summary>
    /// Initialize the board data.
    /// Divide the distance of 1 unit in world coordinates by the interval.
    /// Convert nodes in the world coordinate to the array cooridinate.
    /// </summary>
    /// <param name="topRight">The transform indicating the max x and y components of the board</param>
    /// <param name="bottomLeft">The transform indicating the min x and y components of the board</param>
    private void InitializeBoard(Transform topRight, Transform bottomLeft)
    {
        Vector2Int topRightPos = new Vector2Int(Mathf.CeilToInt(topRight.position.x * 10f), Mathf.CeilToInt(topRight.position.y * 10f));
        Vector2Int bottomLeftPos = new Vector2Int(Mathf.FloorToInt(bottomLeft.position.x * 10f), Mathf.FloorToInt(bottomLeft.position.y * 10f));

        width = Mathf.CeilToInt((topRightPos.x - bottomLeftPos.x) * inverseInterval);
        height = Mathf.CeilToInt((topRightPos.y - bottomLeftPos.y) * inverseInterval);

        board = new Node[width, height];
        blockObjectBoard = new GameObject[width, height];

        // Check whether the node is reachable
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

                yPos += interval;
            }

            xPos += interval;
        }
    }

    #region Basic Path Finding

    /// <summary>
    /// Start the path finding algorithm based on the map data.
    /// </summary>
    private void StartPathFinding()
    {
        state = EPathFindingState.CALCULATING;
        mode = EFindingMode.BASIC;

        // Set the start and target node in the array coordinate
        int startX = Mathf.RoundToInt((startTransform.position.x - bottomLeft.position.x) * inverseInterval);
        int startY = Mathf.RoundToInt((startTransform.position.y - bottomLeft.position.y) * inverseInterval);
        int targetX = Mathf.RoundToInt((targetTransform.position.x - bottomLeft.position.x) * inverseInterval);
        int targetY = Mathf.RoundToInt((targetTransform.position.y - bottomLeft.position.y) * inverseInterval);

        startNode = board[startX, startY];
        targetNode = board[targetX, targetY];

        // Initialize the start node
        startNode.G = 0;
        startNode.H = CalculateDistance(new Vector2Int(startX, startY), new Vector2Int(targetX, targetY));

        nodePriorityQueue.Add(startNode);

        FindPath();
    }

    /// <summary>
    /// Explore the next node with the lowest F value until target node is found.
    /// </summary>
    private void FindPath()
    {
        // Max number of ticks we are allowed to continue working in one run.
        // One tick is 1/10000 of a millisecond.
        // maxTicks is 10 milliseconds, equls to 0.01 second
        // 1 frame is 0.0166 seconds
        long maxTicks = 10 * 10000;
        long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;

        while (nodePriorityQueue.Count > 0)
        {
            if (System.DateTime.UtcNow.Ticks > targetTick) return;

            curNode = nodePriorityQueue.Pop();
            visitedNodeCount++;

            // if target node is found
            if (curNode.Equals(targetNode))
            {
                while (!curNode.Equals(startNode))
                {
                    finalNodeList.Add(curNode);
                    curNode = curNode.parentNode;
                }

                finalNodeList.Reverse();
                state = EPathFindingState.COMPLETE;

                return;
            }

            for (int i = 0; i < 8; i++)
            {
                int tx = curNode.X + dx[i];
                int ty = curNode.Y + dy[i];

                if (tx < 0 || ty < 0 || tx >= width || ty >= height) continue;

                Node nextNode = board[tx, ty];

                if (nextNode.IsWall) continue;

                // Horizontal and vertical cost are 10, diagonal cost is 14
                int moveCost = curNode.G + (dx[i] == 0 || dy[i] == 0 ? 10 : 14);

                // The node to be visited for the first time
                if (nextNode.H < 0)
                {
                    nextNode.G = moveCost;
                    nextNode.H = CalculateDistance(new Vector2Int(tx, ty), new Vector2Int(targetNode.X, targetNode.Y));
                    nextNode.parentNode = curNode;
                    nodePriorityQueue.Add(nextNode);
                }
                // The node already visited
                else
                {
                    if (nextNode.G <= moveCost) continue;

                    nextNode.G = moveCost;
                    nextNode.parentNode = curNode;
                    nodePriorityQueue.Add(nextNode);
                }
            }
        }

        state = EPathFindingState.NOTFOUND;
    }

    #endregion Basic Path Finding

    #region Path Finding with Region

    /// <summary>
    /// Start the path finding algorithm based on the map data divided into the region.
    /// </summary>
    private void StartPathFindingWithRegion()
    {
        state = EPathFindingState.CALCULATING;
        mode = EFindingMode.REGION;

        // Set the start and target node in the array coordinate
        int startX = Mathf.RoundToInt((startTransform.position.x - bottomLeft.position.x) * inverseInterval);
        int startY = Mathf.RoundToInt((startTransform.position.y - bottomLeft.position.y) * inverseInterval);
        int targetX = Mathf.RoundToInt((targetTransform.position.x - bottomLeft.position.x) * inverseInterval);
        int targetY = Mathf.RoundToInt((targetTransform.position.y - bottomLeft.position.y) * inverseInterval);

        startNode = board[startX, startY];
        targetNode = board[targetX, targetY];

        FindRegionPath(startNode, targetNode);

        startNode.G = 0;
        startNode.H = CalculateDistance(new Vector2Int(startX, startY), new Vector2Int(targetX, targetY));

        tempStartNode = startNode;
        nodePriorityQueue.Add(tempStartNode);

        FindPathWithRegion();
    }

    /// <summary>
    /// Explore the next region until target region is found. (1st A* algorithm)
    /// </summary>
    private void FindRegionPath(Node startNode, Node targetNode)
    {
        Region startRegion = null, targetRegion = null;

        foreach (Region region in regionList)
        {
            if (region.CheckInRegion(startNode.IndexPos))
            {
                startRegion = region;
            }
            if (region.CheckInRegion(targetNode.IndexPos))
            {
                targetRegion = region;
            }
        }

        regionPriorityQueue.Add(startRegion);

        while (regionPriorityQueue.Count > 0)
        {
            Region curRegion = regionPriorityQueue.Pop();

            // if target region is found
            if (targetRegion.Equals(curRegion))
            {
                while (curRegion != startRegion)
                {
                    finalRegionList.Add(curRegion);
                    curRegion = curRegion.parentRegion;
                }
                finalRegionList.Add(curRegion);
                finalRegionList.Reverse();
                break;
            }

            foreach (ERegion adjRegionIndex in curRegion.AdjRegion)
            {
                Region adjRegion = regionList[(int)adjRegionIndex];
                int moveCost = curRegion.G + CalculateDistance(curRegion.Position, adjRegion.Position);

                // The region to be visited for the first time
                if (adjRegion.G < 0)
                {
                    adjRegion.G = moveCost;
                    adjRegion.H = CalculateDistance(adjRegion.Position, targetRegion.Position);
                    adjRegion.parentRegion = curRegion;
                    regionPriorityQueue.Add(adjRegion);
                }
                // The region already visited
                else
                {
                    if (adjRegion.G <= moveCost) continue;

                    adjRegion.G = moveCost;
                    adjRegion.parentRegion = curRegion;
                    regionPriorityQueue.Add(adjRegion);
                }
            }
        }
    }

    /// <summary>
    /// Explore the next node with the lowest F value until target node is found. (2nd A* algorithm)
    /// </summary>
    private void FindPathWithRegion()
    {
        if (regionIndex >= finalRegionList.Count - 1)
        {
            FindPathInRegion();
        }
        else
        {
            FindPathToOtherRegion();
        }
    }

    /// <summary>
    /// Explore the next node until reaching the next region.
    /// </summary>
    private void FindPathToOtherRegion()
    {
        // Max number of ticks we are allowed to continue working in one run.
        // One tick is 1/10000 of a millisecond.
        // maxTicks is 10 milliseconds, equls to 0.01 second
        // 1 frame is 0.0166 seconds
        long maxTicks = 10 * 10000;
        long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;

        while (nodePriorityQueue.Count > 0)
        {
            if (System.DateTime.UtcNow.Ticks > targetTick) return;

            curNode = nodePriorityQueue.Pop();
            visitedNodeCount++;

            // if next region is found
            if (finalRegionList[regionIndex].CheckInRegion(curNode.IndexPos))
            {
                Node startNode = tempStartNode;
                Node curNode = this.curNode;

                while (curNode != startNode)
                {
                    tempNodeList.Add(curNode);
                    curNode = curNode.parentNode;
                }

                tempNodeList.Reverse();

                foreach (Node node in tempNodeList)
                {
                    finalNodeList.Add(node);
                }

                tempNodeList.Clear();
                nodePriorityQueue.Clear();

                Vector2Int nextTargetPosition = finalRegionList[++regionIndex].Position;
                tempStartNode = this.curNode;
                tempTargetNode = board[nextTargetPosition.x, nextTargetPosition.y];

                nodePriorityQueue.Add(tempStartNode);

                return;
            }

            for (int i = 0; i < 8; i++)
            {
                int tx = curNode.X + dx[i];
                int ty = curNode.Y + dy[i];

                // Out of range
                if (tx < 0 || ty < 0 || tx >= width || ty >= height) continue;

                Node nextNode = board[tx, ty];

                if (nextNode.IsWall) continue;

                // Horizontal and vertical cost are 10, diagonal cost is 14
                int moveCost = curNode.G + (dx[i] == 0 || dy[i] == 0 ? 10 : 14);

                // The node to be visited for the first time
                if (nextNode.H < 0)
                {
                    nextNode.G = moveCost;
                    nextNode.H = CalculateDistance(new Vector2Int(tx, ty), new Vector2Int(tempTargetNode.X, tempTargetNode.Y));
                    nextNode.parentNode = curNode;
                    nodePriorityQueue.Add(nextNode);
                }
                // The node already visited
                else
                {
                    if (nextNode.G <= moveCost) continue;

                    nextNode.G = moveCost;
                    nextNode.parentNode = curNode;
                    nodePriorityQueue.Add(nextNode);
                }
            }
        }

        state = EPathFindingState.NOTFOUND;
    }

    /// <summary>
    /// Explore the next node until target node is found.
    /// </summary>
    private void FindPathInRegion()
    {
        // Max number of ticks we are allowed to continue working in one run.
        // One tick is 1/10000 of a millisecond.
        // maxTicks is 10 milliseconds, equls to 0.01 second
        // 1 frame is 0.0166 seconds
        long maxTicks = 10 * 10000;
        long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;

        while (nodePriorityQueue.Count > 0)
        {
            if (System.DateTime.UtcNow.Ticks > targetTick) return;

            curNode = nodePriorityQueue.Pop();
            visitedNodeCount++;

            // if target node is found
            if (curNode.Equals(targetNode))
            {
                while (curNode != tempStartNode)
                {
                    tempNodeList.Add(curNode);
                    curNode = curNode.parentNode;
                }

                tempNodeList.Reverse();

                foreach (Node node in tempNodeList)
                {
                    finalNodeList.Add(node);
                }

                tempNodeList.Clear();

                state = EPathFindingState.COMPLETE;

                return;
            }

            for (int i = 0; i < 8; i++)
            {
                int tx = curNode.X + dx[i];
                int ty = curNode.Y + dy[i];

                // Out of range
                if (tx < 0 || ty < 0 || tx >= width || ty >= height) continue;

                Node nextNode = board[tx, ty];

                if (nextNode.IsWall) continue;

                // Horizontal and vertical cost are 10, diagonal cost is 14
                int moveCost = curNode.G + (dx[i] == 0 || dy[i] == 0 ? 10 : 14);

                // The node to be visited for the first time
                if (nextNode.H < 0)
                {
                    nextNode.G = moveCost;
                    nextNode.H = CalculateDistance(new Vector2Int(tx, ty), new Vector2Int(tempTargetNode.X, tempTargetNode.Y));
                    nextNode.parentNode = curNode;
                    nodePriorityQueue.Add(nextNode);
                }
                // The node already visited
                else
                {
                    if (nextNode.G <= moveCost) continue;

                    nextNode.G = moveCost;
                    nextNode.parentNode = curNode;
                    nodePriorityQueue.Add(nextNode);
                }
            }
        }

        state = EPathFindingState.NOTFOUND;
    }

    #endregion Path Finding with Region

    /// <summary>
    /// Clear the board data.
    /// </summary>
    private void ClearBoard()
    {
        // Clear the nodes
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                board[i, j].G = -1;
                board[i, j].H = -1;
                board[i, j].parentNode = null;
            }
        }

        // Clear the regions
        foreach (Region region in regionList)
        {
            region.G = -1;
            region.H = -1;
            region.parentRegion = null;
        }

        // Clear the node variables
        startNode = null;
        targetNode = null;

        // Clear the priority queue
        nodePriorityQueue.Clear();
        finalNodeList.Clear();
        regionPriorityQueue.Clear();
        finalRegionList.Clear();
        regionIndex = 0;

        // Debug the number of visited nodes
        if (mode == EFindingMode.BASIC)
        {
            Debug.Log("# nodes of being visted (Basic A*) : " + visitedNodeCount);
        }
        else if (mode == EFindingMode.REGION)
        {
            Debug.Log("# nodes of being visted (Region Divided A*): " + visitedNodeCount);
        }

        visitedNodeCount = 0;

        state = EPathFindingState.NONE;
        mode = EFindingMode.NONE;
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

    /// <summary>
    /// Draw the path from result of the executed path finding algorithm
    /// </summary>
    private void DrawFinalNodeList()
    {
        if (mode == EFindingMode.BASIC)
        {
            for (int i = 0; i < finalNodeList.Count - 1; i++)
            {
                Debug.DrawLine(finalNodeList[i].Pos, finalNodeList[i + 1].Pos, Color.green, 100);
            }

            Debug.Log("# nodes of path (Basic A*): " + finalNodeList.Count);
        }
        else if (mode == EFindingMode.REGION)
        {
            for (int i = 0; i < finalNodeList.Count - 1; i++)
            {
                Debug.DrawLine(finalNodeList[i].Pos, finalNodeList[i + 1].Pos, Color.green, 100);
            }

            Debug.Log("# nodes of path (Region Divided A*): " + finalNodeList.Count);
        }
    }

    #endregion Helper Methods
}
