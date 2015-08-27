#pragma strict

var target : Transform;

var distance : float = 6f;
var height : float = 2f;
var playerHeight : float = 2f;
var angle : float = 0f;
var sideAmount : float = 0f;

var turnSmooth : float = 5;
var rotsmoothTime : float = 5;

private var finalRot : Quaternion;

function Update () 
{

	if(target != null)
	{
	
		var quat : Quaternion;
		quat = Quaternion.AngleAxis(angle,Vector3.up);
		
		var test : Quaternion = Quaternion.LookRotation(-target.forward,Vector3.up);
		finalRot = Quaternion.Lerp(finalRot, test * quat,Time.deltaTime * turnSmooth);
		
		var pos = target.position + (finalRot * Vector3.forward * distance) + (Vector3.up * height);
		
		transform.position = pos;
		
		var lookDir : Vector3 = target.position - (transform.position-(Vector3.up*playerHeight) + (transform.right * sideAmount));
		transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(lookDir),Time.deltaTime*rotsmoothTime);
		
	}

}