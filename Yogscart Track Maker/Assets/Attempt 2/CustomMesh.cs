using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomMesh : MonoBehaviour
{

    private int[,] map;
    private bool[,] edgeMap;

    private List<List<Vector3>> outlines;
    private HashSet<Vector3> visitedPoints;

    public bool showMap, showEdgeMap;

    [Range(0,100)]
    public float precision = 50;

    public void GenerateMesh(int[,] _map)
    {
        outlines = new List<List<Vector3>>();
        visitedPoints = new HashSet<Vector3>();

        map = _map;
        edgeMap = DetermineEdges(map);

        GetCorners();
    }

    //Go through the grid and determine whether each pixel is an edge or not
    public bool[,] DetermineEdges(int[,] map)
    {
        int width = map.GetLength(0), height = map.GetLength(1);
        bool[,] edgeMap = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x,y] == 1 && ((x > 0 && map[x - 1, y] == 0) || (x < width - 1 && map[x + 1, y] == 0) || (y > 0 && map[x, y - 1] == 0) || (y < height - 1 && map[x, y + 1] == 0)))
                {
                    //Set this point as an edge
                    edgeMap[x, y] = true;
                }
            }
        }

        return edgeMap;
    }

    //Go through the Edge Grid and create a list of the significant corners
    private void GetCorners()
    {
        int width = edgeMap.GetLength(0), height = edgeMap.GetLength(1);
      
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(!visitedPoints.Contains(new Vector3(x, y)))
                {
                    List<Vector3> newRoute = new List<Vector3>();
                    RouteSearch(newRoute, x, y);
                }
            }
        }
    }

    private void RouteSearch(List<Vector3> route, int currentX, int currentY)
    {
        //Look for new next edge in route starting from the top and working clockwise. Stop when you reach the start point or there is no where else to go
        int width = edgeMap.GetLength(0), height = edgeMap.GetLength(1);
        Vector3 currentPoint = new Vector3(currentX, 0, currentY);

        //Add Current Point to Route
        route.Add(currentPoint);
        visitedPoints.Add(currentPoint);

        //Check for end of Route
        if (currentPoint == route[0])
        {
            outlines.Add(route);
            return;
        }

        //Check for new point in route
        if(currentY < height - 1 && edgeMap[currentX,currentY+1] && !visitedPoints.Contains(new Vector3(currentX, currentY + 1)))//Top
        {
            RouteSearch(route, currentX, currentY + 1);
            return;
        }
        else if (currentY < height - 1 && currentX < width - 1 && edgeMap[currentX + 1, currentY + 1] && !visitedPoints.Contains(new Vector3(currentX + 1, currentY + 1)))//Top Right
        {
            RouteSearch(route, currentX + 1, currentY + 1);
            return;
        }
        else if (currentX < width - 1 && edgeMap[currentX + 1, currentY] && !visitedPoints.Contains(new Vector3(currentX, currentY)))//Right
        {
            RouteSearch(route, currentX + 1, currentY);
            return;
        }
        else if (currentX < width - 1 && currentY > 0 && edgeMap[currentX + 1, currentY - 1] && !visitedPoints.Contains(new Vector3(currentX + 1, currentY - 1)))//Bottom Right
        {
            RouteSearch(route, currentX + 1, currentY - 1);
            return;
        }
        else if (currentY > 0 && edgeMap[currentX, currentY - 1] && !visitedPoints.Contains(new Vector3(currentX, currentY - 1)))//Bottom
        {
            RouteSearch(route, currentX, currentY - 1);
            return;
        }
        else if (currentX > 0 && currentY > 0 && edgeMap[currentX - 1, currentY - 1] && !visitedPoints.Contains(new Vector3(currentX - 1, currentY - 1)))//Bottom Left
        {
            RouteSearch(route, currentX - 1, currentY - 1);
            return;
        }
        else if (currentX > 0 && edgeMap[currentX - 1, currentY] && !visitedPoints.Contains(new Vector3(currentX - 1, currentY)))//Left
        {
            RouteSearch(route, currentX - 1, currentY);
            return;
        }
        else if (currentX > 0 && currentY < height - 1 && edgeMap[currentX - 1, currentY + 1] && !visitedPoints.Contains(new Vector3(currentX - 1, currentY + 1)))//Top Left
        {
            RouteSearch(route, currentX - 1, currentY + 1);
            return;
        }

        //Nowhere to go!
        outlines.Add(route);
        return;

    }

    void OnDrawGizmos()
    {
        if (edgeMap != null)
        {
            for (int x = 0; x < edgeMap.GetLength(0); x++)
            {
                for (int y = 0; y < edgeMap.GetLength(1); y++)
                {
                    if(showMap)
                    {
                        Gizmos.color = (map[x, y] == 0) ? Color.white : Color.black;
                        Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one);
                    }

                    if(showEdgeMap && edgeMap[x,y])
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one);
                    }
                }
            }
        }
    }
}

public class Line
{
    public Vector3 startPoint;
    public Vector3 endPoint;

    public Line(Vector3 start, Vector3 end)
    {
        startPoint = start;
        endPoint = end;
    }

    public Vector3 GetVector3()
    {
        return endPoint - startPoint;
    }
}