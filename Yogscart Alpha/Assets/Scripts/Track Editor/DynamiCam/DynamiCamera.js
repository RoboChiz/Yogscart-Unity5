@script RequireComponent(Camera)
@script ExecuteInEditMode()

enum Type {Fixed, PathCamera}
 
public class DynamiCamera extends MonoBehaviour {

public var cameraType : Type = Type.Fixed;

//Fixed Camera Variables
public var FixedRotaton : boolean = false;

//Path Camera Variables
public var PathStart : Vector3;
public var PathEnd : Vector3;

public var Automatic : boolean = false;

public var FixedSpeed : float = 3;

//Free Camera Variables

public var TravelAreaCentre : Vector3;
public var TravelAreaScale : Vector3 = Vector3(1,1,1);

public var rotationClamped : boolean;

public var rotationClampXmin : float;
public var rotationClampXmax : float;

public var rotationClampYmin : float;
public var rotationClampYmax : float;

public var rotationClampZmin : float;
public var rotationClampZmax : float;

public var RotateSpeed : float = 3;

var interested : boolean;
var target : Transform;

}

function FixedUpdate () 
{
	if(target != null)
	{
		if(IsRenderedFrom(target.GetComponentInChildren(Renderer),GetComponent.<Camera>() ) )
			interested = true;
		else
			interested = false;
	}
}

function LateUpdate()
{

	if(target != null && !FixedRotaton)
		transform.LookAt(target.position);
		
	if(rotationClamped)
	{
		var x : float = Mathf.Clamp(transform.eulerAngles.x,rotationClampXmin,rotationClampXmax);
		var y : float = Mathf.Clamp(transform.eulerAngles.y,rotationClampYmin,rotationClampYmax);
		var z : float = Mathf.Clamp(transform.eulerAngles.z,rotationClampZmin,rotationClampZmax);
		
		transform.eulerAngles = Vector3(x,y,z);
	}

}

function IsRenderedFrom(renderer : Renderer, camera : Camera) : boolean
{
    var planes = GeometryUtility.CalculateFrustumPlanes(camera);
	return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
}

function Update () {

	if(transform.name != "DynamiCamera"){
		transform.name = "DynamiCamera";
	}
	
}