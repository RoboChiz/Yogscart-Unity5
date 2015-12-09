#pragma strict

static function TimerToString(time : float)
{
	var returnString : String = "";
	var timeInt : int = time;
	var milliSeconds : float = (time - timeInt) * 1000f;
	
	returnString = (timeInt/60).ToString("00") + ":" + (timeInt%60).ToString("00") + ":" + milliSeconds.ToString("000");

	return returnString;

}

//add string to float function
static function Parse(time : String) : float
{
	var splitUp : String[] = time.Split(":"[0]);
	
	var returnTime : float = (float.Parse(splitUp[0]) * 60f) + (float.Parse(splitUp[1])) + (float.Parse(splitUp[2])/1000f);
	return returnTime;
	
}