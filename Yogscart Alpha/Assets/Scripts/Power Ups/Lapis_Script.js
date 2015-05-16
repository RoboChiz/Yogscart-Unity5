#pragma strict

var PhysicsProp : Transform;

function OnTriggerEnter (other : Collider) {

if(other.transform.name == "mesh Collider"){
	Hit();
	other.transform.parent.parent.GetComponent(kartScript).lapisAmount += 1;
}

}

function Hit(){

var Particles = Instantiate(PhysicsProp,transform.position,transform.rotation);
var OriginalHeight : float = Particles.position.y;

GetComponent.<Renderer>().enabled = false;
GetComponent.<Collider>().enabled = false;

yield WaitForSeconds(1);

while(Particles.position.y > OriginalHeight - 0.5){
Particles.position.y -= Time.deltaTime/10f;
yield;
}

Destroy(Particles.gameObject);

yield WaitForSeconds(50);

GetComponent.<Renderer>().enabled = true;
GetComponent.<Collider>().enabled = true;

}