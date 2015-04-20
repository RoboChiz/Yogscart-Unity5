#pragma strict

var allowedToChange : boolean;
var c : InputController[]; //Holds controllers connected

public class InputController
{

var inputName : String;
var buttonLock : boolean; //Used on GetMenuInputFunction.


function InputController (inputString : String)
{
	inputName = inputString;
	buttonLock = false;
}

function GetMenuInput(axis : String)
{

	var returnVal : float = Input.GetAxisRaw(inputName + axis);

	if(returnVal == 0)
		buttonLock = false;

	if(!buttonLock && returnVal != 0)
	{
		buttonLock = true;
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
