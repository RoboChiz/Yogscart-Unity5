#pragma strict

//enums
enum KartType{Display,Local,Online,Spectator};

private var gd : CurrentGameData;
private var im : InputManager;

var DebugMode : boolean;

function Start()
{

gd = transform.GetComponent(CurrentGameData);
im = transform.GetComponent(InputManager);

	if(DebugMode)
		SpawnKart(KartType.Local,Vector3.zero,Quaternion.identity,Random.Range(0,gd.Karts.Length),Random.Range(0,gd.Wheels.Length),Random.Range(0,gd.Characters.Length),Random.Range(0,gd.Hats.Length));

}

function SpawnKart(kartType : KartType, position : Vector3, rotation : Quaternion, kart : int, wheel : int, character : int, hat : int) : Transform
{

	//Spawn Kart & Wheels
	var kartBody : Transform = Instantiate(gd.Karts[kart].model,Vector3(0,0,0),Quaternion.identity);
	var kartSkel : KartSkeleton = kartBody.GetComponent(KartSkeleton);

	var frontlWheel : Transform = Instantiate(gd.Wheels[wheel].model,kartSkel.FrontLPosition,Quaternion.Euler(0,0,0));
	frontlWheel.parent = kartBody.FindChild("Kart Body");
	frontlWheel.name = "FrontL Wheel";

	var frontrWheel : Transform = Instantiate(gd.Wheels[wheel].model,kartSkel.FrontRPosition,Quaternion.Euler(0,180,0));
	frontrWheel.parent = kartBody.FindChild("Kart Body");
	frontrWheel.name = "FrontR Wheel";

	var backlWheel : Transform = Instantiate(gd.Wheels[wheel].model,kartSkel.BackLPosition,Quaternion.Euler(0,180,0));
	backlWheel.parent = kartBody.FindChild("Kart Body");
	backlWheel.name = "BackL Wheel";

	var backrWheel : Transform = Instantiate(gd.Wheels[wheel].model,kartSkel.BackRPosition,Quaternion.Euler(0,0,0));
	backrWheel.parent = kartBody.FindChild("Kart Body");
	backrWheel.name = "BackR Wheel";

	//Spawn Character & Hat
	var characterMesh : Transform = Instantiate(gd.Characters[character].model,Vector3(0,0,0),Quaternion.identity);
	characterMesh.name = "Character";

	var charSkel : CharacterSkeleton = characterMesh.GetComponent(CharacterSkeleton);

	characterMesh.position = kartSkel.SeatPosition - charSkel.SeatPosition;
	characterMesh.parent = kartBody.FindChild("Kart Body");

	if(hat != 0){

	var hatMesh : Transform = Instantiate(gd.Hats[hat].Model,charSkel.HatHolder.position,Quaternion.identity);
	hatMesh.parent = charSkel.HatHolder;
	hatMesh.localRotation = Quaternion.Euler(0,0,0);

	}

	if(kartType != KartType.Display){

	var kb : GameObject = kartBody.gameObject;

	kb.AddComponent(Rigidbody);
	kb.GetComponent.<Rigidbody>().mass = 2000;
	kb.GetComponent.<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
	kb.GetComponent.<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
	kb.GetComponent.<Rigidbody>().angularDrag = 0;

	kb.AddComponent(AudioSource);
	kb.GetComponent.<AudioSource>().clip = kartSkel.engineSound;
	kb.GetComponent.<AudioSource>().spatialBlend = 1;
	kb.GetComponent.<AudioSource>().minDistance = 0;
	kb.GetComponent.<AudioSource>().maxDistance = 35;
	kb.GetComponent.<AudioSource>().rolloffMode = AudioRolloffMode.Linear;
	
	var kartBodyBody = kartBody.FindChild("Kart Body").gameObject;
	kartBodyBody.AddComponent(AudioSource);
	kartBodyBody.GetComponent.<AudioSource>().spatialBlend = 1;
	kartBodyBody.GetComponent.<AudioSource>().minDistance = 0;
	kartBodyBody.GetComponent.<AudioSource>().maxDistance = 25;
	kartBodyBody.GetComponent.<AudioSource>().rolloffMode = AudioRolloffMode.Linear;
	
	kb.GetComponent.<AudioSource>().playOnAwake = false;
	kartBodyBody.GetComponent(AudioSource).playOnAwake = false;

	kb.AddComponent(DeathCatch);
	kb.GetComponent(DeathCatch).DeathParticles = kartBody.FindChild("Kart Body").FindChild("Particles").FindChild("Death Particles").GetComponent.<ParticleSystem>();

	var frontlWheelCollider : Transform = Instantiate(gd.Wheels[wheel].model,kartSkel.FrontLPosition,Quaternion.Euler(0,0,0));
	frontlWheelCollider.name = "FrontL Wheel";
	frontlWheelCollider.parent = kartBody.FindChild("Colliders");
	SetUpWheelCollider(frontlWheelCollider);

	var frontrWheelCollider : Transform = Instantiate(gd.Wheels[wheel].model,kartSkel.FrontRPosition,Quaternion.Euler(0,180,0));
	frontrWheelCollider.parent = kartBody.FindChild("Colliders");
	frontrWheelCollider.name = "FrontR Wheel";
	SetUpWheelCollider(frontrWheelCollider);

	var backlWheelCollider : Transform = Instantiate(gd.Wheels[wheel].model,kartSkel.BackLPosition,Quaternion.Euler(0,180,0));
	backlWheelCollider.parent = kartBody.FindChild("Colliders");
	backlWheelCollider.name = "BackL Wheel";
	SetUpWheelCollider(backlWheelCollider);

	var backrWheelCollider : Transform = Instantiate(gd.Wheels[wheel].model,kartSkel.BackRPosition,Quaternion.Euler(0,0,0));
	backrWheelCollider.parent = kartBody.FindChild("Colliders");
	backrWheelCollider.name = "BackR Wheel";
	SetUpWheelCollider(backrWheelCollider);

	kb.AddComponent(kartAnimation);
	kb.GetComponent(kartAnimation).ani = characterMesh.GetComponent(Animator);

	kb.AddComponent(kartScript);

	var ks = kb.GetComponent(kartScript);

	ks.engineSound = kartSkel.engineSound;

	ks.wheelColliders = new WheelCollider[4];
	ks.wheelColliders[0] = frontlWheelCollider.GetComponent(WheelCollider);
	ks.wheelColliders[1] = frontrWheelCollider.GetComponent(WheelCollider);
	ks.wheelColliders[2] = backlWheelCollider.GetComponent(WheelCollider);
	ks.wheelColliders[3] = backrWheelCollider.GetComponent(WheelCollider);

	ks.wheelMeshes = new Transform[4];
	ks.wheelMeshes[0] = frontlWheel;
	ks.wheelMeshes[1] = frontrWheel;
	ks.wheelMeshes[2] = backlWheel;
	ks.wheelMeshes[3] = backrWheel;

	var kp : Transform = kartBody.FindChild("Kart Body").FindChild("Particles");

	ks.flameParticles = new ParticleSystem[2];
	ks.flameParticles[0] = kp.FindChild("L_Flame").GetComponent.<ParticleSystem>();
	ks.flameParticles[1] = kp.FindChild("R_Flame").GetComponent.<ParticleSystem>();

	ks.DriftParticles = new Transform[2];
	ks.DriftParticles[0] = kp.FindChild("L_Sparks");
	ks.DriftParticles[1] = kp.FindChild("R_Sparks");

	ks.TrickParticles = kp.FindChild("Trick").GetComponent.<ParticleSystem>();

	kb.AddComponent(Position_Finding);
	kb.AddComponent(kartUpdate);
	
	kb.GetComponent(kartScript).hitSounds = gd.Characters[character].hitSounds;
	kb.GetComponent(kartScript).tauntSounds = gd.Characters[character].tauntSounds;


	if(kartType != KartType.Display)
	{
		if(kartType != KartType.Spectator)
		{
			//Add Script
			kb.AddComponent(kartInput);
			kb.AddComponent(kartInfo);
			//Adjust Scripts
			kb.GetComponent(kartInput).InputNum = 0;
		}
		//Apply to Spectators
		kb.AddComponent(kartItem);
		kb.GetComponent(kartItem).itemDistance = kartSkel.ItemDrop;
		
		}
	}

	//Clear Up
	Destroy(kartSkel);
	Destroy(charSkel);

	kartBody.position = position;
	kartBody.rotation = rotation;
	
	kartBody.gameObject.layer = 8;//Set the Kart's Layer to "Kart" for Kart Collisions 
	
	return kartBody.transform;

}

