// (C) MMOARgames, Inc. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

public class Creature : SoftBody
{
    private const int MAX_VISION_RESULTS = 9;
    private const int ENERGY_HISTORY_LENGTH = 6;

    private const float AGE_FACTOR = 1f;
    private const float EAT_SPEED = 0.5f;
    private const float SAFE_SIZE = 1.25f;
    private const float MATURE_AGE = 0.01f;
    private const float EAT_ENERGY = 0.05f;
    private const float CROSS_SIZE = 0.022f;
    private const float TURN_ENERGY = 0.06f;
    private const float FIGHT_ENERGY = 0.06f;
    private const float SWIM_ENERGY = 0.0008f;
    private const float INJURED_ENERGY = 0.25f;
    private const float FOOD_SENSITIVITY = 0.3f;
    private const float MANUAL_BIRTH_SIZE = 1.2f;
    private const float MAX_VISION_DISTANCE = 10;
    private const float MIN_CREATURE_ENERGY = 1.2f;
    private const float MAX_CREATURE_ENERGY = 2.0f;
    private const float BRIGHTNESS_THRESHOLD = 0.7f;
    private const float ACCELERATION_ENERGY = 0.18f;
    private const float METABOLISM_ENERGY = 0.0004f;
    private const float ACCELERATION_BACK_ENERGY = 0.24f;
    private const float EAT_WHILE_MOVING_INEFFICIENCY_MULTIPLIER = 2.0f;

    internal const float MINIMUM_SURVIVABLE_SIZE = 0.06f;

    private static readonly float[] VisionAngles = {0.0f, -0.4f, 0.4f};

    private readonly float[] visionDistances = {0.0f, -0.7f, 0.7f};
    private readonly float[] prevEnergy = new float[ENERGY_HISTORY_LENGTH];
    private readonly float[] visionResults = new float[MAX_VISION_RESULTS];
    private readonly float[] visionOccludedX = new float[VisionAngles.Length];
    private readonly float[] visionOccludedY = new float[VisionAngles.Length];


    public int Id;
    public int Generation;
    public List<Creature> Parents;
    public Brain CreatureBrain;
    public float PreferredRank = 8;

    private float mouthHue;
    private float rotation;
    private float fightLevel;
    private float rotationVelocity;

    public Creature(
        string newName, bool mutateName,
        Brain newBrain, float newMouthHue,
        List<Creature> parents, int newGeneration,
        float newRotation, float newRotationVelocity,

        float newPositionX, float newPositionY, float newVelocityX, float newVelocityY, float newEnergy, float newDensity, float newHue, float newSaturation, float newBrightness) :
        base(newPositionX, newPositionY, newVelocityX, newVelocityY, newEnergy, newDensity, newHue, newSaturation, newBrightness)
    {
        Id = Board.CreatureCount + 1;
        Board.CreatureCount++;

        name = newName.Length >= 1 ?
            NameGenerator.SanitizeName(mutateName ?
                NameGenerator.MutateName(newName) : newName) :
            NameGenerator.NewName();

        if (newBrain == null)
        {
            CreatureBrain = new Brain(null, null);
        }

        mouthHue = newMouthHue;
        Parents = parents;
        Generation = newGeneration;
        rotation = newRotation;
        rotationVelocity = newRotationVelocity;

        for (int i = 0; i < MAX_VISION_RESULTS; i++)
        {
            visionResults[i] = 0;
        }
    }

    protected override void ApplyMotions(float timeStep)
    {
        if (GetRandomCoveredTile().Fertility > 1)
        {
            Energy -= SWIM_ENERGY * Energy;
        }

        base.ApplyMotions(timeStep);

        rotation += rotationVelocity;
        rotationVelocity *= Mathf.Max(0, 1 - FRICTION / GetMass());
    }

