using UnityEngine;
using System.Collections;

public class PointHandler : MonoBehaviour
{

    public float roadWidth = 5f;

    public enum Point { Position, Lap, Shortcut, Spawn };
    public Point style;

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
        }

        Gizmos.DrawSphere(transform.position, 0.75f);
    }

}
