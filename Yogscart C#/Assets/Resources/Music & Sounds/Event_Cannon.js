var Cannon : GameObject;
var anim : Animator;
var Fix = false;
function Start () {
anim = Cannon.GetComponent("Animator");
}

function Update () {

}

function OnTriggerEnter (other : Collider) {
		if(other.gameObject.tag == "Player"){
		anim.SetBool("Fire", true);
		Reset();
		print("Collision");
		other.GetComponent(Animator).enabled = true;
		}
	}
	
function OnCollisionExit(pl : Collision){
if(pl.gameObject.tag == "Player"){
pl.GetComponent(Animator).enabled = false;

}
}

/*function OnCollisionStay(pl : Collision){
if(pl.gameObject.tag == "Player"){
//pl.transform.rigidbody.velocity = pl.transform.rigidbody.velocity * 1;
pl.gameObject.transform.rotation.x = 0; 

}
}*/
function Reset(){
yield WaitForSeconds(1);
anim.SetBool("Fire", false);
yield WaitForSeconds(1);
Fix = true;
}