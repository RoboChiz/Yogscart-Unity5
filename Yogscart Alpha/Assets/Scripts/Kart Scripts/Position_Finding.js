#pragma strict

var position : int = -1;

var Lap : int = -1;
var currentPos : int;
var currentTotal : int;
var currentDistance : float;

var locked : boolean;

private var tm : TrackData;

function Start(){

	while(GameObject.Find("Track Manager") == null)
		yield;
		
	tm = GameObject.Find("Track Manager").transform.GetComponent(TrackData);
}

function Update () {
	if(tm != null)
	{
	
		var closestDistance : float = Mathf.Infinity;
		closestDistance = Vector3.Distance(transform.position,tm.PositionPoints[NumClamp(currentPos,0,tm.PositionPoints.Length)].position);

		CheckForward(closestDistance);
		CheckBackwards(closestDistance);

		if(Lap == -1 && currentPos > 1)
		{
		currentPos = 0;
		currentTotal = 0;
		}
		else
		currentPos = NumClamp(currentPos,0,tm.PositionPoints.Length);

		currentDistance = Vector3.Distance(transform.position,tm.PositionPoints[NumClamp(currentPos + 1,0,tm.PositionPoints.Length)].position);


		if(tm.PositionPoints[currentPos].GetComponent(PointHandler).style == Point.Lap)
		{

			var Position1 : Vector3 = tm.PositionPoints[currentPos].position;
			var Position2 : Vector3;
			
			if(currentPos+1 < tm.PositionPoints.Length)
			{
				Position2 = tm.PositionPoints[currentPos+1].position;
			}
			else if(currentPos-1 >= 0)
			{
		 		Position2 = tm.PositionPoints[currentPos-1].position;
			}
			
			var ang : float = Vector3.Angle(Position2-Position1,transform.position-Position1);
			if(ang > 85 && ang < 95)
			{
				if(!tm.LoopedTrack)
				{
					if(currentTotal >= CalculateAmount(Lap+1))
					{
						IncreaseLap();
						Debug.Log("Lap from proper detection");
					}
				}
				else
				{
					if(currentTotal >= (Lap+1)*tm.PositionPoints.Length)
					{
						IncreaseLap();
						//Debug.Log("Lap from proper detection");
					}
				}
			}

		}

		if(!tm.LoopedTrack)
		{
			//Lap Catch, used if for some reason the above code dosen't work. i.e. Lag going across the line
			if(currentTotal > CalculateAmount(Lap+1) || Lap == -1)
			{
				IncreaseLap();
				//Debug.Log("Lap from overlap detection, Lap : " + Lap.ToString());
			}
		}
		else
		{
		//Lap Catch, used if for some reason the above code dosen't work. i.e. Lag going across the line
			if((currentTotal > (Lap+1)*tm.PositionPoints.Length) || (currentTotal >= (Lap+1)*tm.PositionPoints.Length && currentPos > 0))
			{
				IncreaseLap();
				//Debug.Log("Lap from overlap detection, Lap : " + Lap.ToString());
			}
		}

		Lap = Mathf.Clamp(Lap,-1,tm.Laps);

		Debug.DrawLine(transform.position, tm.PositionPoints[currentPos].position,Color.red);
	}
}

function IncreaseLap()
{
	Lap += 1;
	
	if(transform.GetComponent(kartInfo) != null && Lap < tm.Laps && Lap > 0)
		transform.GetComponent(kartInfo).NewLap();
		
}

function CalculateAmount(lVal : int)
{
	var val : int;

	for(var i : int = 0; i <= lVal;i++)
	{
	
		if(lVal < tm.pointsNeededToLap.Length)
		{
			val += tm.pointsNeededToLap[i];
		}
		else
			break;
	}

	return val;
}

function CheckForward(closestDistance : float){
	for(var i : int = 1; i < 3; i++){ 
		var newdistance = Vector3.Distance(transform.position,tm.PositionPoints[NumClamp(currentPos+i,0,tm.PositionPoints.Length)].position);
		if(newdistance < closestDistance){
			closestDistance = newdistance;
			currentPos += i;
			currentTotal += i;

			if(transform.GetComponent(kartInfo) != null)//Make the turn around icon stop flasing
				transform.GetComponent(kartInfo).turnAroundFlashing = false;
		}
	}
}

function CheckBackwards(closestDistance : float){
	for(var j : int = -1; j > -3; j--){
		var newdistance = Vector3.Distance(transform.position,tm.PositionPoints[NumClamp(currentPos+j,0,tm.PositionPoints.Length)].position);
		if(newdistance < closestDistance){
			closestDistance = newdistance;
			currentPos += j;
			currentTotal += j;

			var badDirection : Vector3 = tm.PositionPoints[NumClamp(currentPos,0,tm.PositionPoints.Length)].position - transform.position;

			if(Vector3.Angle(transform.forward,badDirection) < 45 && transform.GetComponent(kartInfo) != null)// Make the turn around icon flash
				transform.GetComponent(kartInfo).turnAroundFlashing = true;
		}
	}
}

function NumClamp(val : int,min : int,max : int){

	if(tm.LoopedTrack)
	{
		while(val > max-1)
			val -= (max-min);

		while(val < min)
			val += (max-min);
		}
	else
	{
		val = Mathf.Clamp(val,min,max-1);
	}

	return val;

}
