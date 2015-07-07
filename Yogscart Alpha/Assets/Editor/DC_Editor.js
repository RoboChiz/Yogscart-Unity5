
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
     target.rotationClamped = EditorGUILayout.Toggle("Clamp Rotation",target.rotationClamped);
     
     if(target.rotationClamped)
     {
     	var min = EditorGUILayout.Vector3Field("Minimum",Vector3(target.rotationClampXmin,target.rotationClampYmin,target.rotationClampZmin));
     	target.rotationClampXmin = min.x;
     	target.rotationClampYmin = min.y;
     	target.rotationClampZmin = min.z;
     	
     	var max = EditorGUILayout.Vector3Field("Maximum",Vector3(target.rotationClampXmax,target.rotationClampYmax,target.rotationClampZmax));
     	target.rotationClampXmax = max.x;
     	target.rotationClampYmax = max.y;
     	target.rotationClampZmax = max.z;
     	
     }
     
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
	 
     }
     
     //Get Camera Description
     if(target.cameraType.ToString() == "Fixed")
     if(target.FixedRotaton)
     CameraDescription = "This Camera will remain in a fixed position and rotation.";
     else
     CameraDescription = "This Camera will remain in a fixed position, but will look at objects of interest.";
 
     
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
     
     
     myLabelStyle.wordWrap = true;
     
     GUILayout.Label("");
     GUILayout.Label("Camera Description:");
     GUILayout.Label(CameraDescription,myLabelStyle);
	 
	 if (GUI.changed)
     EditorUtility.SetDirty (target);
            	
       }
       }
       


