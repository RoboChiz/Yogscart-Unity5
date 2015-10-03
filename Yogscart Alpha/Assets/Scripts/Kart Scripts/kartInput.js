private var ks : kartScript;
private var im : InputManager;

var InputNum : int;
var inputOveride : String = "";

var camLocked : boolean = false;
var frontCamera : Camera;
var backCamera : Camera;

function FixedUpdate () {

	var go = GameObject.Find("GameData");
	var lookBehind : float = 0;
	
	if(ks == null && transform.GetComponent(kartScript))
			ks = transform.GetComponent(kartScript);
	
	if(inputOveride != "")
	{
		ks.throttle = Input.GetAxis(inputOveride + "Throttle");
		ks.steer = Input.GetAxis(inputOveride + "Horizontal");
		
		if(Input.GetAxis(inputOveride + "Drift")!=0)
			ks.drift = true;
		else
			ks.drift = false;
			
		lookBehind =  Input.GetAxis(inputOveride + "Look Behind"); 
	}
	else if(go != null)
	{
		im = go.GetComponent(InputManager);

		ks.throttle = im.c[InputNum].GetInput("Throttle");
		ks.steer = im.c[InputNum].GetInput("Horizontal");
		
		if(im.c[InputNum].GetInput("Drift")!=0)
			ks.drift = true;
		else
			ks.drift = false;
			
			
		lookBehind = im.c[InputNum].GetInput("Look Behind");
	}
	
	
	
	if((lookBehind != 0) && !camLocked){
		backCamera.enabled = true;
		frontCamera.enabled = false;
	}else{
		backCamera.enabled = false;
		frontCamera.enabled = true;
	}
	
}


