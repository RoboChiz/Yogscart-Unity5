private var ks : kartScript;

var InputName : String;

var camLocked : boolean = false;
var frontCamera : Camera;
var backCamera : Camera;

function FixedUpdate () {

if(ks == null && transform.GetComponent(kartScript))
ks = transform.GetComponent(kartScript);

ks.throttle = Input.GetAxis(InputName+ "Throttle");
ks.steer = Input.GetAxis(InputName+ "Horizontal");

if(Input.GetAxis(InputName+ "Drift")!=0)
ks.drift = true;
else
ks.drift = false;

if(Input.GetAxis(InputName + "Look Behind") != 0 && !camLocked){
backCamera.enabled = true;
frontCamera.enabled = false;
}else{
backCamera.enabled = false;
frontCamera.enabled = true;
}

}

