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

    public enum TrackErrors { PointWithNoEnd, LapNotOnMain, RouteSkipsLap, NoStartPoint, NoEndPoint, TooManyStartPoints, TooManyEndPoints, LapPointOnLoopedTrack, NotEnoughPoints, BadTrackDesignPointlessLoop };
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
    public List<Transform> positionPoints; //Depracted Used to copy over existing layouts

    public List<PointConnector> connections;
    private int lastConnectionCount;

    public List<CameraPoint> introPans;

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            transform.name = "Track Manager";

            //Fix Intro Pans if null for some reason
            if (introPans == null)
                introPans = new List<CameraPoint>();

            //Depreacted Converting
            if(positionPoints != null && positionPoints.Count > 0)
            {
                //Converts Old Position Point system to new System
                connections = new List<PointConnector>();
                for (int i = 0; i < positionPoints.Count - 1; i++)
                {
                    connections.Add(new PointConnector(positionPoints[i].GetComponent<PointHandler>(), positionPoints[i + 1].GetComponent<PointHandler>()));
                }

                positionPoints = new List<Transform>();
            }

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


                  //DEPRECATED!!!!!!!!!!
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
             }

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
                    TrackWindow window = (TrackWindow)EditorWindow.GetWindow(typeof(TrackWindow));
                    window.Repaint();
                #endif
            }

            //DEPRECATED!!!!!!!!!!

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
                //NewPoint();
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
                    endPoints.Add(ph);
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
            loopedTrack = false;
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
        if (!CalculatePercent(startPoints[0]))
        {
            return;
        }
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
        if(startPoint.style == PointHandler.Point.Lap && !loopedTrack)
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
    private bool CalculatePercent(PointHandler startPoint)
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
        //If there is a Lap point not on the main route, reject!
        //Assign each point in main track a percentage based on distance, and set it's main route marker to true, name each point and set it's position in the track manager

        //Check if looped that we return to the start?
        //Check if not looped that we go from the start to the end node

        //for each other route
        //Find points with no percentage, if none skip for now
        //calculate percent values based on nodes that do have percents
        //If there is a route that skips a lap point, reject

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

            //Check if this node is the end
            if (!loopedTrack && startPoint.connections[i].style == PointHandler.Point.End)
            {
                routes[myRoute].AddPoint(startPoint.connections[i]);
                foundExit = true;
                exitOnThisNode = true;
            }
            else if (loopedTrack && startPoint.connections[i].style == PointHandler.Point.End && routes[myRoute].length >= 3)
            {
                routes[myRoute].AddPoint(startPoint.connections[i]);
                foundExit = true;
                exitOnThisNode = true;
            }

            if (!exitOnThisNode && !startPoint.connections[i].visitedPoint)
            {
                int toRoute = myRoute;
                possibleRouteCount++;

                //Create a New Route if needed
                if(possibleRouteCount >= 2)
                {
                    routes.Add(routeSoFar);
                    toRoute = routes.Count - 1;
                }

                if (FindRoutes(startPoint.connections[i], routes, toRoute))
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