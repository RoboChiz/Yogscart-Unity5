#pragma strict

function Update () {

	if(Camera.main != null)
		transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
 
}