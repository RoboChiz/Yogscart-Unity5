#pragma strict

//Kart Skeleton
//By Robo_Chiz V1

//Designed to hold basic information about the kart a custom kart can then be created. This has been done to avoid creating prefabs for each combination.

//Locations of the axles for each wheel.
var FrontLPosition : Vector3; //Represented by a Blue Box
var FrontRPosition : Vector3; //Represented by a Red Box

var BackLPosition : Vector3; //Represented by a Green Box
var BackRPosition : Vector3; //Represented by a Pink Box

//Location of seat for Character Spawning
var SeatPosition : Vector3; //Represented by a Grey Box

var ItemDrop : float = 2f; //Represented by a Yellow Box

var engineSound : AudioClip;

	function OnDrawGizmos() {
		
		var WheelRadius : float = 0.2f;
		var WheelWidth : float = 0.05f;
		
		var ChairSize : float = 0.5f;
		var ChairWidth : float = 0.05f;
		
		Gizmos.color = Color.blue;
		Gizmos.DrawCube(transform.position + FrontLPosition,Vector3(WheelWidth,WheelRadius,WheelRadius));
		
		Gizmos.color = Color.red;
		Gizmos.DrawCube(transform.position + FrontRPosition,Vector3(WheelWidth,WheelRadius,WheelRadius));
		
		Gizmos.color = Color.green;
		Gizmos.DrawCube(transform.position + BackLPosition,Vector3(WheelWidth,WheelRadius,WheelRadius));
		
		Gizmos.color = Color.magenta;
		Gizmos.DrawCube(transform.position + BackRPosition,Vector3(WheelWidth,WheelRadius,WheelRadius));
		
		Gizmos.color = Color.gray;
		Gizmos.DrawCube(transform.position + SeatPosition,Vector3(ChairSize,ChairWidth,ChairSize));
		
		Gizmos.color = Color.yellow;
		Gizmos.DrawCube(transform.position - (transform.forward*ItemDrop),Vector3(ChairSize,ChairSize,ChairSize));
		
	}