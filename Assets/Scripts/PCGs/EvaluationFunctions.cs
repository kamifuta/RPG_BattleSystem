using InGame.Characters.PlayableCharacters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PCGs
{
    public class EvaluationFunctions
    {
        private const float OptimalSynergyPoint = 12f;
        private const float Sigma1= 3.5f;
        private const float Sigma2= 7.5f;

        private const float beta = 10f;
        private const float gamma = 0.5f;
        private const float delta = 10;

        //private CharacterManager characterManager;

        public float EvaluateCharacter(float synergyPoint, float distance, float penaltyParty, float penaltyCharacter)
        {
            float synagy = 0f;
            if(0<=synergyPoint && synergyPoint <= OptimalSynergyPoint)
            {
                synagy = Mathf.Exp(-(Mathf.Pow((synergyPoint - OptimalSynergyPoint), 2) / (2 * Mathf.Pow(Sigma1, 2))));
            }
            else if (OptimalSynergyPoint < synergyPoint)
            {
                synagy = Mathf.Exp(-(Mathf.Pow((synergyPoint - OptimalSynergyPoint), 2) / (2 * Mathf.Pow(Sigma2, 2))));
            }

            var evaluate = synagy + beta * distance + gamma * penaltyParty + delta * penaltyCharacter;
            return evaluate;
        }

        public float EvaluateSynergy(IEnumerable<Party> parties)
        {
            float synergyPoint = 0f;

            foreach(var party in parties)
            {
                synergyPoint += CalcSynergyPoint(party.winningParcentage);
            }

            return synergyPoint;
        }

        public float CalcSynergyPoint(float winningParcentage)
        {
            if ((0f <= winningParcentage && winningParcentage <= 0.45f) || winningParcentage > 0.75f)
            {
                return 0f;
            }
            else if (0.45f < winningParcentage && winningParcentage <= 0.55f)
            {
                return 10 * winningParcentage - 4.5f;
            }
            else if (0.55f < winningParcentage && winningParcentage <= 0.65f)
            {
                return 1;
            }
            else if (0.65f < winningParcentage && winningParcentage <= 0.75f)
            {
                return -10 * winningParcentage + 7.5f;
            }

            return 0f;
        }

        public float EvaluateParameterDistance(IEnumerable<PlayableCharacter> playableCharacters, PlayableCharacter evaluatedCharacter)
        {
            var sumDistance = 0f;
            foreach(var character in playableCharacters)
            {
                if (character == evaluatedCharacter)
                {
                    continue;
                }

                var distance = CalcParameterDistance(character, evaluatedCharacter);
                sumDistance += 1f / Mathf.Pow(distance, 2);
            }
            return 1f / sumDistance;
        }

        private float CalcParameterDistance(PlayableCharacter character1, PlayableCharacter character2)
        {
            var status1 = character1.characterStatus;
            var status2 = character2.characterStatus;

            float squaredHPDifference = Mathf.Pow(status1.baseMaxHP - status2.baseMaxHP, 2);
            float squaredMPDifference = Mathf.Pow(status1.baseMaxMP - status2.baseMaxMP, 2);
            float squaredAttackDifference = Mathf.Pow(status1.baseAttackValue - status2.baseAttackValue, 2);
            float squaredMagicDifference = Mathf.Pow(status1.baseMagicValue - status2.baseMagicValue, 2);
            float squaredDefenceDifference = Mathf.Pow(status1.baseDefenceValue - status2.baseDefenceValue, 2);
            float squaredMagicDefenceDifference = Mathf.Pow(status1.baseMagicDefenceValue - status2.baseMagicDefenceValue, 2);
            float squaredAgilityDifference = Mathf.Pow(status1.baseAgility - status2.baseAgility, 2);

            var distance = Mathf.Pow(squaredHPDifference + squaredMPDifference + squaredAttackDifference + squaredMagicDifference + squaredDefenceDifference + squaredMagicDefenceDifference + squaredAgilityDifference, 1f/2f);
            return distance;
        }

        public float PenaltyForStrongParty(IEnumerable<Party> parties)
        {
            var evaluate = 0f;
            foreach(var party in parties)
            {
                evaluate += Mathf.Max(party.winningParcentage - 0.7f, 0);
            }
            return evaluate;
        }

        public float PenaltyForStrongCharacter(IEnumerable<Party> parties)
        {
            var characterWinnningParcentage = Sum(parties.Select(x => x.winningParcentage)) / parties.Count();
            return Mathf.Max(characterWinnningParcentage - 0.5f, 0f);
        }

        public float Sum(IEnumerable<float> list)
        {
            var result = 0f;
            foreach(var value in list)
            {
                result += value;
            }
            return result;
        }
    }
}

