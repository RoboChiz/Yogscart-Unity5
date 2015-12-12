#pragma strict

var locked : boolean = true;

var throttle : float;
var steer : float;
var drift : boolean;

private var isFalling : boolean;
private var isColliding : boolean;

private var boostPercent : float = 0.4f;
private var grassPercent : float = 0.45f;

public var maxSpeed : float = 20f;
private var maxGrassSpeed : float = 7.5f;
private var lastMaxSpeed : float;
private var offRoad : boolean;

var acceleration : float = 10;

var BrakeTime : float = 1.5f;

var turnSpeed : float = 2f;
var driftAmount : float = 2f;
private var driftSteer : int;

private var driftStarted : boolean;
private var applyingDrift : boolean;
private var DriftVal : float;

private var tricking : boolean;
private var trickPotential : boolean;
private var trickLock : boolean;

var lapisAmount : int = 0;

var wheelColliders : WheelCollider[];
var wheelMeshes : Transform[];
private var wheelStart : Vector3[];

var BoostAddition : int = 7.5f;
private var isBoosting : String = "";
//Start Boost Variables
private var allowedBoost : boolean;
private var spinOut : boolean;
private var boostAmount : float;

var flameParticles : ParticleSystem[];
var DriftParticles : Transform[];
var TrickParticles : ParticleSystem;

var engineSound : AudioClip;

var kartbodyRot : float = 20;
var driftTime : float;
var blueTime : float = 3;
var orangeTime : float = 6;

var spinTime : float = 0.5f;
var spunTime : float = 1.5f;
private var Spinning : boolean;
private var spunOut : boolean;

private var expectedSpeed : float;

var actualSpeed : float;

@HideInInspector
public var startBoostVal : int = -1;

var snapTime : float = 0.1f;
var pushSpeed : float = 2f;
var pushTime : float = 0.5f;

var touchingKart : Vector3;

private var sfxVolume : float;

var relativeVelocity : Vector3;
var hitSounds : AudioClip[];
var tauntSounds : AudioClip[];
var sounding : boolean;

function Start()
{

	wheelStart = new Vector3[wheelMeshes.Length];
	
	for(var i : int = 0; i < wheelMeshes.Length; i++)
	{
		wheelStart[i] = wheelMeshes[i].localPosition;
	}
	
	if(GameObject.Find("Sound System") != null)
		sfxVolume = GameObject.Find("Sound System").transform.FindChild("SFX").GetComponent.<AudioSource>().volume;
	
	transform.GetChild(0).GetComponent.<AudioSource>().volume = sfxVolume;
	
	BoostAddition = maxSpeed * boostPercent;
	maxGrassSpeed = maxSpeed * grassPercent;
	driftAmount = turnSpeed / 2f;
	
	StartCoroutine("CustomUpdate");
}

var lastTime : float = 0.034;

function CustomUpdate()
{
	if(Time.timeScale != 0)
	{
		var startTime : float = Time.time;			
									
		var nA  = (ExpectedSpeed-actualSpeed)/lastTime;
		if(!isFalling && !isColliding && Mathf.Abs(nA) > 0.1f)
		{	
			GetComponent.<Rigidbody>().AddForce(transform.forward * nA,ForceMode.Acceleration);
		}
		
		CalculateExpectedSpeed(lastTime);
		
		if(!isFalling)
		{
			ApplySteering(lastTime);
		}
		else
		{
			wheelColliders[0].steerAngle = 0;
			wheelColliders[1].steerAngle = 0;
		}
		
		ApplyDrift(lastTime);
		
		var nMaxSpeed : float = Mathf.Lerp(lastMaxSpeed,maxSpeed-(1f-lapisAmount/10f),lastTime);
		
		ExpectedSpeed = Mathf.Clamp(ExpectedSpeed,-nMaxSpeed,nMaxSpeed);
		
		if(isBoosting != "")
		{
			nMaxSpeed = maxSpeed + BoostAddition;
			ExpectedSpeed = maxSpeed + BoostAddition;
		}
		
		actualSpeed = relativeVelocity.z;
		
		lastMaxSpeed = nMaxSpeed;
			
		var processingTime : float = 0.034 - ((Time.time - startTime)/1000f);	
		
		if(processingTime <= 0.034f)
		{
			yield WaitForSeconds(Mathf.Clamp(processingTime,0,0.0034));//Wait till end of 1/30 seconds
			lastTime = 0.034f;
		}
		else
		{
			lastTime = Mathf.Abs(processingTime);
		}
		
	}
	else
	{
		yield WaitForSeconds(0.0034);//Wait till end of 1/30 seconds
	}
	StartCoroutine("CustomUpdate");
}

