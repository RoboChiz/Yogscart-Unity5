#pragma strict

var allowedToChange : boolean;
var c : InputController[]; //Holds controllers connected

var keyboardPlayer : int;
var mouseLock : boolean;

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

function RemoveOtherControllers()
{
	var holder = c[0];
	
	c = new InputController[1];
	c[0] = holder;
	
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
