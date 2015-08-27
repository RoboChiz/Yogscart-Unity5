#pragma strict

import UnityEngine.Networking;

public class NetworkExtras extends NetworkBehaviour
{

	//used to create Karts
	private var km : KartMaker;
	
	function Start()
	{
		km = transform.GetComponent(KartMaker);
	}
		

	//Spawns a kart lcoally, and tells the server to spawn it across the client.
	@ClientRpc
	function RpcSpawnKart(typeInt : int, pos : Vector3, rot : Quaternion, kart : int, wheel : int, character : int, hat : int )
	{
	
		var type : KartType = typeInt;

		var kartPrefab : Transform = km.SpawnKart(type,pos,rot,kart,wheel,character,hat);
		
		//Add Camera
		var IngameCam = Instantiate(Resources.Load("Prefabs/Cameras",Transform),pos,Quaternion.identity);
		IngameCam.name = "InGame Cams";
		
		kartPrefab.GetComponent(kartInput).InputNum = 0;
		kartPrefab.GetComponent(kartInput).camLocked = true;
		kartPrefab.GetComponent(kartInput).frontCamera = IngameCam.GetChild(1).GetComponent.<Camera>();
		kartPrefab.GetComponent(kartInput).backCamera = IngameCam.GetChild(0).GetComponent.<Camera>();
		
		IngameCam.GetChild(1).tag = "MainCamera";

		IngameCam.GetChild(0).transform.GetComponent(Kart_Camera).target = kartPrefab;
		IngameCam.GetChild(1).transform.GetComponent(Kart_Camera).target = kartPrefab;

		kartPrefab.gameObject.AddComponent(NetworkIdentity);

			//SetUpCameras
		var copy = new Array();
		copy.Push(IngameCam.GetChild(0).GetComponent.<Camera>());
		copy.Push(IngameCam.GetChild(1).GetComponent.<Camera>());

		kartPrefab.GetComponent(kartInfo).cameras = copy;
		
		ClientScene.RegisterPrefab(kartPrefab.gameObject);
		
		var returnArray : Object[] = new Object[2];
		returnArray[0] = kartPrefab;
		returnArray[1] = IngameCam;
		
		return returnArray;
	
	}



}	