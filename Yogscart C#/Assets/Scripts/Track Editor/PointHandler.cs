using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PointHandler : MonoBehaviour
{
    public float roadWidth = 5f;

    public enum Point { Position, Start, End, Lap, Shortcut, Spawn };
    public Point style;

    //Used to detect changes
    [HideInInspector, System.NonSerialized]
    public Vector3 lastPos = Vector3.zero;
    [HideInInspector, System.NonSerialized]
    public Point lastStyle;

    //Has this point been checked by recursion
    [HideInInspector, System.NonSerialized]
    public bool visitedPoint;

    //Is this point part of the main route
    [HideInInspector]
    public bool usedByMainRoute;

    //How far along the final track is this node
    [HideInInspector]
    public float percent;

    //Has this point been checked by recursion
    [HideInInspector, System.NonSerialized]
    public List<PointHandler> connections;

    // Used to draw Sphere on point
    void OnDrawGizmos()
    {
        switch (style)
        {
            case Point.Position:
                Gizmos.color = Color.red;
                break;
            case Point.Lap:
                Gizmos.color = Color.blue;
                break;
            case Point.Shortcut:
                Gizmos.color = Color.green;
                break;
            case Point.Spawn:
                Gizmos.color = Color.yellow;
                break;
            case Point.Start:
                Gizmos.color = Color.cyan;
                break;
            case Point.End:
                Gizmos.color = Color.cyan;
                break;
        }

        Gizmos.DrawSphere(transform.position, 0.75f);
    }

}
