#pragma strict

var style : Point = Point.Position;

function OnDrawGizmos() {
	if(style == Point.Position)
		Gizmos.color = Color.red;
	if(style == Point.Lap)	
		Gizmos.color = Color.blue;
	if(style == Point.Shortcut)	
		Gizmos.color = Color.green;
	if(style == Point.Spawn)	
		Gizmos.color = Color.yellow;
			
	Gizmos.DrawSphere (transform.position, 0.75f);
	
	}