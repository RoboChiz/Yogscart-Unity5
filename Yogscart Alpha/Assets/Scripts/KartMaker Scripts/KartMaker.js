#pragma strict

//enums
enum KartType{Display,Local,Online,Spectator};

private var gd : CurrentGameData;
private var im : InputManager;

var kartBase : Transform;

var DebugMode : boolean;

function Start()
{

gd = transform.GetComponent(CurrentGameData);
im = transform.GetComponent(InputManager);

if(DebugMode)
SpawnKart(KartType.Local,Vector3.zero,Quaternion.identity,Random.Range(0,gd.Karts.Length),Random.Range(0,gd.Wheels.Length),Random.Range(0,gd.Characters.Length),Random.Range(0,gd.Hats.Length));

}

function SpawnKart(kartType : KartType, position : Vector3, rotation : Quaternion, kart : int, wheel : int, character : int, hat : int)
{

var spawnedKartBase : Transform = Instantiate(kartBase,Vector3.zero,Quaternion.identity);
var ks : kartScript = spawnedKartBase.GetComponent(kartScript);


//Spawn Kart & Wheels
var kartBody : Transform = Instantiate(gd.Karts[kart].model,Vector3.zero,Quaternion.identity);
kartBody.parent = spawnedKartBase.FindChild("Kart Body");

var kartSkel : KartSkeleton = kartBody.GetComponent(KartSkeleton);

var frontlWheel : Transform = Instantiate(gd.Wheels[wheel].model,kartSkel.FrontLPosition,Quaternion.Euler(0,0,0));
frontlWheel.parent = spawnedKartBase.FindChild("Kart Body");
frontlWheel.name = "FrontL Wheel";

var frontrWheel : Transform = Instantiate(gd.Wheels[wheel].model,kartSkel.FrontRPosition,Quaternion.Euler(0,180,0));
frontrWheel.parent = spawnedKartBase.FindChild("Kart Body");
frontrWheel.name = "FrontR Wheel";

var backlWheel : Transform = Instantiate(gd.Wheels[wheel].model,kartSkel.BackLPosition,Quaternion.Euler(0,0,0));
backlWheel.parent = spawnedKartBase.FindChild("Kart Body");
backlWheel.name = "BackL Wheel";

var backrWheel : Transform = Instantiate(gd.Wheels[wheel].model,kartSkel.BackRPosition,Quaternion.Euler(0,180,0));
backrWheel.parent = spawnedKartBase.FindChild("Kart Body");
backrWheel.name = "BackR Wheel";

//Spawn Character & Hat
var characterMesh : Transform = Instantiate(gd.Characters[character].model,Vector3(0,0,0),Quaternion.identity);
characterMesh.name = "Character";

var charSkel : CharacterSkeleton = characterMesh.GetComponent(CharacterSkeleton);

characterMesh.position = kartSkel.SeatPosition - charSkel.SeatPosition;
characterMesh.parent = spawnedKartBase.FindChild("Kart Body");

if(hat != 0){

var hatMesh : Transform = Instantiate(gd.Hats[hat].Model,charSkel.HatHolder.position,Quaternion.identity);
hatMesh.parent = charSkel.HatHolder;
hatMesh.localRotation = Quaternion.Euler(0,0,0);

}

if(kartType == kartType.Display){
//Delete unwanted scripts
}
else
{

ks.wheelMeshes[0] = frontlWheel;
ks.wheelMeshes[1] = frontrWheel;
ks.wheelMeshes[2] = backlWheel;
ks.wheelMeshes[3] = backrWheel;

}


kartBody.position = position;
kartBody.rotation = rotation;

return kartBody;

}