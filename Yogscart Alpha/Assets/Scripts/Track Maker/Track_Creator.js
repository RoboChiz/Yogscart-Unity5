#pragma strict
@script RequireComponent(MeshFilter)
@script RequireComponent(MeshRenderer)
@script RequireComponent(MeshCollider)

var viewCam : Camera;

private var points : Array; //Holds either a Vector3 or a Bezier Curve
private var allPoints : Vector3[];//Holds just Vector3s

private var clickLock : boolean = false;
private var bezierStarted : boolean;
private var trackHeight : float = 0f;

private var lastPoint : Vector3;

private var creatingMesh : boolean;
private var loopedTrack : boolean;
private var testing : boolean;

var testKart : Transform;
private var ingameKart : Transform;

var point : Texture2D;

function Start()
{
	points = new Array();
}

function OnGUI () 
{

	GUI.Label(Rect(0,0,100,50),"Track Height:" + trackHeight.ToString());

	if(!creatingMesh)
	{
		if(!testing)
		{
			if(GUI.Button(Rect(10,10,100,50),"Build Track"))
			{
				GetComponent.<MeshFilter>().mesh.Clear();
				StartCoroutine("GenerateTrackMesh",10);
			}
			if(GUI.Button(Rect(120,10,100,50),"Clear Track"))
			{
				points = new Array();
				loopedTrack = false;
				bezierStarted = false;
			}
			if(GUI.Button(Rect(230,10,100,50),"Undo"))
			{
				if(points != null && points.length >= 1)
				{
				
					if(loopedTrack)
						loopedTrack = false;
				
					if(points[points.length-1].GetType() == typeof(Vector3))
					{
						points.Pop();
						
					}
					else
					{
						var nBezier : Bezier = points[points.length-1];
						var nBezierLength = nBezier.GetBasePoints().length;
						
						if(nBezierLength <= 2)
						{
							points.Pop();
							bezierStarted = false;
						}
						else
						{
							nBezier.PopBasePoints();
							bezierStarted = true;
						}
					}
				}
			}
			
			if(GUI.Button(Rect(340,10,100,50),"Play"))
			{
				if(allPoints != null && allPoints.Length >= 2)
				{
					testing = true;
					ingameKart = Instantiate(testKart,Vector3.Lerp(allPoints[0],allPoints[1],0.5f) + (Vector3.up*2f),Quaternion.LookRotation(allPoints[1]-allPoints[0]) * Quaternion.Euler(0,90,0));
				}
			}
		}
		else
		{
			if(GUI.Button(Rect(10,10,100,50),"Back"))
			{
				Destroy(ingameKart.gameObject);
				testing = false;
			}
		}
	}
	
	//Draw Debug Lines
	if(!testing && points != null && points.length >= 2)
	{					
		for(var i : int = 0; i < points.length; i++)
		{
			switch(points[i].GetType())
			{
				case(typeof(Vector3)):
					if(i == 0)
						lastPoint = points[0];
					else
					{
						var nPoint : Vector3 = viewCam.WorldToScreenPoint(points[i]);
						var nLast : Vector3 = viewCam.WorldToScreenPoint(lastPoint);
						
						nPoint.y = Screen.height - nPoint.y;
						nLast.y = Screen.height - nLast.y;
						DrawLine(new Vector2(nLast.x,nLast.y),new Vector2(nPoint.x,nPoint.y),2,Color.red);
						
						Debug.DrawLine(lastPoint,points[i],Color.red);
						lastPoint = points[i];
					}
				break;
				case(typeof(Bezier)):
					var bezier : Bezier = points[i];			
					var bezierPoints = bezier.GetFinalPoints();
					var bezierBasePoints = bezier.GetBasePoints();
					
					if(bezierPoints != null)
					{
						for(var j : int = 1; j < bezierPoints.length; j++)
						{
							Debug.DrawLine(lastPoint,bezierPoints[j],Color.cyan);
							
							nPoint = viewCam.WorldToScreenPoint(bezierPoints[j]);
							nLast = viewCam.WorldToScreenPoint(lastPoint);
							
							nPoint.y = Screen.height - nPoint.y;
							nLast.y = Screen.height - nLast.y;
							DrawLine(new Vector2(nLast.x,nLast.y),new Vector2(nPoint.x,nPoint.y),2,Color.cyan);
							
							lastPoint = bezierPoints[j];
						}	
					}
					
					if(bezierBasePoints != null)
					{
						var lastControlPoint : Vector3;
						for(var k : int = 0; k < bezierBasePoints.length; k++)
						{
							if(k == 0)
								lastControlPoint = bezierBasePoints[0];
							else
							{
								nPoint = viewCam.WorldToScreenPoint(bezierBasePoints[k]);
								nLast = viewCam.WorldToScreenPoint(lastControlPoint);
								
								nPoint.y = Screen.height - nPoint.y;
								nLast.y = Screen.height - nLast.y;
								DrawLine(new Vector2(nLast.x,nLast.y),new Vector2(nPoint.x,nPoint.y),2,Color.green);
								
								Debug.DrawLine(lastControlPoint,bezierBasePoints[k],Color.green);
								lastControlPoint = bezierBasePoints[k];
							}
						}	
					}
					
				break; 
			}
		}
		
	}
}

