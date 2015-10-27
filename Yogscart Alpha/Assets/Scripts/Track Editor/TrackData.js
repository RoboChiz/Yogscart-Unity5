#pragma strict
@script ExecuteInEditMode()
//TrackData Script - V2.0
//Created by Robert (Robo_Chiz)
//Do not edit this script, doing so may cause compatibility errors.

var TrackName : String;

var backgroundMusic : AudioClip;

@HideInInspector
var LoopedTrack : boolean = true;
var Laps : int = 3;

@HideInInspector
var pointsNeededToLap : int[];

@HideInInspector
var spawnPoint : Transform;

//@HideInInspector
var PositionPoints : Transform[];

var ShortCuts : ShortCut[];

var IntroPans : CameraPoint[];

enum Point{Position,Lap,Shortcut,Spawn};

public class ShortCut
{

	var startPoint : Transform;
	var endPoint : Transform;
	var PositionPoints : Transform[];
	
	enum ShortCutType{BoostRequired,NeedSmarts,SplitPath};
	//In SplitPath AI has 50% chance of taking path, in Needs Smarts only Smart AI will take it, in Boost required an ai won't take it unless they have a jaffa
	var sct : ShortCutType = ShortCutType.NeedSmarts;
	
	public function ShortCut(StartPoint : Transform)
	{
		startPoint = StartPoint;
	}

}

