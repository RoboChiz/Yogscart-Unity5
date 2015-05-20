#pragma strict

function OnTriggerEnter (other : Collider) {

if(other.transform.parent.parent.GetComponent(kartScript) != null){
other.transform.parent.parent.GetComponent(kartScript).StartCoroutine("Boost");
}

}