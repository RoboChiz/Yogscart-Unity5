using UnityEngine;
using System.Collections;

//Character Skeleton
//By Robo_Chiz V2

public class Character_Skeleton : MonoBehaviour {

    //Locations of the axles for each wheel.
    public Vector3 SeatPosition; //Represented by a Cyan Box
    public Transform HatHolder; //Represented by a Magenta Box

	void OnDrawGizmos()
    {

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position + SeatPosition, 0.1f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawCube(HatHolder.position, new Vector3(0.5f, 0.05f, 0.5f));

    }
}