function Update()//A special Update function that will run at 45fps
{
	lapisAmount = Mathf.Clamp(lapisAmount,0,10);
	relativeVelocity = transform.InverseTransformDirection(GetComponent.<Rigidbody>().velocity);
	
	if(Time.timeScale != 0)
	{
		isFalling = CheckGravity();
		
		var hit : RaycastHit;
		if(Physics.Raycast(transform.position,-transform.up,hit,1) && hit.collider.tag == "OffRoad")
			offRoad = true;
		else
			offRoad = false;
			
		DoTrick();
		
		for(var i : int; i < wheelMeshes.Length; i++)
		{
		
			var wheelPos : Vector3;
			var wheelRot : Quaternion;
			
			wheelColliders[i].GetWorldPose(wheelPos,wheelRot);
			
			if(i == 0 || i == 2)
				wheelMeshes[i].rotation = wheelRot;
			else
				wheelMeshes[i].rotation = wheelRot * Quaternion.Euler(0,180,0);
				
				if(!tricking && !isFalling && !spunOut)
					wheelMeshes[i].position.y = wheelPos.y;
				else
					wheelMeshes[i].localPosition = wheelStart[i];
		}
		
		//Play engine Audio
		if(engineSound != null){
			
			if(!GetComponent.<AudioSource>().isPlaying ){
				GetComponent.<AudioSource>().clip = engineSound;
				GetComponent.<AudioSource>().Play();
				GetComponent.<AudioSource>().loop = true;
			}
			
			GetComponent.<AudioSource>().volume = Mathf.Lerp(0.05,0.4,ExpectedSpeed/maxSpeed) * sfxVolume;
			
			GetComponent.<AudioSource>().pitch = Mathf.Lerp(0.75,1.5,ExpectedSpeed/maxSpeed);

		}
		
		//Calculate Start Boost
		if(startBoostVal == 3 && throttle > 0){
			spinOut = true;
		}
		if(startBoostVal == 2 && throttle > 0 && spinOut == false){
			allowedBoost = true;
		}

		if(startBoostVal < 2 && startBoostVal != 0 && throttle == 0){
			allowedBoost = false;
		}

		if(allowedBoost && throttle > 0)
			boostAmount += Time.fixedDeltaTime * 0.1f;

		if(startBoostVal == 0 && allowedBoost){
			Boost(boostAmount,"Trick");
			startBoostVal = -1;
		}

		if(startBoostVal == 0 && spinOut){
			SpinOut();
			startBoostVal = -1;
		}	
	}
}

function KartCollision(otherKart : Transform)
{
	//Put kart collisions effects here
	var compareVect = otherKart.transform.position - transform.position;
	
	if(touchingKart == Vector3.zero)
	{
		if(Vector3.Angle(compareVect,transform.right) > 90) //Decides where way will push us away from the kart
			touchingKart = transform.right;
		else
			touchingKart = -transform.right;

		//Add the horizontal velocity
		//var stopA = (pushSpeed-relativeVelocity.x) / 0.0333f;
		//GetComponent.<Rigidbody>().AddForce(stopA * touchingKart, ForceMode.Acceleration);	
		//GetComponent.<Rigidbody>().constraints = RigidbodyConstraints.None;
	}
	
	transform.position += (touchingKart * (2f - compareVect.magnitude));
	
	var startTime = Time.time;
	while(Time.time - startTime <= pushTime)
	{
		relativeVelocity.x = pushSpeed * touchingKart.x;	
		GetComponent.<Rigidbody>().velocity = transform.TransformDirection(relativeVelocity);
		yield;
	}
}