function Update () 
{
	//Inputs
	trackHeight = Mathf.Clamp(trackHeight,-100,100);
	if(!testing && !creatingMesh)
	{
		if(Input.GetKeyDown(KeyCode.PageUp))
			trackHeight += 1;
		
		if(Input.GetKeyDown(KeyCode.PageDown))
			trackHeight -= 1;
		
	}
	
	if(!testing && !creatingMesh && !loopedTrack && !clickLock && Input.mousePosition.y < (Screen.height - 60))
	{
		if(Input.GetMouseButton(0)) //Left Mouse Button
		{
			var clickPos = viewCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,500 - trackHeight));
			
			if(points.length > 1 && Vector3.Distance(clickPos,points[0]) < 5)
			{
				clickPos = points[0];
				loopedTrack = true;
			}
			
			if(bezierStarted)
			{
				var lastBezier : Bezier = points[points.length - 1];
				lastBezier.AddBasePoint(clickPos);
				lastBezier.CalculatePoints();
				bezierStarted = false;
			}
			else
			{
				points.Push(clickPos);
			}
			
			lastPoint = clickPos;
			Debug.Log("Added " + clickPos + "! The array is now " + points.length.ToString() + " big!");
			clickLock = true;
			
		}
		
		if(Input.GetMouseButton(1))
		{
			var rightClickPos = viewCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,500 - trackHeight));
			var nBezier : Bezier;
			
			if(!bezierStarted)
			{
				bezierStarted = true;
				
				if(points.length == 0) //Error checking incase no Points exsist
				{
					points.Push(rightClickPos);
					lastPoint = rightClickPos;
				}
				
				nBezier = new Bezier();
				nBezier.AddBasePoint(lastPoint);
				points.Push(nBezier);
			}
			
			if(nBezier == null)
				nBezier = points[points.length-1];
			
			nBezier.AddBasePoint(rightClickPos);
			
			clickLock = true;		
		}
	}
	
	if(!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
		clickLock = false;
		
}

function GetAllPoints()
{
	var tempArray = new Array();
	
	if(points != null && points.length >= 2)
	{					
		for(var i : int = 0; i < points.length; i++)
		{
			switch(points[i].GetType())
			{
				case(typeof(Vector3)):		
					tempArray.Push(points[i]);
					Debug.Log("Added " + points[i] + "tempArray:" + tempArray.length);
				break;
				case(typeof(Bezier)):
					var bezier : Bezier = points[i];			
					var bezierPoints = bezier.GetFinalPoints();
					var bezierBasePoints = bezier.GetBasePoints();
					
					if(bezierPoints != null)
					{
						for(var j : int = 1; j < bezierPoints.length; j++)
						{
							tempArray.Push(bezierPoints[j]);
							Debug.Log("Added Bezier " + bezierPoints[j] + "tempArray:" + tempArray.length);
						}	
					}				
				break; 
			}		
		}
		
	}
	
	allPoints = new Vector3[tempArray.length];
	
	for(i = 0; i < tempArray.length; i++)
	{
		var nValue : Vector3 = tempArray[i];
		allPoints[i] = nValue;
	}
	
}

