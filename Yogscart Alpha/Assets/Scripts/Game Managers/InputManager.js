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

var returnVal : float = Input.GetAxis(inputName + axis);

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

}
