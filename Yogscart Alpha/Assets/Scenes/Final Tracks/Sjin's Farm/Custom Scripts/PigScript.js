#pragma strict
@script ExecuteInEditMode()

var points : Vector3[];

var currentPoint : int = 0;
var grazing : boolean = false;

var pigSpeed : float = 5f;

function Update ()
{

	if(points.Length >= 2)
	{
		for(var i : int = 0; i < points.Length; i++)
		{
			Debug.DrawLine(points[i],points[(i+1)%points.Length],Color.red);
		}
	}
	
	if (Application.isPlaying)
	{
		if(!grazing)
		{
			var ani : Animator = transform.GetComponent(Animator);
			
			if(Vector3.Distance(transform.position,points[currentPoint]) > 3f)
			{
				var nVelo : Vector3 = points[currentPoint] - transform.position;				
				nVelo = nVelo.normalized * pigSpeed;
				
				var lastPoint : int = currentPoint-1;
				if(lastPoint < 0)
					lastPoint = points.Length - 1;
					
				var lookDirection : Vector3 = points[currentPoint] - points[lastPoint];
				
				GetComponent.<Rigidbody>().velocity = Vector3(nVelo.x,GetComponent.<Rigidbody>().velocity.y,nVelo.z);		
				ani.SetBool("moving",true);
				
				transform.rotation = Quaternion.Lerp(transform.rotation,
				Quaternion.LookRotation(lookDirection) * Quaternion.Euler(0,90,0),Time.deltaTime*2f);
			}
			else
			{
				currentPoint += 1;
				
				if(currentPoint >= points.Length)
					currentPoint = 0;
				
				var grazeQuestion : int = Random.Range(0,10);
				if(grazeQuestion >= 5)
				{
					Graze();
				}
				
				ani.SetBool("moving",false);
			}
		}
	}
}

function Graze()
{
	grazing = true;
	
	yield WaitForSeconds(10f);
	
	grazing = false;
	
}