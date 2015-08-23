#pragma strict

//The Slightly Efficent Kart Finding System 
	
private var kartCount : int = 0;
private var collisions : boolean[,];

function Update () 
{
	var otherKarts = GameObject.FindObjectsOfType(kartScript);
	
	if(kartCount != otherKarts.length)
	{
		if(otherKarts != null)
			kartCount = otherKarts.length;
		else
			kartCount = 0;
			
		collisions = new boolean[kartCount,kartCount];
	}
	
	if(kartCount > 1)
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
				else if(collisions[i,j])
				{
					collisions[i,j] = false;
					otherKarts[i].CancelCollision();
					otherKarts[j].CancelCollision();
				}
					
				
			}
		}
	
}