    public void UseBrain(float timeStep, bool useOutput)
    {
        var inputs = new float[Brain.NUMBER_OF_INPUTS];

        for (int i = 0; i < MAX_VISION_RESULTS; i++)
        {
            inputs[i] = visionResults[i];
        }

        inputs[9] = Energy;
        inputs[10] = mouthHue;
        CreatureBrain.Input(inputs);

        if (useOutput)
        {
            float[] output = CreatureBrain.Outputs();
            Hue = Mathf.Abs(output[0]) % 1.0f;
            Accelerate(output[1], timeStep);
            Turn(output[2], timeStep);
            Eat(output[3], timeStep);
            Fight(output[4], timeStep);

            if (output[5] > 0 && Board.Year - BirthTime >= MATURE_AGE && Energy > SAFE_SIZE)
            {
                Reproduce(SAFE_SIZE);
            }

            mouthHue = Mathf.Abs(output[10]) % 1.0f;
        }
    }

    private void Accelerate(float amount, float timeStep)
    {
        float multiplier = amount * timeStep * GetMass();

        VelocityX += Mathf.Cos(rotation) * multiplier;
        VelocityY += Mathf.Sin(rotation) * multiplier;

        if (amount >= 0)
        {
            Energy -= amount * ACCELERATION_BACK_ENERGY * timeStep;
        }
        else
        {
            Energy -= Mathf.Abs(amount * ACCELERATION_BACK_ENERGY * timeStep);
        }
    }

    private void Turn(float amount, float timeStep)
    {
        rotationVelocity += 0.4f * amount * timeStep / GetMass();

        Energy -= Mathf.Abs(amount * TURN_ENERGY * Energy * timeStep);
    }

    private void Eat(float amount, float timeStep)
    {
        // The faster you're moving the less efficiently the creature can eat.
        float totalAmount = amount / (1.0f + MathUtils.CalcTileDist(0, 0, VelocityX, VelocityY) * EAT_WHILE_MOVING_INEFFICIENCY_MULTIPLIER);

        if (totalAmount < 0)
        {
            RadiateEnergy(-amount * timeStep);
            Energy -= -amount * EAT_ENERGY * timeStep;
        }
        else
        {
            Tile coveredTile = GetRandomCoveredTile();
            float foodToEat = coveredTile.FoodLevel * (1 - Mathf.Pow(1 - EAT_SPEED, totalAmount * timeStep));

            if (foodToEat > coveredTile.FoodLevel)
            {
                foodToEat = coveredTile.FoodLevel;
            }

            coveredTile.FoodLevel -= foodToEat;

            float foodDistance = Mathf.Abs(coveredTile.FoodType - mouthHue);
            float multiplier = 1.0f - foodDistance / FOOD_SENSITIVITY;

            if (multiplier >= 0)
            {
                Energy += foodToEat * multiplier;
            }
            else
            {
                Energy -= -foodToEat * multiplier;
            }

            Energy -= amount * EAT_ENERGY * timeStep;
        }
    }

    private void Fight(float amount, float timeStep)
    {
        if (amount > 0 && Board.Year - BirthTime >= MATURE_AGE)
        {
            fightLevel = amount;

            Energy -= fightLevel * FIGHT_ENERGY * Energy * timeStep;

            foreach (SoftBody softBody in SoftBodies)
            {
                var otherCreature = (Creature)softBody;

                if (otherCreature != null)
                {
                    float distance = MathUtils.CalcTileDist(PositionX, PositionY, otherCreature.PositionX, otherCreature.PositionY);
                    float combinedRadius = GetRadius() * INFLUENCE_AREA + otherCreature.GetRadius();

                    if (distance < combinedRadius)
                    {
                        otherCreature.RadiateEnergy(fightLevel * INJURED_ENERGY * timeStep);
                    }
                }
            }
        }
        else
        {
            fightLevel = 0;
        }
    }

