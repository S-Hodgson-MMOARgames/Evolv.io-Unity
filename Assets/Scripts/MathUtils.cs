// (C) MMOARgames, Inc. All Rights Reserved.

using UnityEngine;

public class MathUtils : MonoBehaviour
{
    public const float EULERS_NUMBER = 2.718281828459f;

    /// <summary>
    /// Calculates the distance between two points.
    /// </summary>
    /// <param name="x1">First X coordinate.</param>
    /// <param name="y1">First Y coordinate.</param>
    /// <param name="x2">Second X coodinate.</param>
    /// <param name="y2">Second Y coodinate.</param>
    /// <returns></returns>
    public static float CalcTileDist(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
    }
}
