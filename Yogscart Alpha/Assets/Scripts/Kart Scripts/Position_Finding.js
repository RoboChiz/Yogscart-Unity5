#pragma strict

var position : int = -1;

var Lap : int = -1;
var currentPos : int;
var currentTotal : int;
var currentDistance : float;

var locked : boolean;

private var tm : TrackData;

function Start(){
tm = GameObject.Find("Track Manager").transform.GetComponent(TrackData);
}

function Update () {

var closestDistance : float = Mathf.Infinity;
closestDistance = Vector3.Distance(transform.position,tm.PositionPoints[NumClamp(currentPos,0,tm.PositionPoints.Length)].position);

CheckForward(closestDistance);
CheckBackwards(closestDistance);

if(Lap == -1)
{
currentPos = 0;
currentTotal = 0;
}
else
currentPos = NumClamp(currentPos,0,tm.PositionPoints.Length);

currentTotal = Mathf.Clamp(currentTotal,Lap * tm.PositionPoints.Length,Mathf.Infinity);
currentDistance = Vector3.Distance(transform.position,tm.PositionPoints[NumClamp(currentPos + 1,0,tm.PositionPoints.Length)].position);

var hit : RaycastHit;
if(currentPos == 0 && currentTotal >= (Lap+1)*tm.PositionPoints.Length-1){

if(currentPos == 0 && Vector3.Distance(transform.position,tm.PositionPoints[1].position) <= Vector3.Distance(transform.position,tm.PositionPoints[tm.PositionPoints.Length-1].position))
Lap += 1;

}

//Lap Catch, used if for some reason the above code dosen't work. i.e. Lag going across the line
if(currentPos >0 && currentTotal >= (Lap+1)*tm.PositionPoints.Length)
Lap += 1;

}

function CheckForward(closestDistance : float){
for(var i : int = 1; i < 3; i++){
var newdistance = Vector3.Distance(transform.position,tm.PositionPoints[NumClamp(currentPos+i,0,tm.PositionPoints.Length)].position);
if(newdistance < closestDistance){
closestDistance = newdistance;
currentPos += i;
if(currentPos == (currentTotal + i)-((tm.PositionPoints.Length)*Lap))
currentTotal += i;
}
}
}

function CheckBackwards(closestDistance : float){
for(var j : int = -1; j > -3; j--){
var newdistance = Vector3.Distance(transform.position,tm.PositionPoints[NumClamp(currentPos+j,0,tm.PositionPoints.Length)].position);
if(newdistance < closestDistance){
closestDistance = newdistance;
currentPos += j;
if(currentTotal > Lap * tm.PositionPoints.Length-1)
currentTotal += j;
}
}
}

function NumClamp(val : int,min : int,max : int){

while(val > max-1)
val -= (max-min);

while(val < min)
val += (max-min);


return val;

}
