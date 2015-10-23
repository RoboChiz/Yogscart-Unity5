#pragma strict

var RotateSpeed : float = 0.5;
var FloatSpeed : float = 0.001;
private var Origin : float;
private var Up : boolean;

function Start(){
Origin = transform.position.y;
}

function FixedUpdate () {

transform.Rotate(Vector3.up,RotateSpeed);

if(Up == true){
transform.position.y += FloatSpeed;
}else{
transform.position.y -= FloatSpeed;
}

if(transform.position.y > Origin + 0.1){
Up = false;
}

if(transform.position.y < Origin - 0.1){
Up = true;
}

}