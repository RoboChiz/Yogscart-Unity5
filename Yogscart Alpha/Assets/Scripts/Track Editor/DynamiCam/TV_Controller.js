#pragma strict

<<<<<<< HEAD
var dc : DynamiCamera[];
var currentCamera : int = -1;

var currentTarget : Transform;

function WakeUp (target : Transform) {

	dc = GameObject.FindObjectsOfType(DynamiCamera);
	currentTarget = target;
	
	for(var i : int = 0; i < dc.length; i++)
	{
		//Set up DynamiCameras
		dc[i].transform.GetComponent(Camera).enabled = false;
		dc[i].target = currentTarget;
		
	}
	
	StartCoroutine("StartTV");

}

function Awake()
{
	TurnOff();
}

function TurnOff()
{
	dc = GameObject.FindObjectsOfType(DynamiCamera);
	for(var i : int = 0; i < dc.length; i++)
	{
		//Set up DynamiCameras
		dc[i].transform.GetComponent(Camera).enabled = false;	
	}
	
	StopCoroutine("StartTV");
}

function StartTV () 
{
	while(true)
	{
		FindCamera();
		yield WaitForSeconds(1f);
	}
}

function FindCamera()
{

	var bestDistance : float = float.MaxValue;
	
	if(currentCamera != -1)
		dc[currentCamera].transform.GetComponent(Camera).enabled = false;
		
	currentCamera = -1;

	for(var i : int = 0; i < dc.length; i++)
	{
		if(dc[i].interested && Vector3.Distance(dc[i].transform.position,currentTarget.position) < bestDistance)
		{
			bestDistance = Vector3.Distance(dc[i].transform.position,currentTarget.position);
			currentCamera = i;
		}
	}
	
	if(currentCamera != -1)
	{
		dc[currentCamera].transform.GetComponent(Camera).enabled = true;
	}

}
=======
//Also known as the Jagrafess (Doctor Who Reference)

var cameras : DynamiCamera[];
private var currentCamera : int = 0;

var DesiredPlayer : Transform; //Change to an array of Players

var waitTime : float = 7;
private var totalTime : float;

var depth : int;

private var SpecCam : int;

function Start () {

cameras = GameObject.FindObjectsOfType(DynamiCamera);

for(var i : int = 0; i < cameras.Length; i++)
{
cameras[i].target = DesiredPlayer;

if(cameras[i].cameraType == Type.Spectator)
{
SpecCam = i;
}

}

ChangeCamera();

}

function FixedUpdate () {

var currentTime = Time.timeSinceLevelLoad;

if(currentTime >= totalTime + waitTime)
{
ChangeCamera();
totalTime = currentTime;
}

}

function ChangeCamera()
{
	if(DesiredPlayer != null)
	{
		if(cameras.Length > 0){

		cameras[currentCamera].transform.GetComponent.<Camera>().depth = -depth;
		cameras[currentCamera].transform.GetComponent.<Camera>().enabled = false;

		var changed : boolean = false;

		for(var i : int = 0; i < cameras.Length; i++)
		{

		var currentDist = Vector3.Distance(cameras[currentCamera].transform.position,DesiredPlayer.position);
		var possibleDist = Vector3.Distance(cameras[i].transform.position,DesiredPlayer.position);

		if(cameras[i].interested && (currentCamera == SpecCam || (currentDist >= possibleDist)))
		{
		currentCamera = i;
		changed = true;
		Debug.Log("Someone Interested");
		}

		if(cameras[i].target != DesiredPlayer)
		cameras[i].target = DesiredPlayer;

		}

		if(changed == false)
		currentCamera = SpecCam;

		cameras[currentCamera].transform.GetComponent.<Camera>().depth = depth;
		cameras[currentCamera].transform.GetComponent.<Camera>().enabled = true;
		}
	}
}
>>>>>>> parent of f5b6b47... Update #18 - The Robo fixed the kart collisions update!
