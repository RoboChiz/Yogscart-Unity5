using UnityEngine;
using System.Collections;

public class MathHelper
{
    public static int NumClamp(int value, int min, int max)
    {
        int diff = (max - min);

        while (value >= max)
            value -= diff;

        while (value < min)
            value += diff;

        return value;
    }

    public static float NumClamp(float value, float min, float max)
    {
        float diff = (max - min);

        while (value >= max)
            value -= diff;

        while (value < min)
            value += diff;

        return value;
    }

    /// <summary>
    /// Returns the angle between two vectors in a clockwise motion
    /// </summary>
    /// <param name="toDirection"></param>
    /// <param name="fromDirection"></param>
    /// <returns></returns>
    public static float Angle(Vector3 toDirection, Vector3 fromDirection)
    {
        float toReturn = Vector3.Angle(toDirection, fromDirection);

        Vector3 rightDir = Quaternion.AngleAxis(90, Vector3.up) * toDirection;
        if (Vector3.Angle(fromDirection, rightDir) > 90)
            toReturn = -toReturn;

        return toReturn;
    }

    public static int Sign(float value)
    {
        if (value == 0f)
            return 0;
        else
            return (int)Mathf.Sign(value);
    }
}