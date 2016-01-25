#pragma strict

//The Slightly Efficent Kart Finding System 
	
private var kartCount : int = 0;
var collisions : boolean[,];

var lastKartCount : int;
var lastOtherCount : int;

function Update () 
{
	var otherKarts = GameObject.FindObjectsOfType(kartScript);
	var otherThings = GameObject.FindObjectsOfType(kartCollider);
	
	if(kartCount != otherKarts.length)
	{
		if(otherKarts != null)
			kartCount = otherKarts.length;
		else
			kartCount = 0;
				
	}
	
	if(lastKartCount != kartCount || lastOtherCount != otherThings.Length)
	{
		lastKartCount = kartCount;
		lastOtherCount = otherThings.Length;
		collisions = new boolean[kartCount,kartCount + otherThings.Length];
	}
	
	for(var i : int = 0; i < kartCount; i++)//For every kart on Screen
	{
	
		for(var j : int = i + 1; j < kartCount; j++)// Checks against every kart after the current kart
		{
			var compareVect = otherKarts[j].transform.position - otherKarts[i].transform.position;
			
			if(!collisions[i,j] && compareVect.magnitude < 2f)
			{
				collisions[i,j] = true;
				otherKarts[i].KartCollision(otherKarts[j].transform);
				otherKarts[j].KartCollision(otherKarts[i].transform);
			}
			else if(collisions[i,j] && compareVect.magnitude >= 2f)
			{
				collisions[i,j] = false;
				otherKarts[i].CancelCollision();
				otherKarts[j].CancelCollision();
			}					
		}
	
		
		//Check for SpinoutObject
		for(j = 0; j < otherThings.Length; j++)// Checks against every kart after the current kart
		{
	
			compareVect = otherThings[j].transform.position - otherKarts[i].transform.position;

			
			if(!collisions[i,kartCount + j] && compareVect.magnitude < 2f)
			{
				collisions[i,kartCount + j] = true;
				otherKarts[i].KartCollision(otherThings[j].transform);
				otherKarts[i].SpinOut();
				Debug.Log("Spin Out");
			}
			else if(collisions[i,kartCount + j] && compareVect.magnitude >= 2f)
			{
				collisions[i,kartCount + j] = false;
				otherKarts[i].CancelCollision();
				Debug.Log("Stop spin Out");
			}		
		}
		
	}
}