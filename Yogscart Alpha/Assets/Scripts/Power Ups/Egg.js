#pragma strict

var direction : Vector3;
var flightSpeed : float = 25f;

var nextHeight : float;

var sheilding : boolean;

var parent : Transform;

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

direction = parent.forward;
direction.y = 0;

sheilding = false;

nextHeight = transform.position.y;

transform.GetComponent(SphereCollider).enabled = false;

yield WaitForSeconds(0.5f);

transform.GetComponent(SphereCollider).enabled = true;

yield WaitForSeconds(45);
Destroy(this.gameObject);

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

	if(collision.rigidbody != null || collision.transform.parent.parent.GetComponent.<Rigidbody>() != null)
	{
		if(transform.GetComponent(JR) == null || collision.transform != transform.GetComponent(JR).parent)
		{
			if(collision.transform.parent.parent.GetComponent(kartScript) != null){
				collision.transform.parent.parent.GetComponent(kartScript).SpinOut();
			}

			Destroy(this.gameObject);
		}
	}
	else
	{
	      var contact : ContactPoint = collision.contacts[0];
          var reflection = contact.normal + transform.forward;
          direction = transform.TransformDirection(reflection.normalized * 15.0);
		
	}
}