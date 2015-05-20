#pragma strict

 function OnCollisionEnter(collision : Collision) 
 {
 	if(collision.transform.GetComponent(kartScript) != null)
 	{
 		collision.transform.GetComponent(kartScript).SpinOut();
 	}
 	
 	if(collision.transform.GetComponent.<Rigidbody>() != null)
 	{
 		Destroy(this.gameObject);
 	}
 }