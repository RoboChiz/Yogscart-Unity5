#pragma strict
@script ExecuteInEditMode()

var distance : float = 100f;

function Update () {
	
	var lr : LineRenderer = transform.GetComponent(LineRenderer);
	lr.SetPosition(0,transform.position);
	lr.SetPosition(1,transform.position + (Vector3.up * -distance));
}