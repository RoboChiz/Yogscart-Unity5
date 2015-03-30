#pragma strict

var inputString : String;

var locked : boolean;

var throttle : float;
var steer : float;
var drift : boolean;

private var isFalling : boolean;
private var isColliding : boolean;

var maxSpeed : float = 15f;
private var lastMaxSpeed : float;

var acceleration : float = 10f;

var BrakeTime : float = 0.5f;

var turnSpeed : float = 2f;
var driftAmount : float = 5;
private var driftSteer : int;

private var driftStarted : boolean;
private var applyingDrift : boolean;
private var DriftVal : float;

var lapisAmount : int = 0;

var wheelColliders : WheelCollider[];
var wheelMeshes : Transform[];

var BoostAddition : int = 5;
private var isBoosting : boolean;

var flameParticles : ParticleSystem[];
var DriftParticles : Transform[];


var kartbodyRot : float = 20;
private var driftTime : float;
var blueTime : float = 3;
var orangeTime : float = 6;

private var expectedSpeed : float;
private var actualSpeed : float;

private var localScale : float = 2f;

function FixedUpdate () {

	throttle = Input.GetAxis(inputString + "Throttle");
	steer = Input.GetAxis(inputString + "Horizontal");
	drift = Input.GetAxis(inputString + "Drift") != 0;
	
	isFalling = CheckGravity();
	
	CalculateExpectedSpeed();
	ApplySteering();
	ApplyDrift();
	
	var nMaxSpeed : float = Mathf.Lerp(lastMaxSpeed,maxSpeed-(1f-lapisAmount/10f),Time.fixedDeltaTime);
	
	expectedSpeed = Mathf.Clamp(expectedSpeed,-nMaxSpeed,nMaxSpeed);
	
	if(isBoosting)
	{
		nMaxSpeed = maxSpeed + BoostAddition;
		expectedSpeed = maxSpeed + BoostAddition;
	}
	
	var relativeVelocity : Vector3 = transform.InverseTransformDirection(GetComponent.<Rigidbody>().velocity);
	actualSpeed = relativeVelocity.z;
	
	var nExpectedSpeed = expectedSpeed * localScale;
	var nA  = (nExpectedSpeed-actualSpeed)/Time.fixedDeltaTime;

	if(!isFalling && !isColliding)
	{	
		GetComponent.<Rigidbody>().AddRelativeForce(Vector3.forward * GetComponent.<Rigidbody>().mass * nA);
	}
	
	lastMaxSpeed = nMaxSpeed;
	
	for(var i : int; i < wheelMeshes.Length; i++)
	{
	
		var wheelPos : Vector3;
		var wheelRot : Quaternion;
		
		wheelColliders[i].GetWorldPose(wheelPos,wheelRot);
		
		wheelMeshes[i].position = wheelPos;
		wheelMeshes[i].rotation = wheelRot;
		
	}
	
}

function CalculateExpectedSpeed()
{
	if(throttle == 0 || locked)
	{
		var cacceleration : float = -expectedSpeed/BrakeTime;
		expectedSpeed += (cacceleration * Time.fixedDeltaTime);

		if(Mathf.Abs(expectedSpeed) <= 0.02)
		expectedSpeed = 0;
	}
	else
	{
		if(HaveTheSameSign(throttle,expectedSpeed) == false)
			expectedSpeed += (throttle * acceleration * 2) *  Time.fixedDeltaTime;
		else{
			var percentage : float = (1f/maxSpeed) * Mathf.Abs(expectedSpeed);
			expectedSpeed += (throttle * acceleration * (1f-percentage)) *  Time.fixedDeltaTime;
		}
	}
						
}

