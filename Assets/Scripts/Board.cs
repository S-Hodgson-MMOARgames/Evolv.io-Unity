// (C) MMOARgames, Inc. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

public class Board : Singleton<Board>
{
    public static int BoardWidth = 100;
    public static int BoardHeight = 100;
    public const int CREATURE_MINIMUM = 60;

    public const float MAX_TEMPERATURE = 1.0f;
    public const float MIN_TEMPERATURE = -0.5f;

    private const float ROCK_DENSITY = 5f;
    private const float MIN_ROCK_ENERGY = 0.8f;
    private const float MAX_ROCK_ENERGY = 1.6f;
    private const float MIN_CREATURE_ENERGY = 1.2f;
    private const float MAX_CREATURE_ENERGY = 2.0f;

    public static int CreatureCount = 0;

    public static float Year = 0;

    public static Tile[,] Tiles;
    public static List<SoftBody> Rocks = null;
    public static List<Creature> Creatures = null;
    public static Dictionary<Vector2, SoftBody> SoftBodies;
    public static readonly Color BackgroundColor = Color.HSVToRGB(0, 0, 0.1f);
    public static readonly Color RockColor = Color.HSVToRGB(0, 0, 0.5f);

    public Board(int width, int height, float stepSize)
    {
        BoardWidth = width;
        BoardHeight = height;
        Tiles = new Tile[width, height];
        SoftBodies = new Dictionary<Vector2, SoftBody>();

        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                float bigForce = Mathf.Pow((float)y / BoardHeight, 0.5f);
                float fertility = Mathf.PerlinNoise(x * stepSize * 3, y * stepSize * 3) * (1 - bigForce) * 5f +
                                  Mathf.PerlinNoise(x * stepSize * 0.5f, y * stepSize * 0.5f) * bigForce * 5f - 1.5f;
                float climateType = Mathf.PerlinNoise(x * stepSize * 0.2f + 10000, y * stepSize * 0.2f + 10000) * 1.63f - 0.4f;
                climateType = Mathf.Min(Mathf.Max(climateType, 0), 0.8f);

                Tiles[x, y] = new Tile(x, y, fertility, climateType);
            }
        }

        int rocksToAdd = Random.Range(10, 50);
        Rocks = new List<SoftBody>();
        for (int i = 0; i < rocksToAdd; i++)
        {
            float hue, sat, bri;
            Color.RGBToHSV(RockColor, out hue, out sat, out bri);
            Rocks.Add(new SoftBody(
                Random.Range(0, BoardWidth), Random.Range(0, BoardHeight), 0, 0,
                Mathf.Pow(Random.Range(MIN_ROCK_ENERGY, MAX_ROCK_ENERGY), 4), ROCK_DENSITY,
                hue, sat, bri));
        }

        Creatures = new List<Creature>();
        MaintainCreatureMinimum(false);
    }

    public static Color GetTileColor(int x, int y)
    {
        if (x >= 0 && x < BoardWidth && y >= 0 && y < BoardHeight)
        {
            return Tiles[x, y].TileColor;
        }

        return BackgroundColor;
    }

    private static void MaintainCreatureMinimum(bool choosePreExisting)
    {
        while (Creatures.Count < CREATURE_MINIMUM)
        {
            if (choosePreExisting)
            {
                Creature creature = GetRandomCreature();
                creature.MaintainPopulation();
            }
            else
            {
                Creatures.Add(new Creature(
                    newName: "",
                    mutateName: true,
                    newBrain: null,
                    newMouthHue: Random.Range(0, 1),
                    parents: null, newGeneration: 1,
                    newRotation: Random.Range(0, 2 * Mathf.PI), newRotationVelocity: 0,
                    newPositionX: Random.Range(0, BoardWidth), newPositionY: Random.Range(0, BoardHeight),
                    newVelocityX: 0, newVelocityY: 0,
                    newEnergy: Random.Range(MIN_CREATURE_ENERGY, MAX_CREATURE_ENERGY), newDensity: 1,
                    newHue: Random.Range(0, 1), newSaturation: 1, newBrightness: 1
                ));
            }
        }
    }

    private static Creature GetRandomCreature()
    {
        int index = Random.Range(0, Creatures.Count);
        return Creatures[index];
    }
}
