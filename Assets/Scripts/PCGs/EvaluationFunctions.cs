using InGame.Characters.PlayableCharacters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCGs
{
    public class EvaluationFunctions
    {
        private const float OptimalSynergyPoint = 12f;
        private const float Sigma1= 3.5f;
        private const float Sigma2= 7.5f;

        private CharacterManager characterManager;

        public float EvaluateCharacter(float synergyPoint)
        {
            if(0<=synergyPoint && synergyPoint <= OptimalSynergyPoint)
            {
                Mathf.Exp(-(Mathf.Pow((synergyPoint - OptimalSynergyPoint), 2) / (2 * Mathf.Pow(Sigma1, 2))));
            }
            else if (OptimalSynergyPoint < synergyPoint)
            {
                Mathf.Exp(-(Mathf.Pow((synergyPoint - OptimalSynergyPoint), 2) / (2 * Mathf.Pow(Sigma2, 2))));
            }

            return 0f;
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

        //public float EvalkuateParameterDistance()
        //{

        //}

        //public float PenaltyForStrongParty()
        //{

        //}

        //public float PenaltyForStrongCharacter()
        //{

        //}
    }
}