    private void Reproduce(float spawnSize)
    {
        int hightestGen = 0;

        if (spawnSize >= 0 && BirthTime > MATURE_AGE)
        {
            var parents = new List<Creature> { this };

            float availibleEnergy = Energy - SAFE_SIZE;

            foreach (SoftBody softBody in SoftBodies)
            {
                var potentialMate = (Creature)softBody;

                if (potentialMate.BirthTime > MATURE_AGE &&
                   (potentialMate.fightLevel < fightLevel ||
                    potentialMate.CreatureBrain.Outputs()[9] > -1))
                {
                    float distance = MathUtils.CalcTileDist(PositionX, PositionY, potentialMate.PositionX,
                        potentialMate.PositionY);
                    float combinedRadius = GetRadius() * INFLUENCE_AREA + potentialMate.GetRadius();

                    if (distance < combinedRadius)
                    {
                        parents.Add(potentialMate);
                        availibleEnergy += potentialMate.Energy - SAFE_SIZE;
                    }
                }
            }

            if (availibleEnergy > spawnSize)
            {
                // To avoid / by 0 error...
                float newPosX = Random.Range(-0.01f, 0.01f);
                float newPosY = Random.Range(-0.01f, 0.01f);

                float newHue = 0;
                float newSat = 0;
                float newBri = 0;
                float newMouthHue = 0;
                var parentnames = new string[parents.Count];

                Brain newBrain = Brain.Evolve(parents);

                for (int i = 0; i < parents.Count; i++)
                {
                    int chosenIndex = Random.Range(0, parents.Count);
                    Creature parent = parents[chosenIndex];

                    parent.Energy -= spawnSize * (parent.Energy - SAFE_SIZE / availibleEnergy);

                    newPosX += parent.PositionX / parents.Count;
                    newPosY += parent.PositionY / parents.Count;
                    newHue += parent.Hue / parents.Count;
                    newSat += parent.Saturation / parents.Count;
                    newBri += parent.Brightness / parents.Count;
                    newMouthHue += parent.mouthHue / parents.Count;

                    parentnames[i] = parent.name;

                    if (parent.Generation > hightestGen)
                    {
                        hightestGen = parent.Generation;
                    }
                }

                newSat = 1;
                newBri = 1;

                Board.Creatures.Add(
                    new Creature(
                        newName: NameGenerator.CombineNames(parentnames),
                        mutateName: true,
                        newBrain: newBrain,
                        newMouthHue: newMouthHue,
                        parents: parents,
                        newGeneration: hightestGen + 1,
                        newRotation: Random.Range(0, 2 * Mathf.PI),
                        newRotationVelocity: 0,
                        newPositionX: newPosX,
                        newPositionY: newPosY,
                        newVelocityX: 0,
                        newVelocityY: 0,
                        newEnergy: spawnSize,
                        newDensity: Density,
                        newHue: newHue,
                        newSaturation: newSat,
                        newBrightness: newBri));
            }
        }
    }

    public void Metabolize(float timeStep)
    {
        float age = AGE_FACTOR * (Board.Year - BirthTime);
        Energy -= Energy * METABOLISM_ENERGY * age * timeStep;

        if (Energy < SAFE_SIZE)
        {
            ReturnToEarth();
            Board.Creatures.Remove(this);
        }
    }

    private void ReturnToEarth()
    {
        const int pieces = 20;

        for (int i = 0; i < pieces; i++)
        {
            GetRandomCoveredTile().FoodLevel += Energy / pieces;
        }

        for (int x = MinX; x < MaxX; x++)
        {
            for (int y = MinY; y < MaxY; y++)
            {
                Board.SoftBodies.Remove(new Vector2(x, y));
            }
        }
    }

