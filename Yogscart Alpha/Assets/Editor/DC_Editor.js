
 var CameraDescription : String;
 var myLabelStyle : GUIStyle;
 
@CustomEditor (DynamiCamera)
@CanEditMultipleObjects()

class DC_Editor extends Editor {

	function OnEnable () {
	myLabelStyle = new GUIStyle();
	}
	
    function OnInspectorGUI () {
     
     target.cameraType = EditorGUILayout.EnumPopup("Camera Type ",target.cameraType);
     
     if(target.cameraType.ToString() == "Fixed"){
     
     target.FixedRotaton = EditorGUILayout.Toggle("Fixed Rotation",target.FixedRotaton);
     
     }else if(target.cameraType.ToString() == "PathCamera"){
    
     target.PathStart = EditorGUILayout.Vector3Field("Path Start",target.PathStart);
     target.PathEnd = EditorGUILayout.Vector3Field("Path End",target.PathEnd);
     
	 target.FixedRotaton = EditorGUILayout.Toggle("Fixed Rotation",target.FixedRotaton);
	 
	 target.Automatic = EditorGUILayout.Toggle("Fixed Movement",target.Automatic);

	 if(target.Automatic){
	 target.FixedSpeed = EditorGUILayout.IntField("Travel Speed",target.FixedSpeed);
	 
	 }
	 
     }else if(target.cameraType.ToString() == "FreeCamera"){

	target.TravelAreaCentre = EditorGUILayout.Vector3Field("Travel Area Centre",target.TravelAreaCentre);
	target.TravelAreaScale = EditorGUILayout.Vector3Field("Travel Area Scale",target.TravelAreaScale);

	target.RotationTypes = EditorGUILayout.EnumPopup("Rotation",target.RotationTypes);
	
	if(target.RotationTypes.ToString() == "Spinning")
	target.RotateSpeed = EditorGUILayout.IntField("Rotate Speed",target.RotateSpeed);
	
	}
     
     //Get Camera Description
     if(target.cameraType.ToString() == "Fixed")
     if(target.FixedRotaton)
     CameraDescription = "This Camera will remain in a fixed position and rotation.";
     else
     CameraDescription = "This Camera will remain in a fixed position, but will look at objects of interest.";
     
     if(target.cameraType.ToString() == "Spectator")
     CameraDescription = "This Camera will rotate around the target. Please ensure you add a 'Kart_Camera' script to this camera.";
     
     if(target.cameraType.ToString() == "PathCamera"){
     if(target.Automatic){
     if(target.FixedRotaton == false)
     CameraDescription = "This Camera will travel along it's path when activated at a fixed speed, but will look at objects of interest.";
     else
     CameraDescription = "This Camera will travel along it's path when activated at a fixed speed with a fixed rotation.";
     }else
     if(target.FixedRotaton == false)
     CameraDescription = "This Camera will follow objects of interest along it's path, and will look at objects of interest.";
     else
     CameraDescription = "This Camera will follow objects of interest along it's path with a fixed rotation.";
     }
     
     if(target.cameraType.ToString() == "FreeCamera"){
     
     if(target.RotationTypes.ToString() == "Fixed"){
	 CameraDescription = "This Camera will move freely in the travel area with a fixed rotation.";
	 }else if(target.RotationTypes.ToString() == "Spinning"){
	 CameraDescription = "This Camera will move freely in the travel area, but will rotate vertically at the specified speed.";
	 }else if(target.RotationTypes.ToString() == "Follow"){
	 CameraDescription = "This Camera will move freely in the travel area, and will look at objects of interest.";
     }
     
     
    }
     
     myLabelStyle.wordWrap = true;
     
     GUILayout.Label("");
     GUILayout.Label("Camera Description:");
     GUILayout.Label(CameraDescription,myLabelStyle);
	 
	 if (GUI.changed)
     EditorUtility.SetDirty (target);
            	
       }
       }
       


