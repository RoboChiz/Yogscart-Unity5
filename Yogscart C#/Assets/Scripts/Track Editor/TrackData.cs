using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//TrackData Script - V3.0
//Created by Robert (Robo_Chiz)
//Do not edit this script, doing so may cause compatibility errors.

[ExecuteInEditMode, System.Serializable, RequireComponent(typeof(InEngineRender))]
public class TrackData : MonoBehaviour
{

    public enum TrackErrors { PointWithNoEnd, LapNotOnMainRoute, RouteSkipsLap, NoStartPoint, NoEndPoint, TooManyStartPoints, TooManyEndPoints, LapPointOnLoopedTrack, NotEnoughPoints, BadTrackDesignPointlessLoop };
    public List<TrackErrors> trackErrors;

    //Track Metadata
    public string trackName = "Untitled Track";
    public AudioClip backgroundMusic;
    public float lastLapPitch = 1.1f; //The pitch of the Audio Source on the last lap of the race

    public bool loopedTrack = true;

    public int Laps = 3;

    public Texture2D map;
    public Vector3[] mapEdgesInWorldSpace;
    /*Order
        Top Left,
        Bottom Left,
        Top Right
    */

    [HideInInspector]
    public Transform spawnPoint;
    
    //DEBUG[HideInInspector]
    //public List<Transform> positionPoints; //Depracted Used to copy over existing layouts

    public List<PointConnector> connections;
    private int lastConnectionCount;

    public List<CameraPoint> introPans;

    public BSPTree pointTree;
    public List<PointHandler> validPointHandlers { get; private set; }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            transform.name = "Track Manager";

            //Fix Intro Pans if null for some reason
            if (introPans == null)
                introPans = new List<CameraPoint>();

            /*Depreacted Converting
            if(positionPoints != null && positionPoints.Count > 0)
            {
                //Converts Old Position Point system to new System
                connections = new List<PointConnector>();
                for (int i = 0; i < positionPoints.Count - 1; i++)
                {
                    connections.Add(new PointConnector(positionPoints[i].GetComponent<PointHandler>(), positionPoints[i + 1].GetComponent<PointHandler>()));
                }

                positionPoints = new List<Transform>();
            }*/

            //Check for Spawn Point
            if (spawnPoint == null)
            {
                var obj = new GameObject();
                obj.AddComponent<PointHandler>();

                spawnPoint = obj.transform;
                spawnPoint.GetComponent<PointHandler>().style = PointHandler.Point.Spawn;
                spawnPoint.parent = transform;
            }
            spawnPoint.name = "Spawn Point";


             /*DEPRECATED!!!!!!!!!!
             if (positionPoints.Count > 2)
             {
                 if (loopedTrack)
                 {
                     //If lap point is lap point in looped track
                     if (positionPoints[positionPoints.Count - 1].GetComponent<PointHandler>().style == PointHandler.Point.Lap)
                     {
                         loopedTrack = false;
                     }
                 }
                 else
                 {
                     //If lap point is lap point in looped track
                     if (positionPoints[positionPoints.Count - 1].GetComponent<PointHandler>().style == PointHandler.Point.Position)
                     {
                         loopedTrack = true;
                     }
                 }
             }*/

            //Check if Track needs to be updated
            bool changeDetected = false;

            if (lastConnectionCount != connections.Count)
                changeDetected = true;

            //Check if every point is at the same position
            if(!changeDetected)
            {
                foreach(PointHandler ph in FindObjectsOfType<PointHandler>())
                    if(ph.transform.position != ph.lastPos || ph.style != ph.lastStyle)
                    {
                        changeDetected = true;
                        break;
                    }
            }

