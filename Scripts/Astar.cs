using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;


public class Astar : MonoBehaviour
{
    public List<Node> pathFindedList = new List<Node>();

    public GameObject startingObject;
    public GameObject targetObject;


    Node[,] grid;

    public Vector2Int gridWorldSize;
    public float NodeDiameter;
    public LayerMask wallLayer;

    float nodeRadius { get { return NodeDiameter / 2; } }

    int gridSizeX { get { return Mathf.RoundToInt(gridWorldSize.x / NodeDiameter); } }
    int gridSizeY { get { return Mathf.RoundToInt(gridWorldSize.y / NodeDiameter); } }

    int gridSize { get { return gridSizeX * gridSizeY; } }
    Vector2 bottomLeftGrid { get { return (Vector2)transform.position - ((Vector2.right) * gridWorldSize.x / 2 + Vector2.up * gridWorldSize.y / 2); } }

    private void Start()
    {


        grid = new Node[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 pos = new Vector2((x * NodeDiameter), (y * NodeDiameter)) + bottomLeftGrid;
                bool notWalkable = Physics2D.OverlapCircle(pos, nodeRadius + 0.1f, wallLayer);
                grid[x, y] = new Node(pos, notWalkable, x, y);
            }
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AStarPathFind(startingObject.transform.position, targetObject.transform.position);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            UnityEngine.Debug.Log("Press J");
            JumpPointSearch(startingObject.transform.position, targetObject.transform.position);
        }

