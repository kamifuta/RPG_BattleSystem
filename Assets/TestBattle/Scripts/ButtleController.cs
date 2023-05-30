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
            //Debug.Log("�o�g���J�n");

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
                //�^�[����i�߂�
                currentTurn++;
                //Debug.Log($"{currentTurn}�^�[���ڊJ�n");

                //�N�̃^�[���ł��邩�����߂�
                if (currentActionCharacter == null)
                {
                    currentActionCharacter = buttleManager.player;
                }
                else
                {
                    currentActionCharacter = currentActionCharacter == buttleManager.player ? buttleManager.enemy : buttleManager.player;
                }

                //�t���O��ЂÂ���
                currentActionCharacter.ClearFlag();

                //�s���Ώۂ��G�ł���Ȃ�s�����Ă��炤
                if (currentActionCharacter == buttleManager.enemy)
                {
                    buttleManager.enemy.Attack(buttleManager.player);
                }

                //�L�������s������܂ő҂�
                yield return new WaitUntil(()=>currentActionCharacter.HadDoneAction);

                //���s�����܂������𒲂ׂ�
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
                Debug.Log("����");
                agent.SetReward(-1f);
                agent.EndEpisode();
                return true;
            }

            if (buttleManager.enemy.IsDead)
            {
                winCount++;
                Debug.Log("����");
                agent.SetReward(1f);
                agent.EndEpisode();
                return true;
            }

            return false;
        }

        private void ResultButtle()
        {
            Debug.Log($"����{(float)winCount / battleCount}");
            IsEndButtle = true;
        }
    }
}

