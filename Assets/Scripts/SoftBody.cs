// (C) MMOARgames, Inc. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

public class SoftBody : MonoBehaviour
{
    private const float COLLISION_FORCE = 0.01f;

    protected const float INFLUENCE_AREA = 2.0f;
    protected const float FRICTION = 0.004f;

    public float PositionX;
    public float PositionY;
    public float VelocityX;
    public float VelocityY;

    /// <summary>
    /// Set so when a createure is of minimum size, it equals one.
    /// </summary>
    public float EnergyDensity;
    public float Density;
    protected float Energy
    {
        get
        {
            if (energy < 0)
            {
                energy = 0;
            }

            return energy;
        }
        set
        {
            energy = value;
            if (energy < 0)
            {
                energy = 0;
            }
        }
    }
    private float energy;

    public float Hue;
    public float Saturation;
    public float Brightness;
    public float BirthTime;

    public int MinX;
    public int MinY;
    public int MaxX;
    public int MaxY;

    public int PrevMinX;
    public int PrevMinY;
    public int PrevMaxX;
    public int PrevMaxY;

    public List<SoftBody> SoftBodies;

    public SoftBody(
        float newPositionX, float newPositionY,
        float newVelocityX, float newVelocityY,
        float newEnergy, float newDensity,
        float newHue, float newSaturation, float newBrightness)
    {
        BirthTime = Board.Year;

        PositionX = newPositionX;
        PositionY = newPositionY;
        VelocityX = newVelocityX;
        VelocityY = newVelocityY;

        Energy = newEnergy;
        Density = newDensity;

        Hue = newHue;
        Saturation = newSaturation;
        Brightness = newBrightness;

        SetSoftBodies(false);
        SetSoftBodies(false);

        EnergyDensity = 1.0f / (Creature.MINIMUM_SURVIVABLE_SIZE * Creature.MINIMUM_SURVIVABLE_SIZE * Mathf.PI);
    }

    private void SetSoftBodies(bool shouldRemove)
    {
        float radius = GetRadius() * INFLUENCE_AREA;

        PrevMinX = MinX;
        PrevMinY = MinY;
        PrevMaxX = MaxX;
        PrevMaxY = MaxY;

        MinX = XBound((int)Mathf.Floor(PositionX - radius));
        MinY = YBound((int)Mathf.Floor(PositionY - radius));
        MaxX = XBound((int)Mathf.Floor(PositionX + radius));
        MaxY = YBound((int)Mathf.Floor(PositionY + radius));

        if (PrevMinX != MinX || PrevMinY != MinY ||
            PrevMaxX != MaxX || PrevMaxY != MaxY)
        {
            if (shouldRemove)
            {
                for (int x = PrevMinX; x <= PrevMaxY; x++)
                {
                    for (int y = PrevMinY; y <= PrevMaxY; y++)
                    {
                        if (x < MinX || x > MaxX ||
                            y < MinY || x > MaxY)
                        {
                            Board.SoftBodies.Remove(new Vector2(x, y));
                        }
                    }
                }
            }
            for (int x = MinX; x <= MaxY; x++)
            {
                for (int y = MinY; y <= MaxY; y++)
                {
                    if (x < PrevMinX || x > PrevMaxX ||
                        y < PrevMinY || x > PrevMaxY)
                    {
                        Board.SoftBodies.Add(new Vector2(x, y), this);
                    }
                }
            }
        }
    }

    protected void Collide()
    {
        SoftBodies = new List<SoftBody>();

        for (int x = MinX; x < MaxX; x++)
        {
            for (int y = MinY; y < MaxY; y++)
            {
                foreach (KeyValuePair<Vector2, SoftBody> softBody in Board.SoftBodies)
                {
                    if (SoftBodies.Contains(softBody.Value) && softBody.Value != this)
                    {
                        SoftBodies.Add(softBody.Value);
                    }
                }
            }
        }

        foreach (SoftBody softBody in SoftBodies)
        {
            float distance = MathUtils.CalcTileDist(PositionX, PositionY, softBody.PositionX, softBody.PositionY);
            float combineRadius = GetRadius() + softBody.GetRadius();

            if (distance < combineRadius)
            {
                float force = combineRadius * COLLISION_FORCE;
                VelocityX += (PositionX - softBody.PositionX) / distance * force / GetMass();
                VelocityY += (PositionY - softBody.PositionY) / distance * force / GetMass();
            }
        }
    }

    protected virtual void ApplyMotions(float timeStep)
    {
        PositionX = BodyXBound(PositionX + VelocityX * timeStep);
        PositionY = BodyYBound(PositionY + VelocityY * timeStep);

        VelocityX *= Mathf.Max(0, 1 - FRICTION / GetMass());
        VelocityY *= Mathf.Max(0, 1 - FRICTION / GetMass());

        SetSoftBodies(true);
    }

    protected static int XBound(int x)
    {
        return Mathf.Min(Mathf.Max(x, 0), Board.BoardWidth - 1);
    }

    protected static int YBound(int y)
    {
        return Mathf.Min(Mathf.Max(y, 0), Board.BoardHeight - 1);
    }

    private float BodyXBound(float x)
    {
        return Mathf.Min(Mathf.Max(x, GetRadius()), Board.BoardWidth - GetRadius());
    }

    private float BodyYBound(float y)
    {
        return Mathf.Min(Mathf.Max(y, GetRadius()), Board.BoardHeight - GetRadius());
    }

    protected internal float GetRadius()
    {
        return Energy <= 0 ? 0 : Mathf.Sqrt(Energy / EnergyDensity / Mathf.PI);
    }

    protected float GetMass()
    {
        return Energy / EnergyDensity * Density;
    }
}
