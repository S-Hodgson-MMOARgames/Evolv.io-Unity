// (C) MMOARgames, Inc. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SoftBody : MonoBehaviour
{
    private const float COLLISION_FORCE = 0.01f;

    protected const float INFLUENCE_AREA = 2.0f;
    protected const float FRICTION = 0.004f;

    public float VelocityX;
    public float VelocityY;

    /// <summary>
    /// Set so when a createure is of minimum size, it equals one.
    /// </summary>
    public float EnergyDensity;
    public float Density;

    public float Energy
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
    protected SpriteRenderer spriteRenderer;
    private Vector3 position;

    protected virtual void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        BirthTime = Board.Instance.Year;

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

        MinX = XBound((int)Mathf.Floor(transform.position.x - radius));
        MinY = YBound((int)Mathf.Floor(transform.position.y - radius));
        MaxX = XBound((int)Mathf.Floor(transform.position.x + radius));
        MaxY = YBound((int)Mathf.Floor(transform.position.y + radius));

        if (PrevMinX == MinX && PrevMinY == MinY && PrevMaxX == MaxX && PrevMaxY == MaxY)
        {
            return;
        }

        if (shouldRemove)
        {
            for (int x = PrevMinX; x <= PrevMaxY; x++)
            {
                for (int y = PrevMinY; y <= PrevMaxY; y++)
                {
                    if (x < MinX ||
                        x > MaxX ||
                        y < MinY ||
                        y > MaxY)
                    {
                        Board.Tiles[x, y].SoftBodies.Remove(this);
                    }
                }
            }
        }

        for (int x = MinX; x <= MaxY; x++)
        {
            for (int y = MinY; y <= MaxY; y++)
            {
                if (x < PrevMinX ||
                    x > PrevMaxX ||
                    y < PrevMinY ||
                    y > PrevMaxY)
                {
                    Board.Tiles[x, y].SoftBodies.Add(this);
                }
            }
        }
    }

    protected void Collide()
    {
        SoftBodies.Clear();

        for (int x = MinX; x <= MaxX; x++)
        {
            for (int y = MinY; y <= MaxY; y++)
            {
                for (int i = 0; i < Board.Tiles[x, y].SoftBodies.Count; i++)
                {
                    if (Board.Tiles[x, y].SoftBodies[i] != this && Board.Tiles[x, y].SoftBodies[i] != null)
                    {
                        SoftBodies.Add(Board.Tiles[x, y].SoftBodies[i]);
                    }
                }
            }
        }

        for (int i = 0; i < SoftBodies.Count; i++)
        {
            if (SoftBodies[i] == null)
            {
                SoftBodies.Remove(SoftBodies[i]);
                continue;
            }

            float distance = MathUtils.CalcTileDist(
                transform.position.x,
                transform.position.y,
                SoftBodies[i].transform.position.x,
                SoftBodies[i].transform.position.y);
            float combineRadius = GetRadius() + SoftBodies[i].GetRadius();

            if (distance < combineRadius)
            {
                float force = combineRadius * COLLISION_FORCE;
                VelocityX += (transform.position.x - SoftBodies[i].transform.position.x) / distance * force / GetMass();
                VelocityY += (transform.position.y - SoftBodies[i].transform.position.y) / distance * force / GetMass();
            }
        }
    }

    protected virtual void ApplyMotions(float timeStep)
    {
        position.x = BodyXBound(transform.position.x + VelocityX * timeStep);
        position.y = BodyYBound(transform.position.y + VelocityY * timeStep);

        gameObject.transform.position = position;

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