            if (changeDetected)
            {
                CheckConsistency();
#if UNITY_EDITOR
                if (Application.isEditor)
                {
                        TrackWindow window = (TrackWindow)EditorWindow.GetWindow(typeof(TrackWindow));
                        window.Repaint();
                }
#endif
                pointTree = null;
            }
            /*DEPRECATED!!!!!!!!!!

            //Check that Position Points are in the correct format
            int lapCount = 0;
            for (int i = 0; i < positionPoints.Count; i++)
            {
                if (positionPoints[i] == null) //If spot is blank, delete it from the list
                    positionPoints.RemoveAt(i);

                if (i == 0)
                {
                    positionPoints[0].GetComponent<PointHandler>().style = PointHandler.Point.Lap;
                }

                PointHandler.Point point = positionPoints[i].GetComponent<PointHandler>().style;
                switch (point)
                {
                    case PointHandler.Point.Position:
                        positionPoints[i].name = "Position Point " + i;
                        break;
                    case PointHandler.Point.Shortcut:
                        positionPoints[i].name = "Shortcut Point " + i;
                        break;
                    case PointHandler.Point.Lap:
                        positionPoints[i].name = "Lap Point " + i;
                        if(i != 0)
                        lapCount++;
                        break;
                }
            }

            if(!loopedTrack)
                Laps = lapCount;

          //  if (positionPoints.Count == 0)
                //NewPoint();*/
        }
        else if (pointTree == null && trackErrors != null && trackErrors.Count == 0)
        {
            //Generate BSP Tree
            CheckConsistency();

            //Get Rect Size
            Vector2 min = new Vector2(validPointHandlers[0].lastPos.x, validPointHandlers[0].lastPos.z)
                , max = new Vector2(validPointHandlers[0].lastPos.x, validPointHandlers[0].lastPos.z);

            foreach (PointHandler ph in validPointHandlers)
            {
                if (ph.lastPos.x < min.x)
                    min.x = ph.lastPos.x;

                if (ph.lastPos.x > max.x)
                    max.x = ph.lastPos.x;

                if (ph.lastPos.z < min.y)
                    min.y = ph.lastPos.z;

                if (ph.lastPos.z > max.y)
                    max.y = ph.lastPos.z;
            }

            //Generate the BSP Tree
            min -= new Vector2(100, 100);
            max += new Vector2(100, 100);

            pointTree = new BSPTree(0, new Rect(min.x, min.y, max.x - min.x, max.y - min.y));

            foreach (PointHandler ph in validPointHandlers)
                pointTree.Insert(ph);

            Debug.Log("Generated Point Tree");
        }
    }

    //Checks that network is suitable
    private void CheckConsistency()
    {
        //Reset Track Errors
        trackErrors = new List<TrackErrors>();

        //Get list of every Point Handler
        List<PointHandler> pointHandlers = FindObjectsOfType<PointHandler>().ToList();

        //Send errors if needed
        if (pointHandlers.Count < 4)
        {
            trackErrors.Add(TrackErrors.NotEnoughPoints);
            return;
        }

        //Reset Point Handlers
        foreach (PointHandler ph in pointHandlers)
        {
            //Reset every point handlers done marker
            ph.visitedPoint = false;
            //Reset every point handlers main route marker
            ph.usedByMainRoute = false;
            //Reset every point handlers percentage value
            ph.percent = -1f;
            //Reset every point handlers last position value
            ph.lastPos = ph.transform.position;
            //Reset every point handlers last style value
            ph.lastStyle = ph.style;
            //Reset Point Handlers list of connections
            ph.connections = new List<PointHandler>();
        }

        List<PointHandler> validPoints = new List<PointHandler>();

        //Check for Null Point Handlers in connections
        foreach (PointConnector pointConnector in connections.ToArray())
        {
            //Delete any conenctions with a null handlers
            if (pointConnector.a == null || pointConnector.b == null)
                connections.Remove(pointConnector);
            else
            { 
                //Otherwise add them to a list of valid points
                if(!validPoints.Contains(pointConnector.a))
                    validPoints.Add(pointConnector.a);
                if (!validPoints.Contains(pointConnector.b))
                    validPoints.Add(pointConnector.b);

                pointConnector.a.connections.Add(pointConnector.b);
                pointConnector.b.connections.Add(pointConnector.a);
            }
        }

        //Set Last Connection Count
        lastConnectionCount = connections.Count;

        List<PointHandler> startPoints = new List<PointHandler>(), endPoints = new List<PointHandler>();

        int lapCount = 0;

        //If a point isn't a valid one then delete it
        foreach (PointHandler ph in pointHandlers.ToArray())
        {
            if (!validPoints.Contains(ph))
            {
                pointHandlers.Remove(ph);

              //  if(ph.style != PointHandler.Point.Spawn)
                  //  DestroyImmediate(ph.gameObject);
            }
            else
            {
                //Check there is only one start and one end point
                if (ph.style == PointHandler.Point.Start)
                    startPoints.Add(ph);
                if (ph.style == PointHandler.Point.End)
                {
                    endPoints.Add(ph);
                    lapCount++;
                }

                if (ph.style == PointHandler.Point.Lap)
                    lapCount++;
            }
        }

        //Error Testing
        if(startPoints.Count == 0)
        {
            trackErrors.Add(TrackErrors.NoStartPoint);
            return;
        }

        if (startPoints.Count > 1)
        {
            trackErrors.Add(TrackErrors.TooManyStartPoints);
            return;
        }

        if (endPoints.Count == 0)
        {
            trackErrors.Add(TrackErrors.NoEndPoint);
            return;
        }

        if (endPoints.Count > 1)
        {
            trackErrors.Add(TrackErrors.TooManyEndPoints);
            return;
        }

        //Find Connection from End to Start
        bool foundLink = false;
        foreach(PointConnector pc in connections)
        {
            if((pc.a == startPoints[0] && pc.b == endPoints[0]) || (pc.a == endPoints[0] && pc.b == startPoints[0]))
            {
                foundLink = true;
                break;
            }
        }

        //If we an end point this is a non-looped track
        if (!foundLink)
        {
            loopedTrack = false;
            Laps = lapCount;
        }
        else
            loopedTrack = true;

        //Check that there are no points with only one connection or any loose ends
        if(!CheckForLooseEnds(startPoints[0]))
        {
            return;
        }

        //Reset every point handlers done marker
        foreach (PointHandler ph in pointHandlers)
            ph.visitedPoint = false;

        //Calculate route to the finish
        if (!CalculatePercent(startPoints[0], validPoints))
        {
            return;
        }

        validPointHandlers = pointHandlers;
    }

    //Checks that there are no strands
    private bool CheckForLooseEnds(PointHandler startPoint)
    {
        //Use Recursion, mark Point Handler as done if visited
        startPoint.visitedPoint = true;

        //Check that there are no stray points (Only in 1 Connection, Not Lap Points), reject
        bool checkForConnections = startPoint.style == PointHandler.Point.Shortcut || startPoint.style == PointHandler.Point.Position || startPoint.style == PointHandler.Point.Lap;

        if(checkForConnections && startPoint.connections.Count <= 1) 
        {
            trackErrors.Add(TrackErrors.PointWithNoEnd);
            return false;
        }

        //If Lap Point found with 1 connection then presume non looped track
        if(startPoint.style == PointHandler.Point.Lap && loopedTrack)
        {
            trackErrors.Add(TrackErrors.LapPointOnLoopedTrack);
            return false;
        }

        foreach(PointHandler nextPoint in startPoint.connections)
        {
            //If it's not been visited visit each connection, if it fails, then fail this
            if (!nextPoint.visitedPoint && !CheckForLooseEnds(nextPoint))
                return false;
        }

        return true;
    }

    //Gives each Point Handler a percentage representing how far along the course they are
    private bool CalculatePercent(PointHandler startPoint, List<PointHandler> validPoints)
    {
        //Custom route class that contains a list of points and the points overall distance
        //Use Recursion to map every route through the track
        List<Route> routes = new List<Route>();
        routes.Add(new Route());

        if(!FindRoutes(startPoint, routes, 0))
        {         
            //We've got a route that didn't hit the end
            return false;
        }

        //Clear Null Routes
        while (routes.Contains(null))
            routes.Remove(null);

        Debug.Log("Valid Routes: " + routes.Count);

        //Longest route is main track 0% - 100%
        Route mainRoute = routes[0];

        foreach(Route route in routes)
            if(route.length > mainRoute.length)
                mainRoute = route;

        //Check Lap Points
        foreach(PointHandler ph in validPoints)
        {
            if (ph.style == PointHandler.Point.Lap)
            {
                if (loopedTrack)
                {
                    //There should be no lap points in a looped track
                    ph.style = PointHandler.Point.Position;
                }
                else
                {
                    //If there is a Lap point not on the main route, reject!
                    if (!mainRoute.points.Contains(ph))
                    {
                        trackErrors.Add(TrackErrors.LapNotOnMainRoute);
                        return false;
                    }
                }
            }
        }

        //Assign each point in main track a percentage based on distance, and set it's main route marker to true, name each point and set it's position in the track manager
        int count = 0;
        float currentLength = 0f;

        //If a looped track, Add the start point at end to get actual length
        if (loopedTrack)
            mainRoute.AddPoint(mainRoute.points[0]);

        bool doneStart = false;
        foreach (PointHandler ph in mainRoute.points)
        {
            if (ph.style != PointHandler.Point.Start || !doneStart)
            {
                doneStart = true;

                ph.usedByMainRoute = true;
                ph.percent = currentLength / mainRoute.length;

                ph.transform.name = "Main Route " + count;

                ph.transform.parent = transform;
                ph.transform.SetSiblingIndex(count);

                count++;

                if (count < mainRoute.points.Count)
                    currentLength += (MathHelper.ZeroYPos(mainRoute.points[count].transform.position) - MathHelper.ZeroYPos(mainRoute.points[count - 1].transform.position)).magnitude;
            }
        }

        //For each other route
        int routeCount = 1, routeChildCount = 0;

        while (routes.Count > 0)
        {
            foreach (Route route in routes.ToArray())
            {
                if (route != mainRoute)
                {
                    int startValue = -1;

                    //Find points with no percentage, if none skip for now
                    for (int i = 1; i < route.points.Count; i++)
                    {
                        if (route.points[i].percent == -1)
                        {
                            if (startValue == -1)
                                startValue = i-1;
                        }
                        else if(startValue >= 0)
                        {
                            int endValue = i;
                            float currentSectionPercent = route.points[startValue].percent,
                                sectionLength = route.CalculateLengthAt(startValue, endValue);

                            PointHandler start = route.points[startValue], end = route.points[endValue];

                            float sectionPercent = end.percent - start.percent;

                            routeCount++;

                            //Calculate percent values based on nodes that do have percents
                            for (int j = startValue + 1; j < endValue; j++)
                            {
                                route.points[j].percent = currentSectionPercent + ((route.CalculateLengthAt(startValue, j) / sectionLength) * sectionPercent);

                                route.points[j].transform.name = "Optional Route " + routeCount + " " + (j - startValue).ToString();

                                route.points[j].transform.parent = transform;

                                int siblingIndex = mainRoute.points.Count + routeChildCount + (j - startValue);
                                route.points[j].transform.SetSiblingIndex(siblingIndex);
                            }

                            startValue = -1;
                        }

                        if (startValue == -1)
                        {
                            routes.Remove(route);
                            routeChildCount += route.points.Count;
                        }
                    }
                }
                else
                {
                    routes.Remove(route);
                }
            }
        }

            return true;
    }

    private bool FindRoutes(PointHandler startPoint, List<Route> routes, int myRoute)
    {
        //Use Recursion to map every route through the track
        routes[myRoute].AddPoint(startPoint);

        Route routeSoFar = new Route(routes[myRoute]);

        //Set visited mark to true
        startPoint.visitedPoint = true;

        bool foundExit = false;
        int possibleRouteCount = 0;

        for (int i = 0; i < startPoint.connections.Count; i++)
        {
            bool exitOnThisNode = false;
            PointHandler otherPoint = startPoint.connections[i];

            //Check if this node is the end
            if (!loopedTrack && otherPoint.style == PointHandler.Point.End)
            {
                routes[myRoute].AddPoint(otherPoint);
                foundExit = true;
                exitOnThisNode = true;
            }
            else if (loopedTrack && otherPoint.style == PointHandler.Point.End && routes[myRoute].length >= 3)
            {
                routes[myRoute].AddPoint(otherPoint);
                foundExit = true;
                exitOnThisNode = true;
            }

            bool valid = true;

            //If one way, check that this point is point A
            if(startPoint.oneWay)
            {
                foreach(PointConnector connection in connections)
                {
                    if (connection.a == startPoint && connection.b == otherPoint)
                        break;
                    else if (connection.b == startPoint && connection.a == otherPoint)
                    {
                        valid = false;
                        break;
                    }
                }
            }

            if (!exitOnThisNode && !otherPoint.visitedPoint && valid)
            {
                int toRoute = myRoute;
                possibleRouteCount++;

                //Create a New Route if needed
                if(possibleRouteCount >= 2)
                {
                    routes.Add(routeSoFar);
                    toRoute = routes.Count - 1;
                }

                if (FindRoutes(otherPoint, routes, toRoute))
                    foundExit = true;
            }
        }

        //Set visited mark to false, incase we come here again as part of a different route
        startPoint.visitedPoint = false;

        //If we've looped back around and not found the end we've got a useless piece of track
        if (!foundExit)
        {
            routes[myRoute] = null;
            return false;
        }

        //Return if an exit was eventually found (Should always be true)
        return true;
    }

    [System.Serializable]
    public class Route
    {
        public List<PointHandler> points;
        public float length { get; private set; }

        public Route()
        {
            points = new List<PointHandler>();
            length = 0f;
        }

        public Route(Route existingRoute)
        {
            points = new List<PointHandler>();

            foreach (PointHandler point in existingRoute.points)
                points.Add(point);

            length = existingRoute.length;
        }

        public void AddPoint(PointHandler newPoint)
        {
            points.Add(newPoint);

            if (points.Count >= 2)
            {
                length += (MathHelper.ZeroYPos(points[points.Count - 1].lastPos) - MathHelper.ZeroYPos(points[points.Count - 2].lastPos)).magnitude;
            }
        }

        public void CalculateLength()
        {
            length = 0f;

            for(int i = 0; i < points.Count - 1; i++)
                length += (MathHelper.ZeroYPos(points[i + 1].lastPos) - MathHelper.ZeroYPos(points[i].lastPos)).magnitude;
        }

        public float CalculateLengthAt(int start, int end)
        {
            float returnValue = 0f;

            for (int i = start; i < end; i++)
                returnValue += (points[i + 1].transform.position - points[i].transform.position).magnitude;

            return returnValue;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }
}

