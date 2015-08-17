#pragma strict
@script ExecuteInEditMode()

private var td : TrackData;

function Update () {

if(GameObject.Find("Track Manager") != null && GameObject.Find("Track Manager").GetComponent(TrackData) != null){
	td = GameObject.Find("Track Manager").GetComponent(TrackData);

//Draw Spawn Area
	if(td.spawnPoint != null)
	{
	
		var rot : Quaternion;
		rot = td.spawnPoint.transform.rotation;
		
		var centre : Vector3;
		centre = td.spawnPoint.transform.position;
		
		var pos : Vector3;
		pos = centre + (rot*Vector3.forward*-6.75f);

		var pos1 : Vector3;
		pos1 = centre + (rot*Vector3.forward*6.75f);
		
		var pos2 : Vector3;
		pos2 = pos1 + (rot*Vector3.right * 39f);
		var pos3 : Vector3;
		pos3 = pos + (rot*Vector3.right * 39f);	
		
		Debug.DrawLine(pos,pos1,Color.blue);
		Debug.DrawLine(pos1,pos2,Color.blue);
		Debug.DrawLine(pos2,pos3,Color.blue);
		Debug.DrawLine(pos3,pos,Color.blue);
		
	}

//Draw Main Lap Point
 try {
 
	/*var rot : Quaternion;
	rot = td.PositionPoints[0].transform.rotation;
	
	var centre : Vector3;
	centre = td.PositionPoints[0].transform.position;
	
	var pos : Vector3;
	pos = centre + (rot*Vector3.forward*-6.75f);

	var pos1 : Vector3;
	pos1 = centre + (rot*Vector3.forward*-6.75f);
	
	var pos2 : Vector3;
	pos2 = pos1 + (rot*Vector3.right * 39f);
	var pos3 : Vector3;
	pos3 = pos + (rot*Vector3.right * 39f);	
	
	Debug.DrawLine(pos,pos1,Color.blue);
	Debug.DrawLine(pos1,pos2,Color.blue);
	Debug.DrawLine(pos2,pos3,Color.blue);
	Debug.DrawLine(pos3,pos,Color.blue);
	
	Debug.DrawLine((pos+pos1)/2f,(pos2+pos3)/2f,Color.red);*/

//Render Track Lines
	
	if(td.PositionPoints != null){
		if(td.PositionPoints.Length >=2){
		
			var lapCount : int = 0;
			
			var ppCount : int;
			var copy = new Array();
		
			var adjusterFloat = td.PositionPoints[0].GetComponent(PointHandler).roadWidth;
		
			var lastAdjustOne : Vector3 = Vector3.Cross((td.PositionPoints[0].position-td.PositionPoints[1].position).normalized,transform.up) * -adjusterFloat;
			var lastAdjustTwo : Vector3 = Vector3.Cross((td.PositionPoints[0].position-td.PositionPoints[1].position).normalized,transform.up) * adjusterFloat;
			
			for(var i : int = 0; i < td.PositionPoints.Length; i++){
			
				if(i+1 < td.PositionPoints.Length)
				{
					Debug.DrawLine(td.PositionPoints[i].position,td.PositionPoints[i+1].position,Color.red);
										
					//Draw Road Width
					adjusterFloat = td.PositionPoints[i+1].GetComponent(PointHandler).roadWidth;
					var adjustOne = Vector3.Cross((td.PositionPoints[i+1].position-td.PositionPoints[i].position).normalized,transform.up) * adjusterFloat;
					var adjustTwo = Vector3.Cross((td.PositionPoints[i+1].position-td.PositionPoints[i].position).normalized,transform.up) * -adjusterFloat;
					
					Debug.DrawLine(td.PositionPoints[i].position + lastAdjustOne,td.PositionPoints[i+1].position + adjustOne,Color.cyan);
					Debug.DrawLine(td.PositionPoints[i].position + lastAdjustTwo,td.PositionPoints[i+1].position + adjustTwo,Color.cyan);
					
					
					lastAdjustOne = adjustOne;
					lastAdjustTwo = adjustTwo;
				}
				
				//Draw LapPoint
				if(td.PositionPoints[i].GetComponent(PointHandler) != null && td.PositionPoints[i].GetComponent(PointHandler).style == Point.Lap)
				{
					var Position1 : Vector3 = td.PositionPoints[i].position;
					var Position2 : Vector3;
					
					if(i+1 < td.PositionPoints.Length)
					{
						Position2 = td.PositionPoints[i+1].position;
					}
					else if(i-1 >= 0)
					{
				 		Position2 = td.PositionPoints[i-1].position;
			 		}
				 	else
				 	{
				 		continue;
				 	}
				 
					var nrot : Quaternion = Quaternion.Euler(0,90,0);
					var dir : Vector3 = (Position1-Position2);
					
					dir.y = 0;
					dir.Normalize();
				 
			 		Debug.DrawRay(Position1,nrot*dir * 9f,Color.yellow);
			 		Debug.DrawRay(Position1,nrot*dir * -9f,Color.yellow);
			 		
			 		lapCount += 1;
			 		
			 		copy.Push(ppCount);
			 		ppCount = 0;
			 
				}
				else
				{
					if(td.PositionPoints[i].GetComponent(PointHandler).style == Point.Position)
						ppCount += 1;
				}
			
			}
			
			td.pointsNeededToLap = copy;
			
			if(lapCount > 1)
			{
				td.LoopedTrack = false;
				td.Laps = lapCount;
			}
			else
				td.LoopedTrack = true;
				
			if(td.LoopedTrack)
			{
			
				var last : int = td.PositionPoints.Length-1;
				
				Debug.DrawLine(td.PositionPoints[0].position,td.PositionPoints[last].position,Color.red);
				
				//Draw Road Width
				adjusterFloat = td.PositionPoints[0].GetComponent(PointHandler).roadWidth;
				adjustOne = Vector3.Cross((td.PositionPoints[last].position-td.PositionPoints[0].position).normalized,transform.up) * adjusterFloat;
				adjustTwo = Vector3.Cross((td.PositionPoints[last].position-td.PositionPoints[0].position).normalized,transform.up) * -adjusterFloat;
				
				Debug.DrawLine(td.PositionPoints[last].position + lastAdjustOne,td.PositionPoints[0].position + adjustTwo,Color.cyan);
				Debug.DrawLine(td.PositionPoints[last].position + lastAdjustTwo,td.PositionPoints[0].position + adjustOne,Color.cyan);
				
				
			}
			
			for(i = 0; i < td.ShortCuts.Length; i++){
				
				var sc = td.ShortCuts[i];
				if(sc.PositionPoints != null && sc.PositionPoints.Length > 0)
				{
					if(sc.startPoint != null)
						Debug.DrawLine(sc.startPoint.position,sc.PositionPoints[0].position,Color.red);
					if(sc.endPoint != null)
						Debug.DrawLine(sc.endPoint.position,sc.PositionPoints[sc.PositionPoints.Length-1].position,Color.red);
					
					for(var j : int = 0; j < sc.PositionPoints.Length; j++)
					{
						if(j+1 < sc.PositionPoints.Length)
						{
							Debug.DrawLine(sc.PositionPoints[j].position,sc.PositionPoints[j+1].position,Color.red);

						}
					}
					
				}		
			}
			
		}
	}
}catch(err){

}
}
}