function ApplySteering()
{
	if(!driftStarted){
		
		var speedVal = Mathf.Clamp((maxSpeed - Mathf.Abs(expectedSpeed))/2f,1,Mathf.Infinity);
		
		wheelColliders[0].steerAngle = Mathf.Lerp(wheelColliders[0].steerAngle,steer * turnSpeed * speedVal,Time.fixedDeltaTime*25);
		wheelColliders[1].steerAngle = Mathf.Lerp(wheelColliders[1].steerAngle,steer * turnSpeed * speedVal,Time.fixedDeltaTime*25);
	}else{	
	
		var nSteer = driftSteer * Mathf.Clamp(steer/2f,-driftSteer * 0.4, driftSteer*0.5);
	
		wheelColliders[0].steerAngle = Mathf.Lerp(wheelColliders[0].steerAngle,(driftSteer + nSteer) * driftAmount,Time.fixedDeltaTime*25);
		wheelColliders[1].steerAngle = Mathf.Lerp(wheelColliders[1].steerAngle,(driftSteer + nSteer) * driftAmount,Time.fixedDeltaTime*25);	
		
	}
}

function CheckGravity() : boolean 
{

	var grounded : boolean = false;

	for(var i : int = 0; i < 4; i++)
	{
		if(wheelColliders[i].isGrounded)
		{
			grounded = true;
			break;
		}
	}
	
	if(grounded || Physics.Raycast(transform.position,Physics.gravity.normalized,3))
		return false;
	else
		return true;

}

function ApplyDrift(){

	var KartBody : Transform = transform.FindChild("Kart Body");

	if(drift && expectedSpeed > maxSpeed*0.75f && !isFalling){
	if(!applyingDrift && Mathf.Abs(steer) > 0.2 && driftStarted == false ){
	driftStarted = true;
	driftSteer = Mathf.Sign(steer);
	}
	}else{
	if(driftStarted == true){
	DriftVal = driftSteer;
	applyingDrift = true;
	driftStarted = false;
	ResetDrift();
	}
	}

	if(driftStarted == true){
	driftTime += Time.fixedDeltaTime + (Time.fixedDeltaTime * Mathf.Abs(driftSteer+steer));
	KartBody.localRotation = Quaternion.Slerp(KartBody.localRotation,Quaternion.Euler(0,kartbodyRot * driftSteer,0),Time.fixedDeltaTime*2);

	for(var f : int = 0; f < 2; f++){

	if(driftTime >= orangeTime){

	DriftParticles[f].GetComponent.<Renderer>().material = Resources.Load("Particles/Drift Particles/Spark_Orange", Material);

	}else if(driftTime >= blueTime){

	DriftParticles[f].GetComponent.<ParticleSystem>().Play();
	DriftParticles[f].GetComponent.<Renderer>().material = Resources.Load("Particles/Drift Particles/Spark_Blue", Material);

	}
	}

	}else{
	
	KartBody.localRotation = Quaternion.Slerp(KartBody.localRotation,Quaternion.Euler(0,0,0),Time.fixedDeltaTime*2);

	if(throttle > 0){
	if(driftTime >= orangeTime)
	{
	Boost(1);	
	}
	else if(driftTime >= blueTime)
	{
	Boost(0.5);
	}
	}

	driftTime = 0f;
	DriftParticles[0].GetComponent.<ParticleSystem>().Stop();
	DriftParticles[1].GetComponent.<ParticleSystem>().Stop();
	}

}

function ResetDrift(){
	yield WaitForSeconds(0.1f);
	applyingDrift = false;
}

function Boost(t : float)
{
	isBoosting = true;
	StopCoroutine("StartBoost");
	StartCoroutine("StartBoost",t);
}

function StartBoost(t : float)
{
var BoostSound = Resources.Load("Music & Sounds/SFX/boost",AudioClip);
GetComponent.<AudioSource>().PlayOneShot(BoostSound,3);

for(var i : int = 0; i < flameParticles.Length; i++)
flameParticles[i].Play();

yield WaitForSeconds(t);

for(i = 0; i < flameParticles.Length; i++)
flameParticles[i].Stop();

isBoosting = false;
}

function CancelBoost()
{
StopCoroutine("Boost");

for(var i : int = 0; i < flameParticles.Length; i++)
flameParticles[i].Stop();

isBoosting = false;
}

function HaveTheSameSign(first : float, second : float) : boolean
{
	if (Mathf.Sign(first) == Mathf.Sign(second))
		return true;
	else
		return false;
}

function OnCollisionEnter(collision : Collision)
{
	Collided(collision);
}

function Collided(collision : Collision)
{
	isColliding = true;
	yield WaitForSeconds(0.4f);
	isColliding = false;
}