[System.Serializable]
public class CameraPoint
{
    public Vector3 startPoint, endPoint, startRotation, endRotation;
    public float travelTime = 3f;
}

[System.Serializable]
public class PointConnector
{
    public PointHandler a, b;

    public PointConnector(PointHandler _a, PointHandler _b)
    {
        a = _a;
        b = _b;
    }
}

public class BSPTree
{
    const int maxObjectsPerLeaf = 10, maxLevels = 10;

    public int level { get; private set; }
    public List<PointHandler> leafs { get; private set; }
    public Rect aabbBounds { get; private set; }
    public BSPTree[] nodes;

    //Constructor
    public BSPTree(int _level, Rect _aabbBounds)
    {
        level = _level;
        leafs = new List<PointHandler>();
        aabbBounds = _aabbBounds;
        nodes = new BSPTree[4];
    }

    //Useful Functions

    /// <summary>
    /// Clears the Collision Tree and all it's Splits
    /// </summary>
    public void Clear()
    {
        leafs.Clear();

        for (int i = 0; i < nodes.Length; i++)
        {
            BSPTree node = nodes[i];

            if (node != null)
            {
                node.Clear();
                nodes[i] = null;
            }
        }
    }

    /// <summary>
    /// Splits a Tree into 4 SubTrees
    /// </summary>
    public void Split()
    {
        int splitWidth = (int)(aabbBounds.width / 2f), splitHeight = (int)(aabbBounds.height / 2f);
        int x = (int)aabbBounds.x, y = (int)aabbBounds.y;

        nodes[0] = new BSPTree(level + 1, new Rect(x, y, splitWidth, splitHeight));
        nodes[1] = new BSPTree(level + 1, new Rect(x + splitWidth, y, splitWidth, splitHeight));
        nodes[2] = new BSPTree(level + 1, new Rect(x, y + splitHeight, splitWidth, splitHeight));
        nodes[3] = new BSPTree(level + 1, new Rect(x + splitWidth, y + splitHeight, splitWidth, splitHeight));
    }

