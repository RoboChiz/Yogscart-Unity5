#pragma strict

var allowedToChange : boolean;
var c : InputController[]; //Holds controllers connected

var keyboardPlayer : int;
var mouseLock : boolean;

var changingController : boolean;

private var showIcon : boolean[];
private var iconHeights : int[];

function Awake(){
	showIcon = new boolean[4];
	iconHeights = new int[4];
}

public class InputController
{

	var inputName : String;
	var buttonLock : String; //Used on GetMenuInputFunction.

	function InputController (inputString : String)
	{
		inputName = inputString;
		buttonLock = "";
	}

	function GetMenuInput(axis : String)
	{
		
		var returnVal : float = Input.GetAxisRaw(inputName + axis);
		
		if(buttonLock != "" && Input.GetAxisRaw(inputName + buttonLock) == 0)
		{
			buttonLock = "";
		}
		
		if(buttonLock == "" && returnVal != 0)
		{
			buttonLock = axis;
			return returnVal;
		}
		else
		{
			return 0;
		}
	}


	function GetInput(axis : String)
	{
		return Input.GetAxisRaw(inputName + axis);
	}

}

function Update()
{
	//Look for new Controllers
	if(allowedToChange){
		if((c == null || c.Length < 4)){
			if(Input.GetAxis("Key_Submit"))
				AddController("Key_");
		
			if(Input.GetAxis("C1_Submit"))
				AddController("C1_");
		
			if(Input.GetAxis("C2_Submit"))
				AddController("C2_");
		
			if(Input.GetAxis("C3_Submit"))
				AddController("C3_");
		
			if(Input.GetAxis("C4_Submit"))
				AddController("C4_");
		
		}
		
		if(c != null){
			if(Input.GetAxis("Key_Leave"))
				RemoveController("Key_");
			
		
			if(Input.GetAxis("C1_Leave"))
				RemoveController("C1_");
			
			if(Input.GetAxis("C2_Leave"))
				RemoveController("C2_");
			
			if(Input.GetAxis("C3_Leave"))
				RemoveController("C3_");
			
			if(Input.GetAxis("C4_Leave"))
				RemoveController("C4_");
		
		}
	}
}

function OnGUI()
{

	GUI.skin = Resources.Load("GUISkins/Main Menu", GUISkin);
	
	for(var i : int = 0; i < 4; i++){
		
		var idealSize = Screen.height/6f;
		
		if(showIcon[i])
		iconHeights[i] = Mathf.Lerp(iconHeights[i],idealSize,Time.deltaTime * 3f);
		else
		iconHeights[i] = Mathf.Lerp(iconHeights[i],0,Time.deltaTime * 3f);
		
		var iconRect : Rect = Rect(10 + (i*idealSize),Screen.height - iconHeights[i],idealSize,idealSize);
		
		if(c!= null && c.Length > i){
		var Icon : Texture2D;
		if(c[i].inputName == "Key_")
		Icon = Resources.Load("UI Textures/Controls/Keyboard",Texture2D);
		else
		Icon = Resources.Load("UI Textures/Controls/Xbox",Texture2D);
		
		GUI.Box(iconRect,Icon);
		
		}else
		GUI.Box(iconRect,"Player " + (i+1) + " has left!");
		
		}
}

function ShowInput(i : int)
{

showIcon[i] = true;

yield WaitForSeconds(1);

showIcon[i] = false;


}

function AddController(input : String)
{
	
	while(changingController)
		yield;
		
	changingController = true;
	
	var alreadyIn : boolean;
	
	for(var i : int = 0; i < c.Length; i++)
	{
		if(c[i].inputName == input)
		{
			alreadyIn = true;
			break;
		}
	}
	

	if(!alreadyIn)
	{
	
		var copy = new Array();

		if(c != null)
			copy = c;

		var newInput = new InputController(input);
		newInput.buttonLock = "Submit";
		
		copy.Push(newInput);
		c = copy;
		
		ShowInput(i);
	}
	
	changingController = false;
	
}

function RemoveController(input : String)
{

	while(changingController)
			yield;
		
	changingController = true;

	var copy = new Array();
	var toShow : int = -1;
	var changedSomething : boolean;
	
	for(var i : int = 0; i < c.Length; i++)
	{
		if(c[i].inputName != input)
		{
			copy.Push(c[i]);
		}
		else
		{
			toShow = i;
			changedSomething = true;
		}
	}

	c = copy;
	
	if(changedSomething)
		for(i = 0; i < c.Length; i++)
			ShowInput(i);
	
	changingController = false;

}

function RemoveOtherControllers()
{
	var holder = c[0];
	
	c = new InputController[1];
	c[0] = holder;
	
	for(var i : int = 0; i < c.Length; i++)
			ShowInput(i);
	
}

function MouseIntersects(Area : Rect){
	if(Input.mousePosition.x >= Area.x && Input.mousePosition.x <= Area.x + Area.width 
	&&  Screen.height-Input.mousePosition.y >= Area.y &&  Screen.height-Input.mousePosition.y <= Area.y + Area.height)
		return true;
	else
		return false;
}

function GetClick()
{
	if(Input.GetMouseButtonDown(0) && !mouseLock)
	{
		mouseLock = true;
		return true;
	}
	else
	{
		if(Input.GetMouseButtonUp(0))
			mouseLock = false;
		
		return false;
	}
	
}