        if(Input.GetKeyDown(KeyCode.Z))
        {
            Check();
        }
    }

    void Check()
    {
        for(int x = 0; x < 10; x++)
        {
            for(int y = 0; y < 10; y++)
            {
                if (x > 0 && x < 10 && y > 0 && y < 10)
                {
                    UnityEngine.Debug.Log(x);
                    UnityEngine.Debug.Log(y);
                }
            }
        }
    }

    public List<Node> JumpPointSearch(Vector3 startPos, Vector3 targetPos)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Node startNode = GetNodeFromVector(startPos);
        Node endNode = GetNodeFromVector(targetPos);

        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();

        startNode.gCost = 0;
        startNode.hCost = 0;

        openList.Add(startNode);

        while (openList.Count > 0)
        {

            Node curNode = openList[0];

            for(int i =1; i < openList.Count;i ++)
            {
                if(openList[i].Fcost < curNode.Fcost)
                {
                    curNode = openList[i];
                }
            }

            openList.Remove(curNode);
            closedList.Add(curNode);

            if (curNode == endNode)
            {
                sw.Stop();
                UnityEngine.Debug.Log("Path Founded : " + sw.ElapsedMilliseconds + " ms");
                return backTrace(curNode);
            }

            int endX = endNode.x;
            int endY = endNode.y;


            // var neighbour = findNeighbour(curNode);
            for (int x = 1; x >= -1; x--)
            {
                for (int y = 1; y >= -1; y--)
                {
                    if (x == 0 && y == 0) continue;

                    Node jumpNode = testJump(curNode, x, y, endNode); // Error here!


                    if (jumpNode != null)
                    {
                        int jGcost = curNode.gCost + GetDistanceNodeManHattan(curNode, jumpNode);
                        UnityEngine.Debug.LogWarning("Found JumpNode");

                        if (!openList.Contains(jumpNode) || jGcost < jumpNode.hCost)
                        {

                            UnityEngine.Debug.LogError("You win");

                            jumpNode.gCost = jGcost;
                            jumpNode.hCost = GetDistanceNodeManHattan(jumpNode, endNode);
                            jumpNode.parent = curNode;

                            if (openList.Contains(jumpNode) == false)
                            {
                                openList.Add(jumpNode);
                            }
                        }
                    }
                    
                }

            }
        }
        return null;
    }


    void findNeighbour(Node curNode)
    {

    }

    void CalculateNode(Node nodeStart, Node nodeEnd)
    {
        int manhattan = Mathf.Abs(nodeEnd.x - nodeStart.x) * 10 + Mathf.Abs(nodeEnd.y - nodeStart.y) * 10;

        nodeStart.hCost = manhattan;

        Node parent = nodeStart.parent;

        nodeStart.gCost = parent.gCost + calculateGscore(nodeStart, parent);
    }

    int calculateGscore(Node newNode, Node oldNode)
    {
        int x = newNode.x - oldNode.x;
        int y = newNode.y - oldNode.y;

        if (x == 0 || y == 0) return Mathf.Abs(10 * Mathf.Max(x, y));
        else return Mathf.Abs(14 * Mathf.Max(x, y));
    }

    bool isValidNeighbour(Node node, Node neighbour, List<Node> clostList)
    {
        return neighbour != null && neighbour.notWalkable == false && !clostList.Contains(neighbour) && !neighbour.Equals(node);
    }
    /*
    Node Jump(Node curNode, int x, int y, Node endNode)
    {
        int nextX = curNode.grid_X + x;
        int nextY = curNode.grid_Y + y;

        Node nextNode = grid[nextX, nextY];

        // nextNode is Null
        if (nextNode == null || nextNode.notWalkable == true)
        {
            UnityEngine.Debug.Log("Loop");
            return null;
        }

        if (nextNode == endNode) return nextNode;


        if (x != 0 && y != 0) // diagonal check the neighbour
        {

            if (getNode(nextX - x, nextY) != null && getNode(nextX - x, nextY + y) != null)
            {
                if (getNode(nextX - x, nextY).notWalkable && getNode(nextX - x, nextY + y).notWalkable == false)
                {
                    return nextNode;
                }
            }

            if (getNode(nextX, nextY - y) != null && getNode(nextX + x, nextY - y) != null)
            {
                if (getNode(nextX, nextY - y).notWalkable && getNode(nextX + x, nextY - y).notWalkable == false)
                {
                    return nextNode;
                }
            }

            if (Jump(nextNode, x, 0, endNode) != null || Jump(nextNode, 0, y, endNode) != null)
            {
                return nextNode;
            }

        }
        else // hor ver check the neighbour
        {
            if (x != 0)
            {
                if (isNotPassable(nextX + x, nextY) == false && isNotPassable(nextX, nextX + 1))
                    if (isNotPassable(nextX + x, nextY + 1) == false)
                        return nextNode;
                if (isNotPassable(nextX + x, nextY) == false && isNotPassable(nextX, nextY - 1))
                    if (isNotPassable(nextX + x, nextY - 1) == false)
                        return nextNode;
            }
            else
            {
                if (isNotPassable(nextX, nextY + y) == false && isNotPassable(nextX + 1, nextY))
                    if (isNotPassable(nextX + 1, nextY + y) == false)
                        return nextNode;
                if (isNotPassable(nextX, nextY + y) == false && isNotPassable(nextX - 1, nextY))
                    if (isNotPassable(nextX - 1, nextY + y) == false)
                        return nextNode;

            }
        }
        return Jump(nextNode, x, y, endNode);
    }
    */
    Node testJump(Node curNode, int x, int y, Node endNode)
    {
        int nextX = curNode.x + x;
        int nextY = curNode.y + y;

        Node nextNode = getNode(nextX, nextY);

        if (nextNode == null || nextNode.notWalkable) return null;

        if (nextNode == endNode) return nextNode;


        if (x != 0 && y != 0)
        {/*
            if (getNode(nextX - x, curNode.grid_Y).notWalkable && getNode(curNode.grid_X - x, curNode.grid_Y + y).notWalkable == false ||
        getNode(curNode.grid_X, curNode.grid_Y - y).notWalkable && getNode(nextX, curNode.grid_Y - y).notWalkable)
                return curNode;
            */
            if (testJump(nextNode, x, 0, endNode) != null || testJump(nextNode, 0, y, endNode) != null)
                return nextNode;
        }
        else
        {
            /*
            if (x != 0) // Hor
            {
                if (getNode(curNode.grid_X, curNode.grid_Y + 1).notWalkable && getNode(nextX, curNode.grid_Y + 1).notWalkable == false)
                    return curNode;

                if (getNode(curNode.grid_X, curNode.grid_Y - 1).notWalkable && getNode(nextX, curNode.grid_Y - 1).notWalkable == false)
                    return curNode;
            }
            else if (y != 0) // Ver
            {
                if (getNode(curNode.grid_X + 1, curNode.grid_Y).notWalkable && getNode(curNode.grid_X + 1, nextY).notWalkable == false)
                    return curNode;

                if (getNode(curNode.grid_X - 1, curNode.grid_Y).notWalkable && getNode(curNode.grid_X - 1, nextY).notWalkable == false)
                    return curNode;
            }
            */
        }
        UnityEngine.Debug.Log("Loop");
        return testJump(nextNode, x, y, endNode);
    }

    /// <summary>
    /// gridSizeX > x >= 0
    /// gridSizeY > y >= 0
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    Node getNode(int x,int y)
    {
        
        if (x >= 0 && y >= 0 && x < gridSizeX && y < gridSizeY)
        {
            return grid[x, y];
        }
        else
        {
            return null;
        }
    }
    bool notWalkable(int x, int y)
    {
        if(x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
        {
            return grid[x, y].notWalkable;

        }

        return false;
    }

    List<Node> jpsPath = new List<Node>();
    List<Node> backTrace(Node node)
    {
        List<Node> path = new List<Node>();
        Node temp = node;

        while (temp != null)
        {
            path.Add(temp);
            temp = temp.parent;
        }
        jpsPath = path;
        return path;

    }

    void AStarPathFind(Vector3 startPos, Vector3 targetPos)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Node startNode = GetNodeFromVector(startPos);
        Node endNode = GetNodeFromVector(targetPos);

        Heap<Node> openSet = new Heap<Node>(gridSize);
        List<Node> closedSet = new List<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node curNode = openSet.RemoveFirst();
            closedSet.Add(curNode);

            if (curNode == endNode)
            {
                sw.Stop();
                // TestRetracePath(startNode, endNode);
                RetracePath(startNode, endNode);
                UnityEngine.Debug.Log("Path Founded : " + sw.ElapsedMilliseconds + " ms");
                return;
            }

            foreach (Node nbour in GetNeighbour(curNode))
            {
                if (nbour.notWalkable || closedSet.Contains(nbour)) continue;

                int nBourGcost = curNode.gCost + GetDistanceNodeManHattan(curNode, nbour);

                if (!openSet.Contains(nbour) || nBourGcost < nbour.gCost)
                {
                    nbour.gCost = nBourGcost;
                    nbour.hCost = GetDistanceNodeManHattan(nbour, endNode);
                    nbour.parent = curNode;

                    if (!openSet.Contains(nbour))
                    {
                        openSet.Add(nbour);
                    }
                }
            }
        }
    }

    void RetracePath(Node start, Node target)
    {

        List<Node> path = new List<Node>();

        Node curNode = target;

        while (curNode != start)
        {
            path.Add(curNode);
            curNode = curNode.parent;
        }


        path.Reverse();
        pathFindedList = path;
    }

    Queue<Node> quPath;
    void TestRetracePath(Node start, Node target)
    {
        Queue<Node> quPath = new Queue<Node>();

        Node curNode = target;

        while (curNode != start)
        {
            quPath.Enqueue(curNode);
            curNode = curNode.parent;
        }

        this.quPath = quPath;
    }

    public Node GetNodeFromVector(Vector3 worldPos)
    {

        if (worldPos.x > gridWorldSize.x / 2 || worldPos.x < -gridWorldSize.x / 2) { return null; }
        if (worldPos.y > gridWorldSize.y / 2 || worldPos.y < -gridWorldSize.y / 2) { return null; }

        float GridPercentageOfX = (worldPos.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float GridPercentageOfY = (worldPos.y + gridWorldSize.y / 2) / gridWorldSize.y;

        int x = Mathf.RoundToInt((gridSizeX) * GridPercentageOfX);
        int y = Mathf.RoundToInt((gridSizeY) * GridPercentageOfY);

        return grid[x, y];
    }

    int GetDistanceNodeManHattan(Node nodeStart, Node nodeEnd)
    {

        int X = Mathf.Abs(nodeStart.x - nodeEnd.x);
        int Y = Mathf.Abs(nodeStart.y - nodeEnd.y);
        if (X == 0 || Y == 0) //  Horizontal || Vertical
        {
            return 14 * X + 10 * (Y - X);
        }
        else // Diagonal
        {
            return 14 * Y + 10 * (X - Y);
        }
        /*
        if (X > Y) return 14 * Y + 10 * (X - Y);
        else return 14 * X + 10 * (Y - X);
        */
    }
    List<Node> GetNeighbour(Node n)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = n.x + x;
                int checkY = n.y + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) // if Neighbour in bound. add it
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector2(gridWorldSize.x, gridWorldSize.y));

        if (grid != null)
        {
            Node startNode = GetNodeFromVector(startingObject.transform.position);
            Node endNode = GetNodeFromVector(targetObject.transform.position);
            foreach (Node n in grid)
            {
                if (n.notWalkable == true) { Gizmos.color = Color.black; }
                else { Gizmos.color = Color.white; }



                if (startNode == n) { Gizmos.color = Color.red; }
                if (endNode == n) { Gizmos.color = Color.green; }

                if (jpsPath.Contains(n))
                {
                    Gizmos.color = Color.yellow;
                }

                if (pathFindedList.Contains(n))
                {
                    Gizmos.color = Color.blue;
                }


                Gizmos.DrawCube(n.pos, Vector2.one * (nodeRadius));
            }
        }
    }
}