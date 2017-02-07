// (C) MMOARgames, Inc. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

public class Brain
{
    public const int NUMBER_OF_INPUTS = 11;
    private const int MEMORY_COUNT = 1;
    private const int BRAIN_WIDTH = 3;
    private const int BRAIN_HEIGHT = NUMBER_OF_INPUTS + MEMORY_COUNT + 1;

    private const float AXON_START_MUTABILITY = 0.0005f;
    private const float STARTING_AXON_VARIABLILITY = 1.0f;

    private Axon[,,] axons= new Axon[BRAIN_WIDTH - 1, BRAIN_HEIGHT, BRAIN_HEIGHT - 1];
    private float[,] neurons = new float[BRAIN_WIDTH, BRAIN_HEIGHT];
    private float[] output = new float[NUMBER_OF_INPUTS];

    public Brain(Axon[,,] tbrain, float[,] tneurons)
    {
        if (tbrain == null)
        {
            for (int x = 0; x < BRAIN_WIDTH - 1; x++)
            {
                for (int y = 0; y < BRAIN_HEIGHT; y++)
                {
                    for (int z = 0; z < BRAIN_HEIGHT - 1; z++)
                    {
                        float startingWeight = (Random.value * 2 - 1) * STARTING_AXON_VARIABLILITY;
                        axons[x, y, z] = new Axon(startingWeight, AXON_START_MUTABILITY);
                    }
                }
            }

            for (int x = 0; x < BRAIN_WIDTH; x++)
            {
                for (int y = 0; y < BRAIN_HEIGHT; y++)
                {
                    if (y == BRAIN_HEIGHT - 1)
                    {
                        neurons[x, y] = 1;
                    }
                    else
                    {
                        neurons[x, y] = 0;
                    }
                }
            }
        }
        else
        {
            axons = tbrain;
            neurons = tneurons;
        }
    }

    public static Brain Evolve(List<Creature> parents)
    {
        int parentsTotal = parents.Count;
        var newBrain = new Axon[BRAIN_WIDTH - 1, BRAIN_HEIGHT, BRAIN_HEIGHT - 1];
        var newNeurons = new float[BRAIN_WIDTH, BRAIN_HEIGHT];

        float randomParentRotation = Random.value;

        for (int x = 0; x < BRAIN_WIDTH - 1; x++)
        {
            for (int y = 0; y < BRAIN_HEIGHT; y++)
            {
                for (int z = 0; z < BRAIN_HEIGHT - 1; z++)
                {
                    newBrain[x, y, z] = parents[(int)((Mathf.Atan2((y + z) / 2 - BRAIN_HEIGHT / 2, x - BRAIN_WIDTH / 2) / (2 * Mathf.PI) + Mathf.PI + randomParentRotation) % 1.0f) * parentsTotal].CreatureBrain.axons[x, y, z].MutateAxon();
                }
            }
        }

        for (int x = 0; x < BRAIN_WIDTH; x++)
        {
            for (int y = 0; y < BRAIN_HEIGHT; y++)
            {
                newNeurons[x, y] = parents[(int)((Mathf.Atan2(y - BRAIN_HEIGHT / 2, x - BRAIN_WIDTH / 2) / (2 * Mathf.PI) + Mathf.PI + randomParentRotation) % 1.0f * parentsTotal)].CreatureBrain.neurons[x, y];
            }
        }

        return new Brain(newBrain, newNeurons);
    }

    public void Input(float[] inputs)
    {
        for (int i = 0; i < NUMBER_OF_INPUTS; i++)
        {
            neurons[0, i] = inputs[i];
        }

        for (int i = 0; i < MEMORY_COUNT; i++)
        {
            neurons[0, NUMBER_OF_INPUTS + i] = neurons[BRAIN_WIDTH - 1, NUMBER_OF_INPUTS + i];
        }

        neurons[0, BRAIN_HEIGHT - 1] = 1;

        for (int x = 1; x < BRAIN_WIDTH; x++)
        {
            for (int y = 0; y < BRAIN_HEIGHT - 1; y++)
            {
                float total = 0;

                for (int i = 0; i < BRAIN_HEIGHT; i++)
                {
                    float neuron = neurons[x - 1, i];
                    float axonWeight = axons[x - 1, i, y].Weight;

                    total += neuron * axonWeight;
                }

                if (x == BRAIN_WIDTH - 1)
                {
                    neurons[x, y] = total;
                }
                else
                {
                    neurons[x, y] = Sigmoid(total);
                }
            }
        }
    }

    public float[] Outputs()
    {
        for (int i = 0; i < NUMBER_OF_INPUTS; i++)
        {
            output[i] = neurons[BRAIN_WIDTH - 1, i];
        }

        return output;
    }

    private static float Sigmoid(float input)
    {
        return 1.0f / (1.0f + Mathf.Pow(MathUtils.EULERS_NUMBER, -input));
    }
}
