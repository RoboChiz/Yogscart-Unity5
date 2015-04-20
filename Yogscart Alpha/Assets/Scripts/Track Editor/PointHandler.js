#pragma strict

var style : Point = Point.Position;

function OnDrawGizmos() {
	if(style == Point.Position)
		Gizmos.color = Color.green;
	if(style == Point.Lap)	
		Gizmos.color = Color.blue;
	if(style == Point.Shortcut)	
		Gizmos.color = Color.red;
			
	Gizmos.DrawSphere (transform.position, 0.75f);
	
	if(transform.name != style.ToString())
		transform.name = style.ToString();
	
	}