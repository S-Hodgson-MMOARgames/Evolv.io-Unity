// (C) MMOARgames, Inc. All Rights Reserved.

using UnityEngine;

public class Axon
{
    private const int MUTATE_POWER = 9;
    private const float MUTABILITY_MULTIPLIER = 0.7f;

    public readonly float Weight;
    private readonly float mutability;
    private readonly float mutationMultiplier;

    public Axon(float weight, float mutability)
    {
        Weight = weight;
        this.mutability = mutability;
        mutationMultiplier = Mathf.Pow(0.5f, MUTATE_POWER);
    }

    public Axon MutateAxon()
    {
        float mutateMutability = Mathf.Pow(0.5f, RandomSeed() * MUTABILITY_MULTIPLIER);
        return new Axon(Weight + RandomPower() * mutability / mutationMultiplier, mutability * mutateMutability);
    }

    private static float RandomPower()
    {
        return Mathf.Pow(RandomSeed(), MUTATE_POWER);
    }

    private static float RandomSeed()
    {
        return Random.value * 2 - 1;
    }
}
