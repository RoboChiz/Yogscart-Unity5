#pragma strict

function Start () {

var ks = transform.parent.GetComponent(kartScript);
ks.Boost(2,"Boost");

yield;
Destroy(this.gameObject);

}