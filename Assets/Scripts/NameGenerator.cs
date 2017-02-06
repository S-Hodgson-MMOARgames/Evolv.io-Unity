// (C) MMOARgames, Inc. All Rights Reserved.

using UnityEngine;
using Random = UnityEngine.Random;

public class NameGenerator
{
    private const int MIN_NAME_LENGTH = 3;
    private const int MAX_NAME_LENGTH = 10;

    private static readonly float[] LetterFrequencies =
    {
        8.167f, 1.492f, 2.782f, 4.253f, 12.702f, 2.228f, 2.015f, 6.094f, 6.966f,
        0.153f, 0.772f, 4.025f, 2.406f, 6.749f, 7.507f, 1.929f, 0.095f, 5.987f,
        6.327f, 9.056f, 2.758f, 0.978f, 2.361f, 0.150f, 1.974f, 0.074f
    };

    public static string NewName()
    {
        string nameSoFar = string.Empty;
        int chosenLength = Random.Range(MIN_NAME_LENGTH, MAX_NAME_LENGTH);

        if (chosenLength < 3)
        {
            Debug.LogError("Name Genergation");
            chosenLength = MAX_NAME_LENGTH;
        }

        for (int i = 0; i < chosenLength; i++)
        {
            nameSoFar += GetRandomChar().ToString();
        }

        return SanitizeName(nameSoFar);
    }

    public static string MutateName(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            Debug.LogWarning("Tried to mutate an empty string!");
            return NewName();
        }

        if (input.Length >= 3)
        {
            if (Random.value < 0.2f)
            {
                int removeIndex = Random.Range(0, input.Length);
                input = input.Substring(0, removeIndex) + input.Substring(removeIndex + 1, input.Length - (removeIndex + 1));
            }
        }

        if (input.Length <= 9)
        {
            if (Random.value < 0.2f)
            {
                int insertIndex = Random.Range(0, input.Length + 1);
                input = input.Substring(0, insertIndex) + GetRandomChar().ToString() + input.Substring(insertIndex, input.Length - insertIndex);
            }
        }

        int changeIndex = Random.Range(0, input.Length);
        input = input.Substring(0, changeIndex) + GetRandomChar().ToString() + input.Substring(changeIndex, input.Length - changeIndex);

        return input;
    }

    public static string CombineNames(string[] input)
    {
        string output = string.Empty;

        for (int i = 0; i < input.Length; i++)
        {
            float portion = (float)input[i].Length / input.Length;
            float start = Mathf.Min(Mathf.Max(Mathf.Round(portion * i), 0), input[i].Length);
            float end = Mathf.Min(Mathf.Max(Mathf.Round(portion * (i + 1)), 0), input[i].Length);

            output = output + input[i].Substring((int)start, (int)(end - start));
        }

        return output;
    }

    private static char GetRandomChar()
    {
        float letterFactor = Random.Range(0, 100);
        int letterChoice = 0;

        while (letterFactor > 0)
        {
            letterFactor -= LetterFrequencies[letterChoice];
            letterChoice++;
        }

        return (char)(letterChoice + 96);
    }

    public static string SanitizeName(string input)
    {
        string output = string.Empty;
        int vowelSoFar = 0;
        int consonatsSoFar = 0;

        for (int i = 0; i < input.Length; i++)
        {
            char ch = input[i];
            if (IsVowel(ch))
            {
                vowelSoFar++;
                consonatsSoFar = 0;
            }
            else
            {
                vowelSoFar = 0;
                consonatsSoFar++;
            }

            if (vowelSoFar <= 2 && consonatsSoFar <= 2)
            {
                output += ch.ToString();
            }
            else
            {
                float chanceOfAddingChar;

                if (input.Length <= MIN_NAME_LENGTH)
                {
                    chanceOfAddingChar = 1.0f;
                }
                else if (input.Length >= MAX_NAME_LENGTH)
                {
                    chanceOfAddingChar = 0.0f;
                }
                else
                {
                    chanceOfAddingChar = 0.5f;
                }

                if (Random.value < chanceOfAddingChar)
                {
                    char extraChar = ' ';
                    while (extraChar == ' ' || IsVowel(ch) == IsVowel(extraChar))
                    {
                        extraChar = GetRandomChar();
                    }

                    output = output + extraChar.ToString() + ch.ToString();

                    if (IsVowel(ch))
                    {
                        consonatsSoFar = 0;
                        vowelSoFar = 1;
                    }
                    else
                    {
                        consonatsSoFar = 1;
                        vowelSoFar = 0;
                    }
                }
            }
        }

        return output;
    }

    private static bool IsVowel(char input)
    {
        return input == 'a' || input == 'e' || input == 'i' || input == 'o' || input == 'u' || input == 'y';
    }
}
