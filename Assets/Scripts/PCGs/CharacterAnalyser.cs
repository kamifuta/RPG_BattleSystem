using InGame.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyUtil;
using System.Linq;
using Log;

namespace PCGs
{
    public class CharacterAnalyser : MonoBehaviour
    {
        private List<CharacterStatus> statusList = new List<CharacterStatus>(16);

        private void Start()
        {
            LoadJSON();

            var list = statusList.Combination(2);
            foreach(var e in list)
            {
                var pair = e.ToArray();
                var cosineSimilarity=CalcCosineSimilarity(pair[0], pair[1]);
                Debug.Log($"{statusList.IndexOf(pair[0])}:{statusList.IndexOf(pair[1])} {cosineSimilarity}");
            }
        }

        private void LoadJSON()
        {
            if (!PCGLog.CheckExistJsonFile())
            {
                Debug.Log("ステータスファイルが存在しません");
                return;
            }

            var statusJSONs = PCGLog.ReadJSONLog().Split("\n");

            for (int i = 0; i < statusJSONs.Length - 2; i++)
            {
                var json = statusJSONs[i];
                Debug.Log(json);
                var logStatus = JsonUtility.FromJson<LogStatus>(json);
                var status = new CharacterStatus(logStatus.MaxHP, logStatus.MaxMP, logStatus.AttackValue, logStatus.MagicValue, logStatus.DefenceValue, logStatus.MagicDefenceValue, logStatus.Agility);
                statusList.Add(status);
            }

            Debug.Log("ステータスの読み込みが完了しました");
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

