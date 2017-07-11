using UnityEngine;
using System.Collections;

public class MathHelper
{
    public static int NumClamp(int value, int min, int max)
    {
        if (min == max)
            return value;

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

    public static bool InRange(float value, float min, float max)
    {
        if (value >= min && value <= max)
            return true;

        return false;
    }

    public static Vector3 ZeroYPos(Vector3 position)
    {
        return Vector3.Scale(position, new Vector3(1f, 0f, 1f));
    }

    public static bool HaveTheSameSign(float first, float second)
    {
        return (Mathf.Sign(first) == Mathf.Sign(second));
    }
}
