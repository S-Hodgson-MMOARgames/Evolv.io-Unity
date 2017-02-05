// (C) MMOARgames, Inc. All Rights Reserved.

using UnityEngine;

public class Tile : MonoBehaviour
{
    public readonly int PositionXPos;
    public readonly int PositionYPos;

    private const float FOOD_GROWTH_RATE = 1.0f;
    private const float MAX_GROWTH_LEVEL = 3.0f;

    private readonly Color barrenColor  = Color.HSVToRGB(0.0f, 0.0f, 1.0f);
    private readonly Color fertileColor = Color.HSVToRGB(0.0f, 0.0f, 0.2f);
    private readonly Color blackColor   = Color.HSVToRGB(0.0f, 1.0f, 0.0f);
    private readonly Color waterColor   = Color.HSVToRGB(0.0f, 0.0f, 0.0f);

    public float FoodType { get; set; }
    public float FoodLevel { get; set; }
    public float Fertility { get; set; }
    public float ClimateType { get; set; }

    [SerializeField]
    private Color tileColor;
    public Color TileColor
    {
        get
        {
            Color foodColor = Color.HSVToRGB(FoodType, 1, 1);

            if (Fertility > 1)
            {
                return waterColor;
            }

            tileColor = FoodLevel < MAX_GROWTH_LEVEL ?
                InterpolateColorFixedHue(InterpolateColor(barrenColor, fertileColor, Fertility), foodColor, FoodLevel / MAX_GROWTH_LEVEL, FoodType) :
                InterpolateColorFixedHue(foodColor, blackColor, 1.0f - MAX_GROWTH_LEVEL / FoodLevel, FoodType);

            return tileColor;
        }
    }

    private float lastUpdateTime;

    public Tile(int xPos, int yPos, float newFertility, float type)
    {
        PositionXPos = xPos;
        PositionYPos = yPos;
        Fertility = Mathf.Max(0, newFertility);
        ClimateType = FoodType = type;
    }

    private void Update()
    {
        float updateTime = Board.Year;

        if (Mathf.Abs(lastUpdateTime - updateTime) >= 0.00001f)
        {
            float growthChange = GetGrowthOverTimeRange(lastUpdateTime, updateTime);

            // If we're a water tile.
            if (Fertility > 1)
            {
                FoodLevel = 0;
            }
            else
            {
                if (growthChange > 0)
                {
                    if (FoodLevel < MAX_GROWTH_LEVEL)
                    {
                        float newDistToMax = (MAX_GROWTH_LEVEL - FoodLevel) * Mathf.Pow(MathUtils.EULERS_NUMBER, -growthChange * Fertility * FOOD_GROWTH_RATE);
                        float foodGrowthAmount = MAX_GROWTH_LEVEL - newDistToMax - FoodLevel;

                        FoodLevel += foodGrowthAmount;
                    }
                    else
                    {
                        FoodLevel -= FoodLevel - FoodLevel * Mathf.Pow(MathUtils.EULERS_NUMBER, growthChange * FOOD_GROWTH_RATE);
                    }
                }
            }

            FoodLevel = Mathf.Max(FoodLevel, 0);
            lastUpdateTime = updateTime;
        }
    }

    private static Color InterpolateColor(Color a, Color b, float x)
    {
        float aHue, bHue;
        float aSat, bSat;
        float aBri, bBri;

        Color.RGBToHSV(a, out aHue, out aSat, out aBri);
        Color.RGBToHSV(b, out bHue, out bSat, out bBri);

        float hue = Interpolate(aHue, bHue, x);
        float sat = Interpolate(aSat, bSat, x);
        float bri = Interpolate(aBri, bBri, x);

        return Color.HSVToRGB(hue, sat, bri);
    }

    private static Color InterpolateColorFixedHue(Color a, Color b, float x, float hue)
    {
        float aHue, bHue;
        float aSat, bSat;
        float aBri, bBri;

        Color.RGBToHSV(a, out aHue, out aSat, out aBri);
        Color.RGBToHSV(b, out bHue, out bSat, out bBri);

        if (!(bBri > 0))
        {
            bSat = 1;
        }

        float sat = Interpolate(aSat, bSat, x);
        float bri = Interpolate(aBri, bBri, x);

        return Color.HSVToRGB(hue, sat, bri);
    }

    private static float Interpolate(float a, float b, float x)
    {
        return a + (b - a) * x;
    }

    private static float GetGrowthOverTimeRange(float startTime, float endTime)
    {
        const float tempRange = Board.MAX_TEMPERATURE - Board.MIN_TEMPERATURE;
        const float meanTemp = Board.MIN_TEMPERATURE + tempRange * 0.5f;

        return (endTime - startTime) * meanTemp + tempRange / Mathf.PI / 4 * (Mathf.Sin(2 * Mathf.PI * startTime) - Mathf.Sin(2 * Mathf.PI * endTime));
    }
}
