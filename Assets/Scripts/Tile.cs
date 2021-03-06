﻿// (C) MMOARgames, Inc. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    private const float FOOD_GROWTH_RATE = 1.0f;
    private const float MAX_GROWTH_LEVEL = 3.0f;

    private readonly Color barrenColor  = Color.HSVToRGB(0.0f, 0.0f, 1.0f);
    private readonly Color fertileColor = Color.HSVToRGB(0.0f, 0.0f, 0.2f);
    private readonly Color blackColor   = Color.HSVToRGB(0.0f, 1.0f, 0.0f);
    private readonly Color waterColor   = Color.HSVToRGB(0.0f, 0.0f, 0.0f);

    public float FoodType;

    public float FoodLevel
    {
        get { return foodLevel; }
    }

    private float foodLevel;
    public float Fertility;

    [SerializeField]
    private Color tileColor;
    public Color TileColor
    {
        get
        {
            if (Fertility > 1)
            {
                return waterColor;
            }

            Color foodColor = Color.HSVToRGB(FoodType, 1, 1);

            if (FoodLevel < MAX_GROWTH_LEVEL)
            {
                tileColor = InterpolateColorFixedHue(InterpolateColor(barrenColor, fertileColor, Fertility), foodColor, FoodLevel / MAX_GROWTH_LEVEL, FoodType);
            }
            else
            {
                tileColor = InterpolateColorFixedHue(foodColor, blackColor, 1.0f - MAX_GROWTH_LEVEL / FoodLevel, FoodType);
            }

            return tileColor;
        }
    }

    private float lastUpdateTime;

    public List<SoftBody> SoftBodies;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.color = TileColor;
    }

    public void AddFood(float amount, bool canIterate = true)
    {
        if (canIterate)
        {
            Iterate();
        }
        foodLevel += amount;
    }

    public void RemoveFood(float amount, bool canIterate = true)
    {
        if (canIterate)
        {
            Iterate();
        }
        foodLevel -= amount;
    }

    public void Iterate()
    {
        // If we're a water tile.
        if (Fertility > 1)
        {
            foodLevel = 0;
        }
        else
        {
            float updateTime = Board.Instance.Year;

            if (!(Mathf.Abs(lastUpdateTime - updateTime) >= 0.00001f))
            {
                return;
            }

            float growthChange = GetGrowthOverTimeRange(lastUpdateTime, updateTime);

            if (growthChange > 0)
            {
                if (FoodLevel < MAX_GROWTH_LEVEL)
                {
                    AddFood(MAX_GROWTH_LEVEL - (MAX_GROWTH_LEVEL - FoodLevel) * Mathf.Pow(MathUtils.EULERS_NUMBER, -growthChange * Fertility * FOOD_GROWTH_RATE) - FoodLevel, false);
                }
                else
                {
                    RemoveFood(FoodLevel - FoodLevel * Mathf.Pow(MathUtils.EULERS_NUMBER, growthChange * FOOD_GROWTH_RATE), false);
                }
            }

            foodLevel = Mathf.Max(FoodLevel, 0);

            spriteRenderer.color = TileColor;

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
        return (endTime - startTime) * (Board.MIN_TEMPERATURE + (Board.MAX_TEMPERATURE - Board.MIN_TEMPERATURE) * 0.5f) + (Board.MAX_TEMPERATURE - Board.MIN_TEMPERATURE) / Mathf.PI / 4 * (Mathf.Sin(2 * Mathf.PI * startTime) - Mathf.Sin(2 * Mathf.PI * endTime));
    }
}
