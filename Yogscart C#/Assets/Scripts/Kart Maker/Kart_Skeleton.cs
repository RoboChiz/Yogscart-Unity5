using UnityEngine;
using System.Collections;

//Kart Skeleton
//By Robo_Chiz V2

/*Designed to hold basic information about the kart a custom kart can then be created. 
This has been done to avoid creating prefabs for each combination.*/


public class Kart_Skeleton : MonoBehaviour
{

    public Vector3 FrontLPosition, FrontRPosition, BackLPosition, BackRPosition, SeatPosition;
    public Transform leftHandTarget, rightHandTarget, leftFootTarget, rightFootTarget;

    public float ItemDrop = 3f;
    public AudioClip engineSound;

    void OnDrawGizmos()
    {

        float WheelRadius = 0.2f;
        float WheelWidth = 0.05f;

        float ChairSize = 0.5f;
        float ChairWidth = 0.05f;

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(transform.position + FrontLPosition, new Vector3(WheelWidth, WheelRadius, WheelRadius));

        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position + FrontRPosition, new Vector3(WheelWidth, WheelRadius, WheelRadius));

        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position + BackLPosition, new Vector3(WheelWidth, WheelRadius, WheelRadius));

        Gizmos.color = Color.magenta;
        Gizmos.DrawCube(transform.position + BackRPosition, new Vector3(WheelWidth, WheelRadius, WheelRadius));

        Gizmos.color = Color.gray;
        Gizmos.DrawCube(transform.position + SeatPosition, new Vector3(ChairSize, ChairWidth, ChairSize));

        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position - (transform.forward * ItemDrop), new Vector3(ChairSize, ChairSize, ChairSize));

    }

}