function CancelCollision()
{

	touchingKart = Vector3.zero;
	
	//Remove the horizontal velocity
	relativeVelocity.x = 0;	
	GetComponent.<Rigidbody>().velocity = transform.TransformDirection(relativeVelocity);
	
	GetComponent.<Rigidbody>().constraints = RigidbodyConstraints.None;

}

function DoTrick()
{
	if(isFalling)
	{
		SnapUp();
		
		if(trickPotential)
		{
			tricking = true;
			trickPotential = false;
			SpinKartBody(Vector3.right,spinTime);
			TrickParticles.Play();
		}
	}
	else
	{
		if(!trickPotential && drift && !trickLock)
		{
			trickPotential = true;
			trickLock = true;
			CancelTrickPotential();
		}
		
		if(tricking)
		{
			Boost(0.25f,"Trick");
			tricking = false;
		}
	}
	
	if(drift == false)
		trickLock = false;
}

function CalculateExpectedSpeed(lastTime : float)
{
	if(throttle == 0 || locked)
	{
		var cacceleration : float = -ExpectedSpeed/BrakeTime;
		ExpectedSpeed += (cacceleration * lastTime);

		if(Mathf.Abs(ExpectedSpeed) <= 0.02)
		ExpectedSpeed = 0;
	}
	else
	{
		if(HaveTheSameSign(throttle,ExpectedSpeed) == false)
			ExpectedSpeed += (throttle * acceleration * 2) *  lastTime;
		else{
		
			var percentage : float;
			
			if(offRoad && isBoosting != "Boost")
				percentage  = (1f/maxGrassSpeed) * Mathf.Abs(ExpectedSpeed);
			else
				percentage  = (1f/maxSpeed) * Mathf.Abs(ExpectedSpeed);
				
			ExpectedSpeed += (throttle * acceleration * (1f-percentage)) *  lastTime;
		}
	}
						
}

