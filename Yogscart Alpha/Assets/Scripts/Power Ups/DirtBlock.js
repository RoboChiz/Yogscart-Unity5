#pragma strict

private var parent : Transform;

function Awake()
{
	parent = transform.parent;
}

function OnCollisionEnter(collision : Collision) 
{
	if(collision.transform.GetComponent(kartScript) != null)
	{
		collision.transform.GetComponent(kartScript).SpinOut();
		
		if(parent != null && collision.transform != parent)
			parent.GetComponent(kartScript).HitEnemy();
	}

	if(collision.transform.GetComponent.<Rigidbody>() != null)
	{
		Destroy(this.gameObject);
	}
}