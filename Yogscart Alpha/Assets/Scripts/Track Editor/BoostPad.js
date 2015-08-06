#pragma strict

function OnTriggerEnter (other : Collider) {

	var parent : Transform = other.transform;

	while(parent.parent != null)
		parent = parent.parent;

	if(parent.GetComponent(kartScript) != null){
		parent.GetComponent(kartScript).Boost(2,"Boost");
	}
	
}

function Update()
{
	GetComponent.<MeshRenderer>().material.mainTextureOffset -= Vector2(0,Time.deltaTime);
}