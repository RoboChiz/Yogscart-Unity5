﻿#pragma strict

var specObjects : GameObject[];
var currentSpec : int = 0;

private var velocity = Vector3.zero;
private var gd : CurrentGameData;
private var im : InputManager;

@HideInInspector
var locked : boolean;

enum CameraMode{Spin,Player};
var camMode : CameraMode = CameraMode.Spin;

function Start(){
gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
im = GameObject.Find("GameData").GetComponent(InputManager);
}

function RandomSort(){
while(true){
yield WaitForSeconds(Random.Range(5,20));
currentSpec += 1;

if(currentSpec >= specObjects.Length)
currentSpec = 0;
}
}

function Update () {
	if(specObjects == null || specObjects.Length == 0){
	
		specObjects = GameObject.FindGameObjectsWithTag("Spectated");
		transform.GetComponent(Kart_Camera).enabled = false;
	}else{
	
			transform.GetComponent(Kart_Camera).enabled = true;
			transform.GetComponent(FlyThrough).enabled = false;
			
			var Target = specObjects[currentSpec].transform;
			
			transform.GetComponent(Kart_Camera).target = Target;
			
			if(!locked){
				if(im.c[0].GetMenuInput("Submit") != 0){
				currentSpec += 1;
				
				if(currentSpec >= specObjects.Length)
				currentSpec = 0;
				
				}		
			}
			
			if(camMode == CameraMode.Spin)
			{
				transform.GetComponent(Kart_Camera).angle += Time.deltaTime*20;			
				if(transform.GetComponent(Kart_Camera).angle >360)
				transform.GetComponent(Kart_Camera).angle = 0;
			}
			else
			{
				transform.GetComponent(Kart_Camera).angle = 0;
			}
	}

	
	if(im.c[0].GetMenuInput("Rotate") != 0)
	{
		if(camMode == 0)
			camMode = 1;
		else
			camMode = 0;
	}
		
}