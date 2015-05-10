#pragma strict

var sending : boolean;

var updateTime : float;
var posTime : float;

var networkSendRate : float = 15;
private var timeWait : float;

function Awake()
{
	timeWait = 1f/networkSendRate;
}

function FixedUpdate () {
	if(sending)
	{
		updateTime += Time.fixedDeltaTime;
		posTime += Time.fixedDeltaTime;
	
		if(updateTime > timeWait)
		{
			var ks : kartScript = transform.GetComponent(kartScript);
			transform.GetComponent.<NetworkView>().RPC("MyInput",RPCMode.Others,ks.throttle,ks.steer,ks.drift);
			updateTime = 0;
		}
	
		if(posTime > 3)
		{
			transform.GetComponent.<NetworkView>().RPC("MyPosition",RPCMode.Others,transform.position,transform.rotation);
			posTime = 0;
		}
		
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
function MyPosition(pos : Vector3,rot : Quaternion)
{

	transform.position = pos;
	transform.rotation = rot;
	
}