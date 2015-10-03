#pragma strict

var HayMesh : MeshRenderer;

var fadeTime : float = 0.2f;

function OnTriggerEnter (other : Collider) 
{
	transform.GetChild(0).GetComponent(ParticleSystem).Play();
	DoCollision();
}

function DoCollision()
{

	transform.GetComponent(BoxCollider).enabled = false;

	var startTime : float = Time.time;
	
	while(Time.time - startTime < fadeTime)
	{
		HayMesh.material.color.a = Mathf.Lerp(1,0,(Time.time - startTime)/fadeTime);
		yield;
	}
	
	HayMesh.material.color.a = 0;
	
	yield WaitForSeconds(30f);
	
	startTime = Time.time;
	
	while(Time.time - startTime < fadeTime)
	{
		HayMesh.material.color.a = Mathf.Lerp(0,1,(Time.time - startTime)/fadeTime);
		yield;
	}
	
	transform.GetComponent(BoxCollider).enabled = true;

}
