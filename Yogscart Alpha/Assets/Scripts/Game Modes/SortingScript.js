#pragma strict
import System.Collections.Generic; //Cause lazy
import System.Linq; //Cause lazy

function CalculatePositions(array : Racer[])
{

	var sortedArray = new Racer[0];
	var finished : int = 0;
	var copy = new Array();

	for(var i : int = 0; i < array.Length; i++)
	{
		if(!array[i].finished)
			copy.Push(array[i]);
		else
			finished += 1;
	}

	sortedArray = copy;

	quickSort(sortedArray,0,sortedArray.Length-1);

	for(i = finished; i < array.Length; i++)
	{
	sortedArray[i-finished].position = i;
}

}

function quickSort(array : Racer[],left : int, right : int)
{
	if(right - left >= 1)
	{

	//Debug.Log("Quick Sorting between " + left + " and " + right);

	var pivot : int = (right + left)/2;
	var leftCheck : int = left;
	var rightCheck : int = right;

	while(leftCheck < rightCheck)
	{
		while(array[leftCheck].TotalDistance > array[pivot].TotalDistance || 
			(array[leftCheck].TotalDistance == array[pivot].TotalDistance && array[leftCheck].NextDistance < array[pivot].NextDistance))
				leftCheck += 1;
				
		while(array[rightCheck].TotalDistance < array[pivot].TotalDistance || 
			(array[rightCheck].TotalDistance == array[pivot].TotalDistance && array[rightCheck].NextDistance > array[pivot].NextDistance))
				rightCheck -= 1;
				
		if(leftCheck < rightCheck)
		{
			//Debug.Log("Swapping " + leftCheck.ToString() + " & " + rightCheck.ToString());
			
			if(leftCheck == pivot)
			{
				pivot = rightCheck;	
				//Debug.Log("pivot has swapped " + pivot.ToString());	
			}
			else if(rightCheck == pivot)
			{
				pivot = leftCheck;
				//Debug.Log("pivot has swapped " + pivot.ToString());	
			}
			
			Swap(array,leftCheck,rightCheck);
			
			if(leftCheck != pivot)
			leftCheck += 1;
			
			if(rightCheck != pivot)
			rightCheck -= 1;

		}


	}

	quickSort(array,left,pivot - 1);
	quickSort(array,pivot + 1,right);

	}

return;
	
}

function Swap(array : Racer[], a : int, b : int)
{

	var holder = array[a];

	array[a] = array[b];
	array[b] = holder;

	return;

}

static function SortRacersPoints(toChangeArray : Racer[])
{

	var array = new List.<Racer>();
	
	for(var j : int = 0; j < toChangeArray.Length; j++)
		array.Add(toChangeArray[j]);

	var sorted : boolean = false;
	var endInt : int = 0;

	while(!sorted)
	{
	
		sorted = true;
					
		for(var i : int = 1; i < array.Count - endInt; i++)
		{
			if(array[i-1].points < array[i].points)
			{
				
				var holder = array[i-1];
				array[i-1] = array[i];
				array[i] = holder;
				sorted = false;
			}
		}
		
		endInt++;
	}
	
	return array;

}


static function CalculatePoints(toChangeArray : List.<DisplayRacer>)
{

	var array = new List.<DisplayRacer>();
	
	for(var j : int = 0; j < toChangeArray.Count; j++)
		array.Add(toChangeArray[j]);

	var sorted : boolean = false;
	var endInt : int = 0;

	while(!sorted)
	{
	
		sorted = true;
					
		for(var i : int = 1; i < array.Count - endInt; i++)
		{
			if(array[i-1].points < array[i].points)
			{
				
				var holder = array[i-1];
				array[i-1] = array[i];
				array[i] = holder;
				sorted = false;
			}
		}
		
		endInt++;
	}
	
	return array;

}
