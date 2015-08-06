#pragma strict

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