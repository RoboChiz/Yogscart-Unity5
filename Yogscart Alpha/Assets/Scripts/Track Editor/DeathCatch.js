#pragma strict

var DeathParticles : ParticleSystem;

function OnTriggerEnter (other : Collider) {
if(other.name == "DeathCatch"){

DeathParticles.Play();

if(transform.GetComponent(Position_Finding) != null)
{
if(transform.GetComponent(kartScript) != null)
transform.GetComponent(kartScript).ExpectedSpeed = 0;

yield WaitForSeconds(0.75);

GetComponent.<Rigidbody>().isKinematic = true;

if(transform.GetComponent(kartScript) != null){
var td = GameObject.Find("Track Manager").transform.GetComponent(TrackData);

var nPos = td.PositionPoints[transform.GetComponent(Position_Finding).currentPos].position;
if(transform.GetComponent(Position_Finding).currentPos+1 < td.PositionPoints.Length)
var n1Pos = td.PositionPoints[transform.GetComponent(Position_Finding).currentPos+1].position;
else
n1Pos = td.PositionPoints[0].position;

transform.position = nPos;

transform.rotation = Quaternion.Euler(0,0,0);
transform.rotation = Quaternion.LookRotation(n1Pos-nPos,Vector3.up);
}

GetComponent.<Rigidbody>().isKinematic = false;
}
yield WaitForSeconds(0.1);


DeathParticles.Stop();

}
}