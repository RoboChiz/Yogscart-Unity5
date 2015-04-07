#pragma strict

var Target : Transform;

var Distance : float = 1.6f;
var Height : float = 0.6f;
var PlayerHeight : float = 0.6f;
var Angle : float;
var sideAmount : float;

var smoothTime : float = 0.1;
var rotsmoothTime : float = 5;
private var velocity = Vector3.zero;

var Locked : boolean;

function FixedUpdate () {

if(Target != null)
{
velocity = Target.GetComponent.<Rigidbody>().velocity;

var quat : Quaternion;
quat = Quaternion.AngleAxis(Angle,Vector3.up);

var For : Vector3;
For = quat * (-Target.forward * Distance);

var pos = Target.position + For + (Vector3.up * Height);

transform.position = Vector3.SmoothDamp(transform.position, pos,velocity, smoothTime);

var lookDir : Vector3 = Target.position - (transform.position-(Vector3.up*PlayerHeight) + (transform.right * sideAmount));

transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(lookDir),Time.deltaTime*rotsmoothTime);

GetComponent.<Camera>().fieldOfView = Mathf.Lerp(GetComponent.<Camera>().fieldOfView,60 + Target.GetComponent.<Rigidbody>().velocity.magnitude/4,Time.deltaTime/50f);
}
}
