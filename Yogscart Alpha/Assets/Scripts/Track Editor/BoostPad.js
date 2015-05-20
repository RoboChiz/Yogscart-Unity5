#pragma strict

function OnTriggerEnter (other : Collider) {

	if(other.transform.parent.parent.GetComponent(kartScript) != null){
		other.transform.parent.parent.GetComponent(kartScript).Boost(2,"Boost");
	}
	
}

function Update()
{
	GetComponent.<MeshRenderer>().material.mainTextureOffset -= Vector2(0,Time.deltaTime);
}