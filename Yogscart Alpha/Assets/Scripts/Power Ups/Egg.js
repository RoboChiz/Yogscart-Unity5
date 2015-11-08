#pragma strict

var direction : Vector3;
var flightSpeed : float = 25f;

var nextHeight : float;

var sheilding : boolean;

var parent : Transform;

private var bounces : int = 5;

function Start()
{

var sm : Sound_Manager = GameObject.Find("Sound System").GetComponent(Sound_Manager); 

GetComponent.<AudioSource>().volume = (sm.MasterVolume/100f) * (sm.SFXVolume/150f);

sheilding = true;
parent = transform.parent;

while(transform.parent != null)
{
	yield;
}

var inputDir : float = parent.GetComponent(kartItem).inputDirection;

direction = Mathf.Sign(inputDir) * parent.forward;
direction.y = 0;

sheilding = false;

nextHeight = transform.position.y;

transform.GetComponent(SphereCollider).enabled = false;

yield WaitForSeconds(0.5f);

transform.GetComponent(SphereCollider).enabled = true;


}

function FixedUpdate () {
	if(!sheilding)
	{
		transform.position.y = nextHeight;

		transform.position += direction.normalized*flightSpeed*Time.fixedDeltaTime;
		transform.rotation *= Quaternion.Euler(Vector3(1,1,1)*flightSpeed*Time.fixedDeltaTime*5f);

		var hits : RaycastHit[];
		hits = Physics.RaycastAll(transform.position + (direction.normalized*flightSpeed*Time.fixedDeltaTime),Vector3.down,15);
		
		for(var i : int = 0; i < hits.Length; i++)
		{
			if(hits[i].collider.GetComponent.<Rigidbody>() == null)
			{
				nextHeight = hits[i].point.y + 1f;
				break;
			}
		}
							
	}
}



 function OnCollisionEnter(collision : Collision) 
 {
 	if(collision.transform.GetComponent(kartScript) != null)
 	{
 		collision.transform.GetComponent(kartScript).SpinOut();
 		
 		if(collision.transform.GetComponent(kartInfo) != null)
			collision.transform.GetComponent(kartInfo).itemFlashing = false;
		
		parent.GetComponent(kartScript).HitEnemy();	
					
 		Destroy(this.gameObject);
 	}
 	else if(collision.transform.GetComponent(Egg) != null || collision.transform.GetComponent(DirtBlock) != null)
 	{
 		Destroy(this.gameObject);
 	}
 	else
 	{
 		if(bounces > 0)
 		{
 			direction = Vector3.Reflect(direction,collision.contacts[0].normal);
 			bounces --;
 		} else {
 			Destroy(this.gameObject);
 		}
 	}
 }