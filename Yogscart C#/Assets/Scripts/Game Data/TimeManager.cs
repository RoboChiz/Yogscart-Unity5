using UnityEngine;
using System.Collections;

public class TimeManager
{
    public static string ToString(float time)
    {
        int timeInt = (int)time;
        float milliSeconds = (time - (float)timeInt) * 1000f;

        return(timeInt / 60).ToString("00") + ":" + (timeInt % 60).ToString("00") + ":" + milliSeconds.ToString("000");
    }

    public static float Parse(string time)
    {
        string[] splitup = time.Split(":"[0]);

        return ((float.Parse(splitup[0]) * 60f) + (float.Parse(splitup[1])) + (float.Parse(splitup[2]) / 1000f));
    }

}
