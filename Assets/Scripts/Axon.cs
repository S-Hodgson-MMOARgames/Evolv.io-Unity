// (C) MMOARgames, Inc. All Rights Reserved.

using UnityEngine;

public class Axon
{
    private const int MUTATE_POWER = 9;
    private const float MUTABILITY_MULTIPLIER = 0.7f;

    private readonly float mutationMultiplier;

    public float Weight;
    public float Mutability;

    public Axon(float weight, float mutability)
    {
        Weight = weight;
        Mutability = mutability;
        mutationMultiplier = Mathf.Pow(0.5f, MUTATE_POWER);
    }

    public Axon MutateAxon()
    {
        float mutateMutability = Mathf.Pow(0.5f, RandomSeed() * MUTABILITY_MULTIPLIER);
        return new Axon(Weight + RandomPower() * Mutability / mutationMultiplier, Mutability * mutateMutability);
    }

    public static float RandomPower()
    {
        return Mathf.Pow(RandomSeed(), MUTATE_POWER);
    }

    public static float RandomSeed()
    {
        return Random.value * 2 - 1;
    }
}
