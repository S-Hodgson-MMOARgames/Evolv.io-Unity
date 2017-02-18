// (C) MMOARgames, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : Singleton<Board>
{
    [SerializeField]
    private Vector2 boardSize;

    public static int BoardWidth;
    public static int BoardHeight;

    private const int CREATURE_MINIMUM = 60;
    private const int  POPULATION_HISTORY_LENGTH = 200;

    public const float MAX_TEMPERATURE = 1.0f;
    public const float MIN_TEMPERATURE = -0.5f;

    private const float ROCK_DENSITY = 5f;
    private const float MIN_ROCK_ENERGY = 0.8f;
    private const float MAX_ROCK_ENERGY = 1.6f;
    private const float MIN_CREATURE_ENERGY = 1.2f;
    private const float MAX_CREATURE_ENERGY = 2.0f;
    private const float RECORD_POPULATION_FEQUENCY = 0.02f;

    public static int CreatureIndex = 0;

    public float Year;
    public float SimSpeedMultiplier = 0.0001f;

    public float GlobalTemperature;

    public static Tile[,] Tiles;
    public static List<SoftBody> Rocks = null;
    public static List<Creature> Creatures = null;
    public static readonly Color BackgroundColor = Color.HSVToRGB(0, 0, 0.1f);
    public static readonly Color RockColor = Color.HSVToRGB(0, 0, 0.5f);
    public static int[] PopulationHistory;

    public GameObject TilePrefab;
    private GameObject tileGroup;
    public GameObject RockPrefab;
    public GameObject RockGroup;
    public GameObject CreaturePrefab;
    public GameObject CreatureGroup;

    private void Start()
    {
        Random.InitState(1000000);

        tileGroup = new GameObject("Tiles");
        RockGroup = new GameObject("Rocks");
        CreatureGroup = new GameObject("Creatures");

        tileGroup.transform.SetParent(transform);
        RockGroup.transform.SetParent(transform);
        CreatureGroup.transform.SetParent(transform);

        BoardWidth = (int)boardSize.x;
        BoardHeight = (int)boardSize.y;

        Tiles = new Tile[BoardWidth, BoardHeight];

        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                float bigForce = Mathf.Pow((float)y / BoardHeight, 0.5f);
                float fertility = Mathf.PerlinNoise(x * 3, y * 3) * (1 - bigForce) * 5f +
                                  Mathf.PerlinNoise(x * 0.5f, y * 0.5f) * bigForce * 5f - 1.5f;
                float climateType = Mathf.PerlinNoise(x * 0.2f + 10000, y * 0.2f + 10000) * 1.63f - 0.4f;
                climateType = Mathf.Min(Mathf.Max(climateType, 0), 0.8f);

                var newTileObj = Instantiate(TilePrefab, new Vector3(x, y, 0), Quaternion.identity, tileGroup.transform);
                var tile = newTileObj.GetComponent<Tile>();

                tile.Fertility = fertility;
                tile.FoodType = climateType;
                Tiles[x, y] = tile;
            }
        }

        int rocksToAdd = Random.Range(10, 50);
        Rocks = new List<SoftBody>();

        for (int i = 0; i < rocksToAdd; i++)
        {
            float hue, sat, bri;
            Color.RGBToHSV(RockColor, out hue, out sat, out bri);

            var newRockObj = Instantiate(RockPrefab, new Vector3(Random.Range(0, BoardWidth), Random.Range(0, BoardHeight), 0), Quaternion.identity, RockGroup.transform);
            var rock = newRockObj.GetComponent<SoftBody>();

            rock.VelocityX = 0;
            rock.VelocityY = 0;
            rock.Energy = Mathf.Pow(Random.Range(MIN_ROCK_ENERGY, MAX_ROCK_ENERGY), 4);
            rock.EnergyDensity = ROCK_DENSITY;
            rock.Hue = hue;
            rock.Saturation = sat;
            rock.Brightness = bri;
        }

        Creatures = new List<Creature>();
        MaintainCreatureMinimum(false);

        PopulationHistory = new int[POPULATION_HISTORY_LENGTH];
        for (int i = 0; i < POPULATION_HISTORY_LENGTH; i++)
        {
            PopulationHistory[i] = 0;
        }
    }

    private void FixedUpdate()
    {
        float prevYear = Instance.Year;
        Instance.Year += Time.deltaTime * SimSpeedMultiplier;

        if (Math.Abs(Mathf.Floor(Instance.Year / RECORD_POPULATION_FEQUENCY) - Mathf.Floor(prevYear / RECORD_POPULATION_FEQUENCY)) > 0.00001f)
        {
            for (int i = POPULATION_HISTORY_LENGTH - 1; i >= 1; i--)
            {
                PopulationHistory[i] = PopulationHistory[i - 1];
            }

            PopulationHistory[0] = Creatures.Count;
        }

        GlobalTemperature = GetGrowthRate(GetSeason());
        float tempChangedIntoFrame = GlobalTemperature - GetGrowthRate(GetSeason() - Instance.Year);
        float tempChangedOutOfFrame = GetGrowthRate(GetSeason() + Instance.Year) - GlobalTemperature;

        if (tempChangedIntoFrame * tempChangedOutOfFrame <= 0)
        {
            for (int x = 0; x < BoardHeight; x++)
            {
                for (int y = 0; y < BoardHeight; y++)
                {
                    Tiles[x,y].Iterate();
                }
            }
        }

        MaintainCreatureMinimum(false);
    }

    private static float GetSeason()
    {
        return Instance.Year % 1.0f;
    }

    private static float GetGrowthRate(float time)
    {
        float tempRange = MAX_TEMPERATURE - MIN_TEMPERATURE;
        return MIN_TEMPERATURE + tempRange * 0.5f - tempRange * 0.5f * Mathf.Cos(time * 2 * Mathf.PI);
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
                creature.MaintainEnergy();
            }
            else
            {
                var newCreatureObj = Instantiate(Instance.CreaturePrefab, new Vector3(Random.Range(0, BoardWidth), Random.Range(0, BoardHeight), 0), Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward), Instance.CreatureGroup.transform);
                var newCreature = newCreatureObj.GetComponent<Creature>();

                newCreature.name = NameGenerator.NewName();
                newCreature.name = newCreature.name.Length >= 1 ? NameGenerator.SanitizeName(newCreature.name) : NameGenerator.NewName();
                newCreature.MouthHue = Random.Range(0, 1);
                newCreature.Generation = 1;
                newCreature.Energy = Random.Range(MIN_CREATURE_ENERGY, MAX_CREATURE_ENERGY);
                newCreature.Density = 1;
                newCreature.Hue = Random.Range(0, 1);
                newCreature.Saturation = 1;
                newCreature.Brightness = 1;

                Creatures.Add(newCreature);
            }
        }
    }

    private static Creature GetRandomCreature()
    {
        int index = Random.Range(0, Creatures.Count);
        return Creatures[index];
    }
}
