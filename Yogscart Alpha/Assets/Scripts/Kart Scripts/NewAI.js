#pragma strict

private var ks : kartScript;
private var td : TrackData;
private var pf : Position_Finding;

var angleRequired : float = 1f;
var turnSpeed : float = 17f;
var turnAngle : float = 45f;

var turnPoint : Transform;

function Awake() {

	ks = transform.GetComponent(kartScript);
	td = GameObject.Find("Track Manager").GetComponent(TrackData);
	pf = transform.GetComponent(Position_Finding);
}

function FixedUpdate () {

	ks.throttle = 1;
	
	var currentPos : int = pf.currentPos;
	var transformPos : Vector3 = transform.position;
	transformPos.y = 0;
	
	var pointTotal = td.PositionPoints.Length;
	
	var nextPoint : Vector3 = td.PositionPoints[NumClamp(currentPos+1,0,pointTotal)].position;
	nextPoint.y = 0;
	
	var currentPoint : Vector3 = td.PositionPoints[currentPos].position;
	currentPoint.y = 0;
	
	var desiredDirection = nextPoint - currentPoint;
	desiredDirection.y = 0;
	
	
	Debug.DrawRay(transform.position,desiredDirection,Color.green);
	
	var fireDirection = transform.right;
	fireDirection.y = 0;

	var angle : float = Vector3.Angle(fireDirection,desiredDirection);
	var turnRequired : int = CheckAngle(angle);

	if(turnPoint == null)
	{
		turnPoint = td.PositionPoints[NumClamp(currentPos+1,0,pointTotal)];	
	}
	
	if(turnPoint != null)
	{
	
		var tpDistance = Vector3.Distance(transformPos,turnPoint.position);
		
		//If turn within turn range start to turn
		if(tpDistance < 7.5f)
		{
			ks.steer = turnRequired;	
			SlowDownCar(angle);
				
			if(turnPoint == td.PositionPoints[currentPos])
			{
				turnPoint = null;
			}
				
		}	
		else
		{
		
			//If not within range of next point, aim kart towards the point	
			var NeededDirection : Vector3 = turnPoint.position - transformPos;
			NeededDirection.y = 0;
			
			var nAngle : float = Vector3.Angle(fireDirection,NeededDirection);
			var nTurnRequired : int = CheckAngle(nAngle);
			
			ks.steer = nTurnRequired;	
			SlowDownCar(nAngle);
			
		}
		
	}
	
}

function SlowDownCar (angle : float)
{
	if(angle > turnAngle)
	{
		if(ks.actualSpeed >= turnSpeed)
			ks.throttle = 0;	
	}
}

function CheckAngle(angle : float)
{
	if(angle > 90 + angleRequired)
		return -1;
	else if(angle < 90 - angleRequired)
		return 1;
	else 
		return 0;
}

function NumClamp(val : int,min : int,max : int){

while(val > max-1)
val -= (max-min);

while(val < min)
val += (max-min);


return val;

}