﻿#pragma strict

var locked : boolean = true;

var throttle : float;
var steer : float;
var drift : boolean;

private var isFalling : boolean;
private var isColliding : boolean;

public var maxSpeed : float = 20f;
var maxGrassSpeed : float = 7.5f;
private var lastMaxSpeed : float;
private var offRoad : boolean;

var acceleration : float = 10;

var BrakeTime : float = 0.5f;

var turnSpeed : float = 3f;
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
private var driftTime : float;
var blueTime : float = 3;
var orangeTime : float = 6;

var spinTime : float = 0.5f;
var spunTime : float = 1.5f;
private var Spinning : boolean;
private var spunOut : boolean;

@HideInInspector
public var expectedSpeed : float;

var actualSpeed : float;

@HideInInspector
public var startBoostVal : int = -1;

var snapTime : float = 0.1f;
var pushSpeed : float = 20f;
private var touchingKart : Vector3;

private var sfxVolume : float;

var relativeVelocity : Vector3;

function Start()
{

	wheelStart = new Vector3[wheelMeshes.Length];
	
	for(var i : int = 0; i < wheelMeshes.Length; i++)
	{
		wheelStart[i] = wheelMeshes[i].localPosition;
	}
	
	sfxVolume = GameObject.Find("Sound System").transform.FindChild("SFX").GetComponent.<AudioSource>().volume;
}

function Update()
{
	lapisAmount = Mathf.Clamp(lapisAmount,0,10);
	relativeVelocity = transform.InverseTransformDirection(GetComponent.<Rigidbody>().velocity);
		
}

function FixedUpdate()
{
	isFalling = CheckGravity();
	
	var hit : RaycastHit;
	if(Physics.Raycast(transform.position,-transform.up,hit,1) && hit.collider.tag == "OffRoad")
		offRoad = true;
	else
		offRoad = false;
		
	DoTrick();
	
	CalculateExpectedSpeed();
	
	if(!isFalling)
	{
		ApplySteering();
	}
	else
	{
		wheelColliders[0].steerAngle = 0;
		wheelColliders[1].steerAngle = 0;
	}
	
	ApplyDrift();
	
	var nMaxSpeed : float = Mathf.Lerp(lastMaxSpeed,maxSpeed-(1f-lapisAmount/10f),Time.fixedDeltaTime);
	
	expectedSpeed = Mathf.Clamp(expectedSpeed,-nMaxSpeed,nMaxSpeed);
	
	if(isBoosting != "")
	{
		nMaxSpeed = maxSpeed + BoostAddition;
		expectedSpeed = maxSpeed + BoostAddition;
	}
	
	actualSpeed = relativeVelocity.z;
	
	var nA  = (expectedSpeed-actualSpeed)/Time.fixedDeltaTime;

	if(!isFalling && !isColliding)
	{	
		GetComponent.<Rigidbody>().AddForce(transform.forward * GetComponent.<Rigidbody>().mass * nA);
	}
	
	lastMaxSpeed = nMaxSpeed;
	
	for(var i : int; i < wheelMeshes.Length; i++)
	{
	
		var wheelPos : Vector3;
		var wheelRot : Quaternion;
		
		wheelColliders[i].GetWorldPose(wheelPos,wheelRot);
		
		if(i == 3 || i == 0)
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
		
		GetComponent.<AudioSource>().volume = Mathf.Lerp(0.01,0.075,expectedSpeed/maxSpeed) * sfxVolume;
		
		GetComponent.<AudioSource>().pitch = Mathf.Lerp(0.75,1.5,expectedSpeed/maxSpeed);

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
		var stopA = (pushSpeed-relativeVelocity.x) / 0.0333f;
		GetComponent.<Rigidbody>().AddForce(stopA * touchingKart, ForceMode.Acceleration);	
		GetComponent.<Rigidbody>().constraints = RigidbodyConstraints.None;
		
		yield;
		
		relativeVelocity.x = pushSpeed * touchingKart.x;	
		GetComponent.<Rigidbody>().velocity = transform.TransformDirection(relativeVelocity);
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
		
			var percentage : float;
			
			if(offRoad && isBoosting != "Boost")
				percentage  = (1f/maxGrassSpeed) * Mathf.Abs(expectedSpeed);
			else
				percentage  = (1f/maxSpeed) * Mathf.Abs(expectedSpeed);
				
			expectedSpeed += (throttle * acceleration * (1f-percentage)) *  Time.fixedDeltaTime;
		}
	}
						
}

function ApplySteering()
{

	if(!driftStarted){
		
		if(steer != 0)
		{
			steer = Mathf.Sign(steer);
			
			steer *= Mathf.Lerp(3,1,actualSpeed/maxSpeed);
			
		}
		
		wheelColliders[0].steerAngle = steer * turnSpeed;
		wheelColliders[1].steerAngle = steer * turnSpeed;
		
	}else{	
	
		wheelColliders[0].steerAngle = Mathf.Lerp(wheelColliders[0].steerAngle,(driftSteer * turnSpeed) + (steer * driftAmount),Time.fixedDeltaTime*500f);
		wheelColliders[1].steerAngle = Mathf.Lerp(wheelColliders[1].steerAngle,(driftSteer * turnSpeed) + (steer * driftAmount),Time.fixedDeltaTime*500f);	
		
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

function ApplyDrift(){

	var KartBody : Transform = transform.FindChild("Kart Body");

	if(drift && expectedSpeed > maxSpeed*0.75f && !isFalling && (!offRoad || (offRoad && isBoosting == "Boost"))){
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
	if(!Spinning)
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
	
	if(isFalling || offRoad)
		driftTime = 0;
	
	if(!Spinning)
		KartBody.localRotation = Quaternion.Slerp(KartBody.localRotation,Quaternion.Euler(0,0,0),Time.fixedDeltaTime*2);

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
		transform.FindChild("Kart Body").Rotate((dir * 360f * Time.fixedDeltaTime)/time);
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

	var t : float = 0;

	var Ani = transform.FindChild("Kart Body").FindChild("Character").GetComponent(Animator);
	Ani.SetBool("Hit",true);

	yield SpinKartBody(Vector3.up,spunTime);

	Ani.SetBool("Hit",false);
	locked = false;

	spunOut = false;

	}
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
	isColliding = true;
	//expectedSpeed /= 4f;
	//expectedSpeed = -expectedSpeed;
	yield WaitForSeconds(0.2f);
	isColliding = false;
}