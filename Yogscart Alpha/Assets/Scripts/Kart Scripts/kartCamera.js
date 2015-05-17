#pragma strict

var Target : Transform;

var Distance : float = 6f;
var Height : float = 2f;
var PlayerHeight : float = 2f;
var Angle : float = 0f;
var sideAmount : float = 0f;

var rotsmoothTime : float = 5;


function Update () {

	if(Target != null)
	{
	
		var quat : Quaternion;
		quat = Quaternion.AngleAxis(Angle,Vector3.up);

		var For : Vector3;
		For = quat * (-Target.forward * Distance);
		
		var pos = Target.position + For + (Vector3.up * Height);
		
		transform.position = pos;

		var lookDir : Vector3 = Target.position - (transform.position-(Vector3.up*PlayerHeight) + (transform.right * sideAmount));

		transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(lookDir),Time.deltaTime*rotsmoothTime);

	}

}