#pragma strict

var target : Transform;
@HideInInspector
var parent : Transform;

private var td : TrackData;
private var pf : Position_Finding;

function Start(){

	td = GameObject.Find("Track Manager").transform.GetComponent(TrackData);
	pf = transform.GetComponent(Position_Finding);
	
	parent = transform.parent;
	pf.Lap = 0;
	pf.currentPos = parent.GetComponent(Position_Finding).currentPos;

	
//Wait until you've been fire then choose a target
	while(transform.parent != null)
		yield;

	var objs = GameObject.FindObjectsOfType(kartScript);
	
	for(var i : int; i < objs.Length; i++)
	{
		if(objs[i].transform != parent && objs[i].GetComponent(Position_Finding).position == (parent.GetComponent(Position_Finding).position - 1))
		{
			target = objs[i].transform;
			break;
		}
	}

}

function FixedUpdate ()
{
	if(target != null)
	{
		var egg : Egg = transform.GetComponent(Egg);

		var dir : Vector3 = target.position - transform.position;
		var hit : RaycastHit;

		if(Physics.Raycast(transform.position,dir,hit,dir.magnitude) && hit.transform == target)
		{
			egg.direction = (target.position - transform.position).normalized;
		}
		else
		{
			var nextChump : int;
			if(pf.currentPos+1 < td.PositionPoints.Length)
			nextChump = pf.currentPos+1;
			else
			nextChump = 0;

			var Target : Vector3 = td.PositionPoints[nextChump].position;

			egg.direction = (Target - transform.position).normalized;
		}
	}
}