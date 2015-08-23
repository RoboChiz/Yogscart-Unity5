#pragma strict

private var ks : kartScript;
private var ki : kartItem;
private var gd : CurrentGameData;

private var td : TrackData;
private var pf : Position_Finding;

var Stupidity : int; //Bigger the number, stupider the AI.

private var angleRequired : float = 3f;
private var turnSpeed : float = 15f;
private var turnAngle : float = 30f;

private var reverseDistance : float = 10f;

private var turnPoint : Transform;
private var turnPointInt : int;

private var reversing : boolean = false;
private var startDrive : boolean = true;

var adjusterFloat : float = -999;

private var iteming : boolean;

function Awake() {

	ks = transform.GetComponent(kartScript);
	ki = transform.GetComponent(kartItem);
	td = GameObject.Find("Track Manager").GetComponent(TrackData);
	pf = transform.GetComponent(Position_Finding);
	gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
	
	if(ks.locked)
		startDrive = false;
}

function FixedUpdate () {

	//Wait till positions are set to create Adjuster
	if(adjusterFloat <= -999 && pf.position != -1)
	{
		adjusterFloat = 5 - ((pf.position % 3)*5);
	}
	
	var currentPos : int = pf.currentPos;
	var transformPos : Vector3 = transform.position;
	transformPos.y = 0;
	
	var pointTotal = td.PositionPoints.Length;
	
	var nextPoint : Vector3 = td.PositionPoints[NumClamp(currentPos+1,0,pointTotal)].position;
	nextPoint.y = 0;
	
	var nextNextPoint : Vector3 = td.PositionPoints[NumClamp(currentPos+2,0,pointTotal)].position;
	nextNextPoint.y = 0;
	
	var currentPoint : Vector3 = td.PositionPoints[currentPos].position;
	currentPoint.y = 0;
	
	var desiredDirection = nextPoint - currentPoint;
	desiredDirection.y = 0;
	
	var fireDirection = transform.right;
	fireDirection.y = 0;

	var angle : float = Vector3.Angle(fireDirection,desiredDirection);
	var turnRequired : int = CheckAngle(angle);

	if(startDrive)
		ks.throttle = 1f;
	else
		ks.throttle = 0f;
	
	if(turnPointInt == currentPos)
	{
		turnPoint = null;
	}
	
	if(turnPoint == null)
	{
	
		turnPoint = td.PositionPoints[NumClamp(currentPos+1,0,pointTotal)];	
		turnPointInt = NumClamp(currentPos+1,0,pointTotal);
		
		if(adjusterFloat > -999)
		{
			adjusterFloat += Random.Range(-1f,1f);
			
			var limit : float = turnPoint.GetComponent(PointHandler).roadWidth;
			
			adjusterFloat = Mathf.Clamp(adjusterFloat,-limit,limit);
		}
		
	}
	
	if(turnPoint != null)
	{
	
		var tpDistance = Vector3.Distance(transformPos,turnPoint.position);
		
		//If turn within turn range start to turn
		if(tpDistance < 7.5f && !reversing)
		{
		
			SlowDownCar(Vector3.Angle(transform.forward,desiredDirection));
			
			Debug.DrawRay(transform.position,desiredDirection,Color.green);	
				
		}	
		else
		{
		
			//If not within range of next point, aim kart towards the point	
			
			var Adjuster = Vector3.Cross((turnPoint.position-currentPoint).normalized,transform.up) * adjusterFloat;
			
			Debug.DrawLine(transform.position,turnPoint.position + Adjuster,Color.green);
			
			var NeededDirection : Vector3 = (turnPoint.position + Adjuster) - transformPos;
			NeededDirection.y = 0;
			
			var nAngle : float = Vector3.Angle(fireDirection,NeededDirection);
			var nTurnRequired : int = CheckAngle(nAngle);
			
			turnRequired = nTurnRequired;	
			SlowDownCar(Vector3.Angle(transform.forward,NeededDirection));
			
		}
		
	}
	
	//Reverse if Kart hits somethings
	if(ks.expectedSpeed > 5 && ks.actualSpeed < 1) //Presume something is blocking the kart.
		reversing = true;
		
	if(reversing)
	{
		turnRequired = Mathf.Sign(-turnRequired);
		ks.throttle = -1;
		
		var checkPos = transform.position + Vector3.up;
		
		if(!Physics.Raycast(checkPos,transform.forward,reverseDistance)&&!Physics.Raycast(checkPos + transform.right,transform.forward,reverseDistance)&&!Physics.Raycast(checkPos - transform.right,transform.forward,reverseDistance))
			reversing = false;
	}
	
	//Handles Start Boosting
	if(ks.startBoostVal != -1)
	{
		if((Stupidity < 4 && ks.startBoostVal <= 2)||(Stupidity > 8 && ks.startBoostVal <= 3))
			ks.throttle = 1;
			
		if(ks.startBoostVal <= 1)
			startDrive = true;	
			
	}
	
	ks.steer = turnRequired;
	
	if(ki.heldPowerUp != -1)
	{
		if(!iteming)
			DoItems();
	}
	else
		ki.input = false;
	
}

function DoItems()
{

	iteming = true;
	
	if(gd.PowerUps[ki.heldPowerUp].type == ItemType.AffectsOther)
	{
		if(pf.position > 0) //Only use items that effects others if you aren't in 1st
		{
			UseRandomItem(10f);
		}
	}	
	else if(gd.PowerUps[ki.heldPowerUp].type == ItemType.AffectsPlayer)
	{
		//Wait till kart is on a straight
		while(ks.steer != 0)
			yield;
			
		UseRandomItem(5f);
		
	}
	else if(gd.PowerUps[ki.heldPowerUp].type == ItemType.Projectile)
	{
	
		if(Stupidity > 8f)
		{
			UseRandomItem(10f);
		}
		else if(Stupidity > 4f)
		{
		
			var rh : RaycastHit;
		
			if(Physics.Raycast(transform.position,transform.forward,rh,15f))
			{
				if(rh.transform.GetComponent(kartScript) != null)
				{
					UseRandomItem(0f);
				}
			
			}
			
			if(gd.PowerUps[ki.heldPowerUp].usableShield && Stupidity <= 4f) //Use item as shield
				ki.input = true;
		
		}
	}
	
	iteming = false;
	
}

function UseRandomItem(maxVal : float)
{

	yield WaitForSeconds(Random.Range(0,maxVal));
	
	ki.input = true;
	
	yield;
	yield;
	
	ki.input = false;
	
}

function SlowDownCar (angle : float)
{

	if(angle > turnAngle)
	{
			//Slow down to make turn
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