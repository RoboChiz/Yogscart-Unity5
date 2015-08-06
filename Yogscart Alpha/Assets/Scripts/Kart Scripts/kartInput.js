private var ks : kartScript;
private var im : InputManager;

var InputNum : int;

var camLocked : boolean = false;
var frontCamera : Camera;
var backCamera : Camera;

function FixedUpdate () {

	var go = GameObject.Find("GameData");

	if(go != null)
	{
		im = go.GetComponent(InputManager);

		if(ks == null && transform.GetComponent(kartScript))
			ks = transform.GetComponent(kartScript);

		ks.throttle = im.c[InputNum].GetInput("Throttle");
		ks.steer = im.c[InputNum].GetInput("Horizontal");
		
		if(im.c[InputNum].GetInput("Drift")!=0)
			ks.drift = true;
		else
			ks.drift = false;

		if(im.c[InputNum].GetInput("Look Behind") != 0 && !camLocked){
			backCamera.enabled = true;
			frontCamera.enabled = false;
		}else{
			backCamera.enabled = false;
			frontCamera.enabled = true;
		}
	}

}

