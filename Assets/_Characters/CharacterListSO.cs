using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu (menuName = "Blue Mana/Character List") ]
public class CharacterListSO : ScriptableObject {
    public List<Character> characters;

    public List<Character> GetRandomFromChallengeRating(int maxChallengeTotal, int maxEnemies = 7) {
        List<Character> filteredEnemies = new List<Character>();
        bool canProceed = false;
        int minChallengeRating = 1000000;
        foreach (var c in characters) {
            int thisChallengeRating = c.challengeRating;
            if (thisChallengeRating < minChallengeRating) {
                minChallengeRating = thisChallengeRating;
            }
            if (c.challengeRating <= maxChallengeTotal) {
                canProceed = true;
            }
        }
        if (!canProceed) {
            Debug.LogError("No enemies found that are below the challenge rating");
            return filteredEnemies;
        }

        int totalChallengeRating = 0;
        int maxIterations = 1000;
        int iterations = 0;
        while (filteredEnemies.Count < maxEnemies && totalChallengeRating <= maxChallengeTotal - minChallengeRating) {
            iterations++;
            if (iterations > maxIterations) {
                break;
            }
            int index = Random.Range(0, characters.Count);
            if (characters[index].challengeRating + totalChallengeRating <= maxChallengeTotal) {
                filteredEnemies.Add(Instantiate(characters[index]));
                totalChallengeRating += characters[index].challengeRating;
            }
        }
        
        return filteredEnemies;

    }
}
