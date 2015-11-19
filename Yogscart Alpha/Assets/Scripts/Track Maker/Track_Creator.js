#pragma strict
@script RequireComponent(MeshFilter)
@script RequireComponent(MeshRenderer)
@script RequireComponent(MeshCollider)

var viewCam : Camera;

private var points : Array; //Holds either a Vector3 or a Bezier Curve
private var allPoints : Vector3[];//Holds just Vector3s

private var clickLock : boolean = false;
private var bezierStarted : boolean;

private var lastPoint : Vector3;

private var creatingMesh : boolean;

function Start()
{
	points = new Array();
}

function OnGUI () 
{
	
	if(!creatingMesh)
	{
		if(GUI.Button(Rect(10,10,100,50),"Build Track"))
		{
			GetComponent.<MeshFilter>().mesh.Clear();
			StartCoroutine("GenerateTrackMesh",10);
		}
		if(GUI.Button(Rect(120,10,100,50),"Clear Track"))
		{
			points = new Array();
		}
		if(GUI.Button(Rect(230,10,100,50),"Undo"))
		{
			if(points != null && points.length >= 1)
			{
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
	}
	
}

function Update () 
{
	//Inputs
	if(!creatingMesh && !clickLock && Input.mousePosition.y < (Screen.height - 60))
	{
		if(Input.GetMouseButton(0)) //Left Mouse Button
		{
			var clickPos = viewCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,viewCam.transform.position.y));
			
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
			var rightClickPos = viewCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,viewCam.transform.position.y));
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

	//Draw Debug Lines
	if(points != null && points.length >= 2)
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
			
				if(i < allPoints.length-1)
				{
					var leftVert : Vector3 = allPoints[i] + Vector3.Cross((allPoints[i+1]-allPoints[i]).normalized,transform.up) * -roadWidth;
					var rightvert : Vector3 = allPoints[i] + Vector3.Cross((allPoints[i+1]-allPoints[i]).normalized,transform.up) * roadWidth;
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