function GenerateTrackMesh(roadWidth : float)
{
	if(points != null && points.length >= 2)
	{
		creatingMesh = true;
		var returnMesh = new Mesh();
		
		GetAllPoints();
		
		var verts = new Vector3[allPoints.length*2];
		var tris = new int[(allPoints.length-1)*6];
		var uvs = new Vector2[allPoints.length*2];
		var normals = new Vector3[allPoints.length*2];
		
		var currentUVY : float = 0;
		
		var triCount : float = 3;
		var vertCount : float = 0f;
		
		var startTime = Time.realtimeSinceStartup;
		
		if(allPoints != null && allPoints.length >= 2)
		{
		
			for(var i : int = 0; i < allPoints.length; i++)
			{
				
				if(Time.realtimeSinceStartup - startTime > 0.0167)
				{
					startTime = Time.realtimeSinceStartup;
					yield;
				}
			
				if(!loopedTrack || i != allPoints.length-1)
				{
					if(i == allPoints.length-2 && loopedTrack)
					{
						var leftVert : Vector3 = verts[0];
						var rightvert : Vector3 = verts[1];
						
						Debug.Log("leftVert:" + leftVert + " & rightvert:" + rightvert);
					}
					else if(i < allPoints.length-1)
					{
						 leftVert = allPoints[i] + Vector3.Cross((allPoints[i+1]-allPoints[i]).normalized,transform.up) * -roadWidth;
						 rightvert = allPoints[i] + Vector3.Cross((allPoints[i+1]-allPoints[i]).normalized,transform.up) * roadWidth;
					}
					else
					{
						leftVert = allPoints[i] + Vector3.Cross((allPoints[i]-allPoints[i-1]).normalized,transform.up) * -roadWidth;
						rightvert = allPoints[i] + Vector3.Cross((allPoints[i]-allPoints[i-1]).normalized,transform.up) * roadWidth;
					}
						verts[vertCount] = leftVert;
						uvs[vertCount] = new Vector2(0,currentUVY);
						normals[vertCount] = Vector3.up;
						vertCount++;
						
						verts[vertCount] = rightvert;
						uvs[vertCount] = new Vector2(1,currentUVY);
						normals[vertCount] = Vector3.up;
						vertCount++;	
					
					if(i < allPoints.length-1)
						currentUVY += Vector3.Distance(allPoints[i+1],allPoints[i]);
					
					if(i > 0) //Add tris from last point
					{
						tris[((i-1)*6)] = triCount;
						tris[((i-1)*6) + 1] = triCount-1;
						tris[((i-1)*6) + 2] = triCount-2;
						
						tris[((i-1)*6) + 3] = triCount-3;
						tris[((i-1)*6) + 4] = triCount-2;
						tris[((i-1)*6) + 5] = triCount-1;	
						
						triCount += 2;
					}
				}
			}
			
			returnMesh.vertices = verts;
			returnMesh.triangles = tris;
			returnMesh.uv = uvs;
			returnMesh.normals = normals;
			
		}
		
		creatingMesh = false;
		
		GetComponent.<MeshFilter>().mesh = returnMesh;
		GetComponent.<MeshCollider>().sharedMesh = returnMesh;
		
	}
}

private function DrawLine(start : Vector2,end : Vector2,width : int,c : Color)
{
	GUI.color = c;
	
    var d : Vector2 = end - start;
    var a : float = Mathf.Rad2Deg * Mathf.Atan(d.y / d.x);
    if (d.x < 0)
        a += 180;

    var width2 : int = Mathf.Ceil(width / 2f);

    GUIUtility.RotateAroundPivot(a, start);
    GUI.DrawTexture(new Rect(start.x, start.y - width2, d.magnitude, width), point);
    GUIUtility.RotateAroundPivot(-a, start);
}