function ApplySteering(lastTime : float)
{

	if(!driftStarted){
		
		if(steer != 0)
		{
			steer = Mathf.Sign(steer);
			
			steer *= Mathf.Lerp(3,0.8,Mathf.Abs(actualSpeed)/(maxSpeed+BoostAddition));
			
		}
		
		wheelColliders[0].steerAngle = steer * turnSpeed;
		wheelColliders[1].steerAngle = steer * turnSpeed;
		
	}else{	
	
		wheelColliders[0].steerAngle = Mathf.Lerp(wheelColliders[0].steerAngle,(driftSteer * turnSpeed) + (steer * driftAmount),lastTime*500f);
		wheelColliders[1].steerAngle = Mathf.Lerp(wheelColliders[1].steerAngle,(driftSteer * turnSpeed) + (steer * driftAmount),lastTime*500f);	
		
	}
	
	if(isColliding)
	{
		wheelColliders[0].steerAngle = 0;
		wheelColliders[0].steerAngle = 0;
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
	
	if(grounded || Physics.Raycast(transform.position,-transform.up,1))
		return false;
	else
		return true;

}

function ApplyDrift(lastTime : float){

	var KartBody : Transform = transform.FindChild("Kart Body");

	if(drift && ExpectedSpeed > maxSpeed*0.75f && !isFalling && (!offRoad || (offRoad && isBoosting == "Boost"))){
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
	driftTime += lastTime * Mathf.Abs(driftSteer+(steer/2f));
	if(!Spinning)
		KartBody.localRotation = Quaternion.Slerp(KartBody.localRotation,Quaternion.Euler(0,kartbodyRot * driftSteer,0),lastTime*2);

	for(var f : int = 0; f < 2; f++){

	if(driftTime >= orangeTime){

	DriftParticles[f].GetComponent.<Renderer>().material = Resources.Load("Particles/Drift Particles/Spark_Orange", Material);

	}else if(driftTime >= blueTime){

	DriftParticles[f].GetComponent.<ParticleSystem>().Play();
	DriftParticles[f].GetComponent.<Renderer>().material = Resources.Load("Particles/Drift Particles/Spark_Blue", Material);

	}
	}

	}else{
	
	if(isFalling || offRoad)
		driftTime = 0;
	
	if(!Spinning)
		KartBody.localRotation = Quaternion.Slerp(KartBody.localRotation,Quaternion.Euler(0,0,0),lastTime*2);

	if(throttle > 0){
	if(driftTime >= orangeTime)
	{
	Boost(1,"Drift");	
	}
	else if(driftTime >= blueTime)
	{
	Boost(0.5,"Drift");
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

function Boost(t : float, type : String)
{
	isBoosting = type;
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

isBoosting = "";
}

function CancelBoost()
{
StopCoroutine("Boost");

for(var i : int = 0; i < flameParticles.Length; i++)
flameParticles[i].Stop();

isBoosting = "";
}

private var snapping : boolean;

function CancelTrickPotential()
{
	yield WaitForSeconds(0.5f);
	trickPotential = false;
}

function SpinKartBody(dir : Vector3, time : float)
{
	
	Spinning = true;

	var startTime : float = Time.realtimeSinceStartup;
	
	while(Time.realtimeSinceStartup - startTime < time)
	{
		transform.FindChild("Kart Body").Rotate((dir * 360f * Time.deltaTime)/time);
		yield;
	}
	
	Spinning = false;
	
}

function SpinOut(){
	StartCoroutine("StartSpinOut");
}


function StartSpinOut(){
	if(!spunOut)
	{

	spunOut = true;

	CancelBoost();

	locked = true;
	
	if(!sounding && SilenceCheck() && hitSounds != null && hitSounds.Length > 0)
	{
		sounding = true;
		var randi = Random.Range(0,hitSounds.Length);
		transform.GetChild(0).GetComponent.<AudioSource>().PlayOneShot(hitSounds[randi]);
	}
	
	var t : float = 0;

	var Ani = transform.FindChild("Kart Body").FindChild("Character").GetComponent(Animator);
	Ani.SetBool("Hit",true);

	yield SpinKartBody(Vector3.up,spunTime);

	Ani.SetBool("Hit",false);
	locked = false;
	
	spunOut = false;
	sounding = false;

	}
}

function HitEnemy()
{
	if(!sounding && SilenceCheck())
	{
		if(tauntSounds != null && tauntSounds.Length > 0)
		{
			sounding = true;
			var randi = Random.Range(0,tauntSounds.Length);
			transform.GetChild(0).GetComponent.<AudioSource>().PlayOneShot(tauntSounds[randi]);
			yield WaitForSeconds(tauntSounds[randi].length);
			sounding = false;
		}
	}
}

function GetSounding()
{
	return sounding;
}

function SilenceCheck()
{
	var objs = GameObject.FindObjectsOfType(kartScript);
	
	for(var i = 0; i < objs.Length; i++)
	{
		if(objs[i].GetSounding() && Vector3.Distance(transform.position,objs[i].transform.position) < 15)
		{
			return false;
		}	
	}
	
	return true;
}

function SnapUp()
{

	if(!snapping)
	{
	
	snapping = true;
	
	var startRot : Quaternion = transform.rotation;
	var startTime : float = Time.realtimeSinceStartup;
	
	GetComponent.<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
	
	while(Time.realtimeSinceStartup - startTime < snapTime)
	{
	transform.rotation = Quaternion.Lerp(startRot,Quaternion.Euler(0,transform.rotation.eulerAngles.y,0),(Time.realtimeSinceStartup - startTime)/snapTime);
	yield;
	}
	
	GetComponent.<Rigidbody>().constraints = RigidbodyConstraints.None;
	
	snapping = false;
	
	}
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
	if(collision.rigidbody == null)
		Collided(collision);
}

function Collided(collision : Collision)
{
	
	var hit : RaycastHit;
	if(!Physics.Raycast(transform.position,transform.right * 4f) && !Physics.Raycast(transform.position,-transform.right * 4f))
	{
		if(Physics.Raycast(transform.position,transform.forward * Mathf.Sign(ExpectedSpeed),hit))
		{
			if(hit.collider == collision.collider)
			{
				ExpectedSpeed /= 2f;
				ExpectedSpeed = -ExpectedSpeed;
			}
		}
	}
	
	isColliding = true;
	yield WaitForSeconds(0.2f);
	isColliding = false;
}

public function get ExpectedSpeed() : float{return expectedSpeed;}
public function set ExpectedSpeed(value : float)
{
	if(!isColliding)
    	expectedSpeed = value;
}