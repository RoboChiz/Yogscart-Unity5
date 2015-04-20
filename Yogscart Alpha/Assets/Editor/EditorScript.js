#pragma strict

//EditorScript Script - V0.1
//Created by Robert (Robo_Chiz)

@CustomEditor (TrackData)
public class TrackDataEditor extends Editor{

	//Readd this secion of code when a nicer Camera System has been added
    //function OnInspectorGUI(){
	
	//var td : TrackData = target;
	
	//td.TrackName = EditorGUILayout.TextField("Track Name:",td.TrackName);
	//td.Scale = EditorGUILayout.FloatField("Kart Scale:",td.Scale);
	//td.Scale = Mathf.Clamp(td.Scale,0.01,Mathf.Infinity);
	
	//td.LoopedTrack = EditorGUILayout.Toggle("Looped Track:",td.LoopedTrack);
	//if(td.LoopedTrack == true)
	//td.Laps = EditorGUILayout.IntField("Number of Laps:",td.Laps);
	//td.Laps = Mathf.Clamp(td.Laps,1,Mathf.Infinity);
	
	//if(GUI.changed)
	//EditorUtility.SetDirty(target);
        
   // }
    
    @MenuItem ("Track Editor/Create Track Manager")
    static function CTM () {
    if(GameObject.Find("Track Manager") == null){
    
    var obj = new GameObject();
    
    obj.AddComponent(TrackData);
    
    }
    }
    
    @MenuItem ("Track Editor/Create Position Point at end of queue")
    static function CPP () {
        if(GameObject.Find("Track Manager") != null && GameObject.Find("Track Manager").GetComponent(TrackData) != null)
        GameObject.Find("Track Manager").GetComponent(TrackData).NewPoint();
        else
        Debug.Log("Something's gone wrong! Make sure that you have setup a track in this scene."); 
    }
    
    @MenuItem ("Track Editor/Create Position Point after selection")
    static function CPPAS() {
        if(GameObject.Find("Track Manager") != null && GameObject.Find("Track Manager").GetComponent(TrackData) != null){
	        var td = GameObject.Find("Track Manager").GetComponent(TrackData);
	        for(var i : int = 0; i < td.PositionPoints.Length; i++){
		        if(td.PositionPoints[i] == Selection.activeTransform){
			        td.AddPoint(i+1);
			        break; 
		        }	  
	        }
	        
	        
        }else
        Debug.Log("Something's gone wrong! Make sure that you have setup a track in this scene."); 
    }
    
    @MenuItem ("Track Editor/Create Short Cut")
    static function CSC() {
        if(GameObject.Find("Track Manager") != null && GameObject.Find("Track Manager").GetComponent(TrackData) != null){
	        var td = GameObject.Find("Track Manager").GetComponent(TrackData);
	         for(var i : int = 0; i < td.PositionPoints.Length; i++){
		        if(td.PositionPoints[i] == Selection.activeTransform){
			        td.AddShortCutPoint(td.PositionPoints[i]);
			        break; 
		        }
	        }	          
		}
    }
    
    @MenuItem ("Track Editor/Create Short Cut Point after Selection")
    static function CSCP() {
        if(GameObject.Find("Track Manager") != null && GameObject.Find("Track Manager").GetComponent(TrackData) != null){
	        var td = GameObject.Find("Track Manager").GetComponent(TrackData);
	        
	         for(var i : int = 0; i < td.ShortCuts.Length; i++){
	         	for(var j : int = 0; j < td.ShortCuts[i].PositionPoints.Length; j++){
	         		if(td.ShortCuts[i].PositionPoints[j] == Selection.activeTransform){
			        	td.AddShortCutPointPoint(i);
			        	break; 
		        	}
	         	}
	        }
	        	          
		}
    }
    
    @MenuItem ("Track Editor/Play Test Track (Normal Race)")
    static function PTT_NR () {
    	if(GameObject.Find("GameData") == null)
    	   LoadLevel(0);
    	else
    	Debug.Log("Please delete the existing GameData");
    }
    
     @MenuItem ("Track Editor/Play Test Track (Time Trial)")
    static function PTT_TT () {
    	if(GameObject.Find("GameData") == null)
    	   LoadLevel(1);
    	else
    	Debug.Log("Please delete the existing GameData");
    }
    
    static function LoadLevel (i : int) {
    /*
    if(Application.isPlaying == false){
		if(GameObject.Find("Track Manager") != null && GameObject.Find("Track Manager").GetComponent(TrackData) != null && Resources.Load("Prefabs/GameData",Transform) != null){
 			EditorApplication.isPlaying = true;
 			var gd = Instantiate(Resources.Load("Prefabs/GameData",Transform),Vector3.zero,Quaternion.identity);
 			gd.name = "GameData";
 			gd.gameObject.AddComponent(Race_Test);
 			
 			gd.GetComponent(InputManager).c = new InputController[1];
 			if(Input.GetJoystickNames().Length > 0)
 			gd.GetComponent(InputManager).c[0] = new InputController("C1_");
 			else
 			gd.GetComponent(InputManager).c[0] = new InputController("Key_");
 			
 			if(i == 0)
 			gd.GetComponent(Race_Test).type = RaceStyle.CustomRace;
 			if(i == 1)
 			gd.GetComponent(Race_Test).type = RaceStyle.TimeTrial;
 			
		}else{
			Debug.Log("Cannot test this level as there is no Track!");
		}
    }else{
    Debug.Log("Game is already running!");
    }*/
}
    

}


