#pragma strict

private var td : TrackData;
private var pf : Position_Finding;
private var ks : kartScript;
//private var ki : kartItem;
private var gd : CurrentGameData;

//0 - 50cc, 1 - 100cc, 2 - 150cc, 3 - Insane
var Stupidity : int; //Bigger the number, stupider the AI.

var angleRequired : float = 0f;
private var steering : int;

private var nTarget : Vector3;
private var targestPos : int = -1;

private var usedItem : boolean = false;
private var lastAngle : float;

private var adjusterFloat : float;

function Awake(){
td = GameObject.Find("Track Manager").GetComponent(TrackData);
pf = transform.GetComponent(Position_Finding);
ks = transform.GetComponent(kartScript);
//ki = transform.GetComponent(kartItem);
//gd = GameObject.Find("GameData").GetComponent(CurrentGameData);

adjusterFloat = Random.Range(-8f,8f);

}

function Update () {
//Calculate Item

/*if(ki.heldPowerUp != -1 && usedItem == false){

if(Stupidity < 5)
{

if(gd.PowerUps[ki.heldPowerUp].type == ItemType.UsableAsShield)
useShield();
else if(gd.PowerUps[ki.heldPowerUp].type == ItemType.Projectile)
FireProjectile();
else 
useItemRandom();

}else
useItemRandom();

usedItem = true;
}*/

//Calculate Steering
if(NumClamp(pf.currentPos+1,0,td.PositionPoints.Length) != targestPos ){

var nextChump : int;
if(pf.currentPos+1 < td.PositionPoints.Length)
nextChump = pf.currentPos+1;
else
nextChump = 0;

var Target : Vector3 = td.PositionPoints[nextChump].position;
var lastTarget : Vector3 = td.PositionPoints[pf.currentPos].position;

adjusterFloat += Random.Range(-3f,3f);
adjusterFloat = Mathf.Clamp(adjusterFloat,-6f,6f);

var Adjuster = Vector3.Cross((Target-lastTarget).normalized,transform.up) * adjusterFloat;

nTarget = Target + Adjuster;

targestPos = nextChump;

}

if(!ks.locked){
var NeededDirection : Vector3 = nTarget - transform.position;
NeededDirection.y = 0;

var fireDirection = transform.right;
fireDirection.y = 0;

var angle : float = Vector3.Angle(fireDirection,NeededDirection);

Debug.DrawRay(transform.position,transform.forward*5,Color.red);
Debug.DrawRay(transform.position,NeededDirection,Color.green);


if(angle > 90+angleRequired)
steering = -1;
else if(angle < 90-angleRequired)
steering = 1;
else{
steering = 0;
ks.drift = false;
}

if(steering != 0)
ks.throttle = 0.75;

//Calculate Throttle
if(angle >= 120 || angle  <= 60){

if(Stupidity < 6)
ks.drift = true;
ks.throttle = 0.5;

}else{
ks.throttle = 1;
}

if(ks.drift == true && lastAngle < angle)
ks.drift = false;

var testangle : float = Vector3.Angle(transform.forward,NeededDirection);
if(testangle >= 135){
steering = 1;
ks.throttle = 0.25;
}

var relativeVelocity : Vector3 = transform.InverseTransformDirection(GetComponent.<Rigidbody>().velocity);

if(ks.ExpectedSpeed > 1 && relativeVelocity.z < 1) //Presume something is blocking the kart.
reversing = true;

if(reversing){

Debug.DrawRay(transform.position,transform.forward*6,Color.green);
Debug.DrawRay(transform.position + transform.right,transform.forward*6,Color.green);
Debug.DrawRay(transform.position - transform.right,transform.forward*6,Color.green);

if(!Physics.Raycast(transform.position,transform.forward,6)&&!Physics.Raycast(transform.position + transform.right,transform.forward,6)&&!Physics.Raycast(transform.position - transform.right,transform.forward,6))
reversing = false;

steering = -steering;
ks.throttle = -1;
}

ks.steer = steering;

}

if(ks.startBoostVal != -1){
if(Stupidity < 4 && ks.startBoostVal <= 2)
ks.throttle = 1;

if(Stupidity > 8 && ks.startBoostVal <= 3)
ks.throttle = 1;
}

lastAngle = angle;

}

var reversing : boolean;

function useItemRandom(){
/*Debug.Log("Started Iteming!");
yield WaitForSeconds(Random.Range(5,20));
ki.input = true;
yield;
yield;
Debug.Log("Done Iteming!");
ki.input = false;
usedItem = false;*/
}

function useShield(){
/*Debug.Log("Started Iteming!");
ki.input = true;
while(ki.heldPowerUp != -1)
yield;
Debug.Log("Done Iteming!");
ki.input = false;
usedItem = false;*/
}

function FireProjectile()
{
/*
while(true)
{
var hit : RaycastHit;

if(Physics.Raycast(transform.position,transform.forward,hit,25) && hit.transform.tag == "Kart")
{
ki.input = true;
break;
}

yield;
}

ki.input = false;
usedItem = false;
*/
}

function NumClamp(val : int,min : int,max : int){

while(val > max-1)
val -= (max-min);

while(val < min)
val += (max-min);


return val;
}