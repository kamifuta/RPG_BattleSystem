using InGame.Characters.PlayableCharacters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using InGame.Characters;
using MyUtil;

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

        private CharacterStatusData statusData;
        private static CharacterStatusData StatusData;

        public EvaluationFunctions(CharacterStatusData statusData)
        {
            this.statusData = statusData;

            StatusData = statusData;
        }

        public float EvaluateCharacter(float synergyPoint, float distance, float penaltyParty, float penaltyCharacter, float penaltyLongBattle)
        {
            float synagy = 0f;
            if(0 <= synergyPoint && synergyPoint <= OptimalSynergyPoint)
            {
                synagy = Mathf.Exp(-(Mathf.Pow((synergyPoint - OptimalSynergyPoint), 2) / (2 * Mathf.Pow(Sigma1, 2))));
            }
            else if (OptimalSynergyPoint < synergyPoint)
            {
                synagy = Mathf.Exp(-(Mathf.Pow((synergyPoint - OptimalSynergyPoint), 2) / (2 * Mathf.Pow(Sigma2, 2))));
            }

            var evaluate = synagy + (beta * distance) - (gamma * penaltyParty) - (delta * penaltyCharacter) - penaltyLongBattle;
            return evaluate;
        }

        public float EvaluateSynergy(IEnumerable<Party> parties)
        {
            float synergyPoint = 0f;

            foreach (var party in parties)
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
                return (10 * winningParcentage) - 4.5f;
            }
            else if (0.55f < winningParcentage && winningParcentage <= 0.65f)
            {
                return 1;
            }
            else if (0.65f < winningParcentage && winningParcentage <= 0.75f)
            {
                return (-10 * winningParcentage) + 7.5f;
            }

            return 0f;
        }

        public float EvaluateParameterDistance(IEnumerable<CharacterStatus> playableCharacterStatuses, CharacterStatus evaluatedCharacterStatus)
        {
            var sumDistance = 0f;
            foreach(var status in playableCharacterStatuses)
            {
                if (status == evaluatedCharacterStatus)
                {
                    continue;
                }

                var distance = CalcParameterDistance(status, evaluatedCharacterStatus);
                sumDistance += 1f / Mathf.Pow(distance, 2);
            }
            return 1f / sumDistance;
        }

        public float CalcParameterDistance(CharacterStatus status1, CharacterStatus status2)
        {
            var normalizedStatus1 = GetNormalizedStatus(status1);
            var normalizedStatus2 = GetNormalizedStatus(status2);

            float squaredHPDifference = Mathf.Pow(normalizedStatus1.HP - normalizedStatus2.HP, 2);
            float squaredMPDifference = Mathf.Pow(normalizedStatus1.MP - normalizedStatus2.MP, 2);
            float squaredAttackDifference = Mathf.Pow(normalizedStatus1.Attack - normalizedStatus2.Attack, 2);
            float squaredMagicDifference = Mathf.Pow(normalizedStatus1.Magic - normalizedStatus2.Magic, 2);
            float squaredDefenceDifference = Mathf.Pow(normalizedStatus1.Defence - normalizedStatus2.Defence, 2);
            float squaredMagicDefenceDifference = Mathf.Pow(normalizedStatus1.MagicDefence - normalizedStatus2.MagicDefence, 2);
            float squaredAgilityDifference = Mathf.Pow(normalizedStatus1.Agility - normalizedStatus2.Agility, 2);

            var distance = Mathf.Pow(squaredHPDifference + squaredMPDifference + squaredAttackDifference + squaredMagicDifference + squaredDefenceDifference + squaredMagicDefenceDifference + squaredAgilityDifference, 1f/2f);
            return distance;
        }

        public static NormalizedStatus GetNormalizedStatus(CharacterStatus characterStatus)
        {
            float hp = Calculator.CalcNormalizedValue(StatusData.maxHP_min, StatusData.maxHP_max, characterStatus.baseMaxHP);
            float mp = Calculator.CalcNormalizedValue(StatusData.maxMP_min, StatusData.maxMP_max, characterStatus.baseMaxMP);
            float attack = Calculator.CalcNormalizedValue(StatusData.attackValue_min, StatusData.attackValue_max, characterStatus.baseAttackValue);
            float magic = Calculator.CalcNormalizedValue(StatusData.magicValue_min, StatusData.magicValue_max, characterStatus.baseMagicValue);
            float defence = Calculator.CalcNormalizedValue(StatusData.defenceValue_min, StatusData.defenceValue_max, characterStatus.baseDefenceValue);
            float magicDefence = Calculator.CalcNormalizedValue(StatusData.magicDefenceValue_min, StatusData.magicDefenceValue_max, characterStatus.baseMagicDefenceValue);
            float agility = Calculator.CalcNormalizedValue(StatusData.agility_min, StatusData.agility_max, characterStatus.baseAgility);

            var status = new NormalizedStatus(hp, mp, attack, magic, defence, magicDefence, agility);
            return status;
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

        public float PenaltyForLongBattle(IEnumerable<Party> parties)
        {
            return parties.Count(x => x.HadSuspended) * 0.5f;
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