    private int[] GetIndex(Rect possibleRect)
    {
        List<int> quadrents = new List<int>();

        if (nodes != null)
        {
            for (int i = 0; i < 4; i++)
            {
                if (nodes[i] != null)
                {
                    Rect checkRect = nodes[i].aabbBounds;

                    if (checkRect.Contains(new Vector2(possibleRect.x, possibleRect.y)) ||
                        checkRect.Contains(new Vector2(possibleRect.x + possibleRect.width, possibleRect.y)) ||
                        checkRect.Contains(new Vector2(possibleRect.x, possibleRect.y + possibleRect.height)) ||
                        checkRect.Contains(new Vector2(possibleRect.x + possibleRect.width, possibleRect.y + possibleRect.height)))
                        quadrents.Add(i);
                }
            }
        }

        return quadrents.ToArray();
    }

    public void Insert(PointHandler point)
    {
        if (nodes[0] != null)
        {
            Vector3 position = point.transform.position;
            int[] index = GetIndex(new Rect(position.x, position.y, 1f, 1f));

            if (index.Length == 1)
            {
                nodes[index[0]].Insert(point);
                return;
            }
        }

        leafs.Add(point);

        if (leafs.Count > maxObjectsPerLeaf && level < maxLevels)
        {
            if (nodes[0] == null)
                Split();

            for (int i = 0; i < leafs.Count; i++)
            {
                Vector3 position = leafs[i].transform.position;
                int[] index = GetIndex(new Rect(position.x, position.y, 1f, 1f));

                if (index.Length == 1)
                {
                    nodes[index[0]].Insert(leafs[i]);
                    leafs.RemoveAt(i);

                    i--;
                    continue;
                }
            }
        }
    }

    public List<PointHandler> Retrieve(List<PointHandler> returnList, Vector3 position)
    {
        Rect rect = new Rect(position.x - 5, position.y - 5, 10, 10);

        int[] index = GetIndex(rect);

       for(int i = 0; i < index.Length; i++)
        {
            nodes[index[i]].Retrieve(returnList, position);
        }

        returnList.AddRange(leafs);

        return returnList;
    }

}