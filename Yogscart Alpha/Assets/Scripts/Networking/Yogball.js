#pragma strict

function StartGame () {
	Debug.Log("Let's play YogBall");
	EndGame();
}

function EndGame ()
{
	Debug.Log("That's a wrap!");
	
	//Reset any variables in the script which have been changed.
	
	var nm : Network_Manager = transform.GetComponent(Network_Manager);
	nm.EndGame();
}
