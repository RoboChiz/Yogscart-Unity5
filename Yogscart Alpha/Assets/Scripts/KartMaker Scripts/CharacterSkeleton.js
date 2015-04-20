#pragma strict

//Character Skeleton
//By Robo_Chiz V1

//Locations of the axles for each wheel.
var SeatPosition : Vector3; //Represented by a Cyan Box
var HatHolder : Transform; //Represented by a Magenta Box

	function OnDrawGizmos() {
		
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(transform.position + SeatPosition,0.1);
		
		Gizmos.color = Color.magenta;
		Gizmos.DrawCube(HatHolder.position,Vector3(0.5,0.05,0.5));
			
	}