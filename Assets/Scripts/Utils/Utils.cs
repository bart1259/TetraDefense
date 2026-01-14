using UnityEngine;

public class Utils
{

    public static string NormalizeNewlines(string input)
    {
        return input.Replace("\r\n", "\n");
    }

    public static int GetBitIndex(int input)
    {
        // Returns the index of the first bit that is set (starting from LSB)
        if (input == 0)
            return -1;

        int index = 0;
        while ((input & 1) == 0)
        {
            input >>= 1;
            index++;
        }
        return index;
    }

    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        float xDiff = b.x - a.x;
        float yDiff = b.y - a.y;
        float zDiff = b.z - a.z;
        float valueXDiff = value.x - a.x;
        float valueYDiff = value.y - a.y;
        float valueZDiff = value.z - a.z;

        return Mathf.Max(
            Mathf.Abs(xDiff) < 1e-6f ? valueXDiff / xDiff : 0f,
            Mathf.Abs(yDiff) < 1e-6f ? valueYDiff / yDiff : 0f,
            Mathf.Abs(zDiff) < 1e-6f ? valueZDiff / zDiff : 0f
        );
    }
}