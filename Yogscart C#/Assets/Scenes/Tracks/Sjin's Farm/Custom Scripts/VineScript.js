#pragma strict

function Start () {

	var ani : Animator;
	ani = transform.GetComponent(Animator);
	
	while(true)
	{
		transform.rotation = Quaternion.LookRotation(Vector3(Random.Range(0,101)/100f,0,Random.Range(0,101)/100f));
		yield WaitForSeconds(Random.Range(5,12));
		ani.SetTrigger("Show");
		yield WaitForSeconds(12);
	}
	
}
