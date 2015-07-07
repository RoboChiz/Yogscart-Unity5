#pragma strict

var sending : boolean;

private var updateTime : float;
private var posTime : float;

var networkSendRate : float = 20;
var frameCount : int = 0;

function FixedUpdate () {
	if(sending)
	{
	
		if(frameCount % 20 == 0)
		{
			var ks : kartScript = transform.GetComponent(kartScript);
			transform.GetComponent.<NetworkView>().RPC("MyInput",RPCMode.Others,ks.throttle,ks.steer,ks.drift);
			updateTime = 0;
		}
	
		if(frameCount % 180 == 0)
		{
			transform.GetComponent.<NetworkView>().RPC("MyPosition",RPCMode.Others,transform.position,transform.rotation,GetComponent.<Rigidbody>().velocity);
			posTime = 0;
		}
		
		frameCount++;
		
	}
}

@RPC
function MyInput(t : float, s : float, d : boolean)
{

	var ks : kartScript = transform.GetComponent(kartScript);
	
	ks.throttle = t;
	ks.steer = s;
	ks.drift = d;
	
}

@RPC
function MyPosition(pos : Vector3,rot : Quaternion, vel : Vector3)
{

	transform.position = pos;
	transform.rotation = rot;
	
	GetComponent.<Rigidbody>().velocity = vel;
	
}