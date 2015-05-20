#pragma strict

function Start () {

var ks = transform.parent.GetComponent(kartScript);
ks.lapisAmount += 3;

yield;
Destroy(this.gameObject);

}