public class CameraPoint{
 var StartPoint : Vector3;
 var EndPoint : Vector3;
 var StartRotation : Vector3;
 var EndRotation : Vector3;
 var TravelTime : float;
 }
 
 
 function Update(){
 
 transform.name = "Track Manager";
 
 //Check for Spawn Point
 	if(spawnPoint == null)
 	{
 		var obj = new GameObject();
 		obj.AddComponent(PointHandler);
 		
 		spawnPoint = obj.transform;		
 		spawnPoint.GetComponent(PointHandler).style = Point.Spawn;
 		spawnPoint.parent = transform;
 	}
 
 //Check for empty objects in Position Points
	if(PositionPoints != null)
	{
	
		for(var i : int = 0; i < PositionPoints.Length; i++){
			if(PositionPoints[i] == null)
				RemovePoint(i);
				
			if (!Application.isPlaying) {
				if(PositionPoints[i].GetComponent(PointHandler).style == Point.Position)
					PositionPoints[i].name = "Position Point " + i;
				else if(PositionPoints[i].GetComponent(PointHandler).style == Point.Shortcut)
					PositionPoints[i].name = "Shortcut Point " + i;
				else
					PositionPoints[i].name = "Lap Point " + i;
					
			}
			
		}
		
		if(PositionPoints[0].GetComponent(PointHandler).style != Point.Lap)
			PositionPoints[0].GetComponent(PointHandler).style = Point.Lap;	

 	}
 	
 	if(ShortCuts != null)
 	{
 		for(var j : int = 0; j < ShortCuts.Length; j++){
 			
 			if(ShortCuts[j].PositionPoints != null)
	 		{		
				for(var k : int = 0; k < ShortCuts[j].PositionPoints.Length; k++){
					if(ShortCuts[j].PositionPoints[k] == null)
						RemoveShortCutPoint(j,k);
					else
						ShortCuts[j].PositionPoints[k].name = "ShortCut Point " + k;
				}
			}
			
			if(ShortCuts[j].PositionPoints == null || ShortCuts[j].PositionPoints.Length == 0)
				RemoveShortCut(j);	
				
		}
 	}

 //Check that there's at least one lap point
 if(PositionPoints == null || PositionPoints.Length == 0){
 NewPoint();
 }
 
 if(transform.GetComponent(InEngineRender) == null)
 gameObject.AddComponent(InEngineRender);
 
 }
 
 function RemovePoint (removei : int){
 
 var copy = new Array();
 
 if(PositionPoints != null){
 for(var i : int = 0; i < PositionPoints.Length; i++)
 if(i != removei)
 copy.Push(PositionPoints[i]);
 
 PositionPoints = copy;
 }
 
 
 }
 
 function RemoveShortCut (removei : int)
 {
 	var copy = new Array();
 	
 	if(ShortCuts != null)
 	{
 		copy = ShortCuts;
 		copy.RemoveAt(removei);
 		ShortCuts = copy;
 	}
 	
 }
 
  function RemoveShortCutPoint (shortCut : int, toDelete : int)
 {
 	var copy = new Array();
 	
 	if(ShortCuts != null)
 	{
 		copy = ShortCuts[shortCut].PositionPoints;
 		copy.RemoveAt(toDelete);
 		ShortCuts[shortCut].PositionPoints = copy;
 	}
 	
 }
 
 function AddPoint (addat : int){
 
	 if(addat >= PositionPoints.Length)
	 	NewPoint();
 	else
 	{
		 var copy = new Array();
		 
		 if(PositionPoints != null){
			 var obj = new GameObject();
			 obj.transform.parent = GameObject.Find("Track Manager").transform;
			 
			 var nPoint : Transform = obj.transform;
			 
			 var pos : Vector3;
			 if(PositionPoints == null || PositionPoints.Length == 0)
			  pos = Vector3(0,0,0);
			  else
			  pos = PositionPoints[addat-1].position;
			 
			 obj.transform.position = pos;
			 
			 obj.AddComponent(PointHandler);
			 
			 for(var i : int = 0; i < addat; i++)
			 copy.Push(PositionPoints[i]);
			 
			 copy.Push(nPoint);
			 
			 for(i = addat; i < PositionPoints.Length; i++)
			 copy.Push(PositionPoints[i]);
			 
			 PositionPoints = copy;
			 
			#if UNITY_EDITOR
			Selection.activeTransform = obj.transform; 
 			#endif
 			
		 }
 
	 }
 }
 
 function AddShortCutPoint (addat : Transform)
 {
 	var copy = new Array();
 	
 	if(ShortCuts != null)
 		copy = ShortCuts;
 		
	var nShortCut = new ShortCut(addat);
	
	var obj = new GameObject();
	obj.transform.parent = addat;
	obj.transform.position = addat.position;
	
	obj.AddComponent(PointHandler);
 	obj.GetComponent(PointHandler).style = Point.Shortcut;
 	
 	nShortCut.PositionPoints = new Transform[1];
 	
 	nShortCut.PositionPoints[0] = obj.transform;
 	
 	copy.Add(nShortCut);
 	
 	ShortCuts = copy;
 	
 	#if UNITY_EDITOR
		Selection.activeTransform = obj.transform; 
 	#endif
	
 }
 
 function AddShortCutPointPoint(shortcut : int)
 {
	 var copy = new Array();
 	
 	if(ShortCuts[shortcut] != null)
 	{
 		copy = ShortCuts[shortcut].PositionPoints;
 		
 		var addat : Transform = ShortCuts[shortcut].PositionPoints[copy.length -1].parent;
	
		var obj = new GameObject();
		obj.transform.parent = addat;
		obj.transform.position = addat.position;
		
		obj.AddComponent(PointHandler);
	 	obj.GetComponent(PointHandler).style = Point.Shortcut;
	 	
	 	var nShortCut : Transform = obj.transform;
	 	
	 	copy.Add(nShortCut);
	 	
	 	ShortCuts[shortcut].PositionPoints = copy;
	 	
	 	#if UNITY_EDITOR
		Selection.activeTransform = obj.transform; 
 		#endif
 		
 	}
 }
 
 function NewPoint(){
 
 var obj = new GameObject();
 
 obj.transform.parent = GameObject.Find("Track Manager").transform;
 
 var pos : Vector3;
 if(PositionPoints == null || PositionPoints.Length == 0)
  pos = Vector3(0,0,0);
  else
  pos = PositionPoints[PositionPoints.Length-1].position;
 
 obj.transform.position = pos;
 
 var copy = new Array();
 
 if(PositionPoints != null)
 	copy = PositionPoints;
 
 var nPoint : Transform = obj.transform;
 
 obj.AddComponent(PointHandler);

 copy.Push(nPoint);
 
 PositionPoints = copy;
 
 #if UNITY_EDITOR
	Selection.activeTransform = obj.transform; 
 #endif
 
 }
 
 function OnDrawGizmos() {
		Gizmos.color = Color.cyan;
		Gizmos.DrawCube(transform.position,Vector3(1,1,1));
	}