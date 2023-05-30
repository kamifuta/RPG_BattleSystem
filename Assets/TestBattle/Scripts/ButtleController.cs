using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

namespace TestButtle
{
    public class ButtleController : MonoBehaviour
    {
        [SerializeField] private ButtleManager buttleManager;
        [SerializeField] private Agent agent;

        private int currentTurn = 0;
        public Character currentActionCharacter { get; private set; }

        public bool IsEndButtle { get; private set; }

        private Coroutine battleCoroutine=null;

        private int battleCount = 0;
        private int winCount = 0;

        public void StartButtle()
        {
            //Debug.Log("バトル開始");

            IsEndButtle = false;
            currentTurn = 0;

            buttleManager.Init();
            battleCount++;

            if (battleCoroutine != null)
                StopCoroutine(battleCoroutine);

            battleCoroutine=StartCoroutine(Buttle());
        }

        private IEnumerator Buttle()
        {
            while (true)
            {
                //ターンを進める
                currentTurn++;
                //Debug.Log($"{currentTurn}ターン目開始");

                //誰のターンであるかを決める
                if (currentActionCharacter == null)
                {
                    currentActionCharacter = buttleManager.player;
                }
                else
                {
                    currentActionCharacter = currentActionCharacter == buttleManager.player ? buttleManager.enemy : buttleManager.player;
                }

                //フラグを片づける
                currentActionCharacter.ClearFlag();

                //行動対象が敵であるなら行動してもらう
                if (currentActionCharacter == buttleManager.enemy)
                {
                    buttleManager.enemy.Attack(buttleManager.player);
                }

                //キャラが行動するまで待つ
                yield return new WaitUntil(()=>currentActionCharacter.HadDoneAction);

                //勝敗が決まったかを調べる
                if (CheckEndButtle())
                {
                    ResultButtle();
                    break;
                }

                yield return null;
            }
        }

        private bool CheckEndButtle()
        {
            if (buttleManager.player.IsDead)
            {
                Debug.Log("負け");
                agent.SetReward(-1f);
                agent.EndEpisode();
                return true;
            }

            if (buttleManager.enemy.IsDead)
            {
                winCount++;
                Debug.Log("勝ち");
                agent.SetReward(1f);
                agent.EndEpisode();
                return true;
            }

            return false;
        }

        private void ResultButtle()
        {
            Debug.Log($"勝率{(float)winCount / battleCount}");
            IsEndButtle = true;
        }
    }
}