    public void See()
    {
        for (int i = 0; i < VisionAngles.Length; i++)
        {
            float visionAngle = rotation + VisionAngles[i];
            float endX = GetVisionEndX(i);
            float endY = GetVisionEndY(i);

            visionOccludedX[i] = endX;
            visionOccludedY[i] = endY;

            Color color = Board.GetTileColor((int)endX, (int)endY);

            float hue, sat, bri;
            Color.RGBToHSV(color, out hue, out sat, out bri);

            visionResults[i * 3] = hue;
            visionResults[i * 3 + 1] = sat;
            visionResults[i * 3 + 2] = bri;

            int prevTileX = -1;
            int prevTileY = -1;

            var potentialOccuders = new List<SoftBody>();

            for (int j = 0; j < visionDistances[i] + 1; j++)
            {
                int tileX = (int)(PositionX + Mathf.Cos(visionAngle) * j);
                int tileY = (int)(PositionY + Mathf.Sin(visionAngle) * j);

                if (tileX != prevTileX || tileY != prevTileY)
                {
                    AddPotentialOcculder(tileX, tileY, ref potentialOccuders);
                    if (prevTileX >= 0 && tileX != prevTileX && prevTileY >= 0 && tileY != prevTileY)
                    {
                        AddPotentialOcculder(prevTileX, tileY, ref potentialOccuders);
                        AddPotentialOcculder(tileX, prevTileY, ref potentialOccuders);
                    }
                }

                prevTileX = tileX;
                prevTileY = tileY;
            }

            var rotationMatrix = new float[2, 2];
            rotationMatrix[0, 0] = rotationMatrix[1, 1] = Mathf.Cos(-visionAngle);
            rotationMatrix[0, 1] = Mathf.Sin(-visionAngle);
            rotationMatrix[1, 0] = -rotationMatrix[0, 1];

            float visionLineLength = visionDistances[i];

            foreach (SoftBody potentialOccluder in potentialOccuders)
            {
                float posX = potentialOccluder.PositionX - PositionX;
                float posY = potentialOccluder.PositionY - PositionY;
                float radius = potentialOccluder.GetRadius();
                float translatedX = rotationMatrix[0, 0] * posX + rotationMatrix[1, 0] * posY;
                float translatedY = rotationMatrix[0, 1] * posX + rotationMatrix[1, 1] * posY;

                if (Mathf.Abs(translatedY) <= radius)
                {
                    if (translatedX >= 0 && translatedX < visionLineLength && translatedY < visionLineLength ||
                            MathUtils.CalcTileDist(0, 0, translatedX, translatedY) < radius ||
                            MathUtils.CalcTileDist(visionLineLength, 0, translatedX, translatedY) < radius)
                    {
                        visionLineLength = translatedX - Mathf.Sqrt(radius * radius - translatedY * translatedY);
                        visionOccludedX[i] = PositionX + visionLineLength * Mathf.Cos(visionAngle);
                        visionOccludedY[i] = PositionY + visionLineLength * Mathf.Sin(visionAngle);

                        visionResults[i * 3] = potentialOccluder.Hue;
                        visionResults[i * 3 + 1] = potentialOccluder.Saturation;
                        visionResults[i * 3 + 2] = potentialOccluder.Brightness;
                    }
                }
            }
        }
    }

    private void AddPotentialOcculder(int x, int y, ref List<SoftBody> softBodies)
    {
        if (x >= 0 && x < Board.BoardWidth && y >= 0 && y < Board.BoardHeight)
        {
            SoftBody occluder;
            if (Board.SoftBodies.TryGetValue(new Vector2(x, y), out occluder))
            {
                if (!softBodies.Contains(occluder) && occluder != this)
                {
                    softBodies.Add(occluder);
                }
            }
        }
    }

    private float GetVisionEndX(int index)
    {
        float visionTotalAngle = rotation + VisionAngles[index];
        return PositionX + visionDistances[index] * Mathf.Cos(visionTotalAngle);
    }

    private float GetVisionEndY(int index)
    {
        float visionTotalAngle = rotation + VisionAngles[index];
        return PositionY + visionDistances[index] * Mathf.Sin(visionTotalAngle);
    }

    public void MaintainPopulation()
    {
        Energy += SAFE_SIZE;
        Reproduce(SAFE_SIZE);
    }

    /// <summary>
    /// Puts energy back into the environment.
    /// </summary>
    /// <param name="energyLost"></param>
    private void RadiateEnergy(float energyLost)
    {
        if (energyLost > 0)
        {
            energyLost = Mathf.Min(energyLost, Energy);
            Energy -= energyLost;
            GetRandomCoveredTile().FoodLevel += energyLost;
        }
    }

    public float GetEnergyUsage(float timeStep)
    {
        return Energy - prevEnergy[ENERGY_HISTORY_LENGTH - 1] / ENERGY_HISTORY_LENGTH / timeStep;
    }

    private Tile GetRandomCoveredTile()
    {
        float radius = GetRadius();
        float choiceX = 0;
        float choiceY = 0;

        while (MathUtils.CalcTileDist(PositionX, PositionY, choiceX, choiceY) > radius)
        {
            choiceX = Random.value * 2 * radius - radius + PositionX;
            choiceY = Random.value * 2 * radius - radius + PositionY;
        }

        int x = XBound((int)choiceX);
        int y = YBound((int)choiceY);

        return Board.Tiles[x, y];
    }
}
