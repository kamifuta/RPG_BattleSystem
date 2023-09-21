using InGame.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyUtil;
using System.Linq;

namespace PCGs
{
    public class CharacterAnalyser : MonoBehaviour
    {
        private List<CharacterStatus> statusList = new List<CharacterStatus>(16);

        private void Start()
        {
            statusList.Add(new CharacterStatus(302, 492, 79, 447, 225, 343, 115));
            statusList.Add(new CharacterStatus(179, 481, 212, 235, 296, 378, 155));
            statusList.Add(new CharacterStatus(601, 112, 266, 331, 234, 386, 85));
            statusList.Add(new CharacterStatus(219, 156, 627, 225, 83, 197, 175));
            statusList.Add(new CharacterStatus(172, 131, 230, 140, 387, 466, 119));
            statusList.Add(new CharacterStatus(551, 232, 279, 118, 104, 141, 93));
            statusList.Add(new CharacterStatus(631, 187, 96, 330, 562, 103, 100));
            statusList.Add(new CharacterStatus(499, 356, 585, 178, 253, 375, 56));
            //statusList.Add(new CharacterStatus(328,345,78,401,98,303,150));
            //statusList.Add(new CharacterStatus(230,111,179,219,463,128,228));

            var list = statusList.Combination(2);
            foreach(var e in list)
            {
                var pair = e.ToArray();
                var cosineSimilarity=CalcCosineSimilarity(pair[0], pair[1]);
                Debug.Log($"{statusList.IndexOf(pair[0])}:{statusList.IndexOf(pair[1])} {cosineSimilarity}");
            }

            Debug.Log(CalcCosineSimilarity(statusList[0], statusList[0]));
        }

        private float CalcVectorSize(CharacterStatus status)
        {
            float squaredHPDifference = Mathf.Pow(status.MaxHP, 2);
            float squaredMPDifference = Mathf.Pow(status.MaxMP, 2);
            float squaredAttackDifference = Mathf.Pow(status.AttackValue, 2);
            float squaredMagicDifference = Mathf.Pow(status.MagicValue, 2);
            float squaredDefenceDifference = Mathf.Pow(status.DefecnceValue, 2);
            float squaredMagicDefenceDifference = Mathf.Pow(status.MagicDefecnceValue, 2);
            float squaredAgilityDifference = Mathf.Pow(status.Agility, 2);

            var vectorSize = Mathf.Pow(squaredHPDifference + squaredMPDifference + squaredAttackDifference + squaredMagicDifference + squaredDefenceDifference + squaredMagicDefenceDifference + squaredAgilityDifference, 1f / 2f);
            return vectorSize;
        }

        private float CalcInnerProduct(CharacterStatus status1, CharacterStatus status2)
        {
            float hp = status1.MaxHP * status2.MaxHP;
            float mp = status1.MaxMP * status2.MaxMP;
            float attack = status1.AttackValue * status2.AttackValue;
            float magic = status1.MagicValue * status2.MagicValue;
            float defence = status1.DefecnceValue * status2.DefecnceValue;
            float magicDefence = status1.MagicDefecnceValue * status2.MagicDefecnceValue;
            float agility = status1.Agility * status2.Agility;

            var innerProduct = hp + mp + attack + magic + defence + magicDefence + agility;
            return innerProduct;
        }

        private float CalcCosineSimilarity(CharacterStatus status1, CharacterStatus status2)
        {
            return CalcInnerProduct(status1, status2) / (CalcVectorSize(status1) * CalcVectorSize(status2));
        }
    }
}

