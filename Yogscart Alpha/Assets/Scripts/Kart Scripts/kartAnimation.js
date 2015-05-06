#pragma strict
private var ks : kartScript;
var ani : Animator;

function Update () {

if(ks == null)
ks = transform.GetComponent(kartScript);

if(ks != null && ani != null){
ani.SetBool("Drift",ks.drift);
ani.SetFloat("Steer",ks.steer);
}

/*var ki = transform.GetComponent(kartItem); 
if(ki != null){
if(ki.heldPowerUp == -1)
ani.SetBool("Item",false);
else
ani.SetBool("Item",true);
}*/

}
