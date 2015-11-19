#pragma strict

class Bezier
{
	private var basePoints : Array; //First and last points are control points
	private var finalPoints : Array;

	function Bezier()
	{
		basePoints = new Array();
	}

	function AddBasePoint(point : Vector3)
	{
		basePoints.Push(point);
	}
	
	function GetBasePoints() : Array
	{
		return basePoints;
	}
	function PopBasePoints()
	{
		basePoints.Pop();
		finalPoints = new Array();
	}
	function GetFinalPoints() : Array
	{
		return finalPoints;
	}

	function CalculatePoints()
	{
		if(basePoints != null && basePoints.length > 2)
		{	
			var segments : int = ((basePoints.length/3f)* 10f);
		
			finalPoints = new Array();
			finalPoints.Push(basePoints[0]); //Add Control Point
			
			for(var i : int = 1; i < segments; i++)
			{
				var sVal = (1f/segments)*i;
				finalPoints.Push(CalculatePoint(basePoints,sVal));	
			}
			
			finalPoints.Push(basePoints[basePoints.length - 1]);
			
		}
	}
	
	function CalculatePoint(points : Array, amount : float) : Vector3
	{
		var returnArray = new Array();
		
		for(var i : int = 0; i < points.length; i++)
		{
			if(i + 1 < points.length)
			{
				var nPoint = Vector3.Lerp(points[i],points[i+1],amount);
				returnArray.Push(nPoint);
			}
		}
		
		if(returnArray.length != 1)
			nPoint = CalculatePoint(returnArray,amount);

		return nPoint;	
		
	}
}