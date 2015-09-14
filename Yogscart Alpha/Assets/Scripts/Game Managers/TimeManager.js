#pragma strict

static function TimerToString(time : float)
{
	var returnString : String = "";
	var timeInt : int = time;
	var milliSeconds : float = (time - timeInt) * 1000f;
	
	returnString = (timeInt/60).ToString("00") + ":" + (timeInt%60).ToString("00") + ":" + milliSeconds.ToString("000");

	return returnString;

}