function SetUpWheelCollider(collider : Transform)
{

collider.gameObject.AddComponent(WheelCollider);

var wheelCollider = collider.GetComponent(WheelCollider);

//Setup Collider Settings
wheelCollider.mass = 20;

if(collider.GetComponent(WheelSkeleton) != null)
{
	wheelCollider.radius = collider.GetComponent(WheelSkeleton).wheelRadius;
	Destroy(collider.GetComponent(WheelSkeleton));
}
else
	wheelCollider.radius = 0.15;

wheelCollider.wheelDampingRate = 0.05;
wheelCollider.suspensionDistance = 0.25;
wheelCollider.forceAppPointDistance = 1.25f;

wheelCollider.suspensionSpring.spring = 25000;
wheelCollider.suspensionSpring.damper = 25000;
wheelCollider.suspensionSpring.targetPosition = 1;

wheelCollider.forwardFriction.extremumSlip = 0.8;
wheelCollider.forwardFriction.extremumValue = 3;
wheelCollider.forwardFriction.asymptoteSlip = 1.5;
wheelCollider.forwardFriction.asymptoteValue = 2.25;
wheelCollider.forwardFriction.stiffness = 1;

wheelCollider.sidewaysFriction.extremumSlip = 0.8;
wheelCollider.sidewaysFriction.extremumValue = 3;
wheelCollider.sidewaysFriction.asymptoteSlip = 1.5;
wheelCollider.sidewaysFriction.asymptoteValue = 2.25;
wheelCollider.sidewaysFriction.stiffness = 3;

Destroy(collider.GetComponent(MeshFilter));
Destroy(collider.GetComponent(MeshRenderer));


}