#pragma strict

var target : Transform;

var distance : float = 6f;
var height : float = 2f;
var playerHeight : float = 2f;
var angle : float = 0f;
var sideAmount : float = 0f;

var smoothTime : float = 0.1;
var rotsmoothTime : float = 5;
private var velocity = Vector3.zero;

function Update () 
{

	if(target != null)
	{
	
		var quat : Quaternion;
		quat = Quaternion.AngleAxis(angle,Vector3.up);

		var For : Vector3;
		For = quat * (-target.forward * distance);
		
		var pos = target.position + For + (Vector3.up * height);
		
		if(target.GetComponent.<Rigidbody>() != null)
		{
			velocity = target.GetComponent.<Rigidbody>().velocity;
			transform.position = Vector3.SmoothDamp(transform.position, pos,velocity, smoothTime,float.MaxValue,0.0333f);
			GetComponent.<Camera>().fieldOfView = Mathf.Lerp(GetComponent.<Camera>().fieldOfView,60 + target.GetComponent.<Rigidbody>().velocity.magnitude/4,Time.deltaTime/50f);
		}
		else
		{
			transform.position = Vector3.Lerp(transform.position,pos,smoothTime * Time.deltaTime);
		}

		var lookDir : Vector3 = target.position - (transform.position-(Vector3.up*playerHeight) + (transform.right * sideAmount));

		transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(lookDir),Time.deltaTime*rotsmoothTime);

	}

}