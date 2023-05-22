using InGame.Buttles.Actions;
using InGame.Parties;
using MyUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace InGame.Buttles.PlayerAIs
{
    /// <summary>
    /// ��{�I�ɒʏ�U�������āA�̗͂��Ⴍ�Ȃ�����m���Ŗh�䂷��
    /// �^�[�Q�b�g�̓����_��
    /// </summary>
    public class NormalAttackAndDefenceAI : PlayerAI
    {
        [Inject]
        public NormalAttackAndDefenceAI(PartyManager partyManager) : base(partyManager)
        {

        }

        public override void SelectCharacterAction()
        {
            foreach(var player in partyManager.partyCharacters)
            {
                if (player.characterHealth.IsDead)
                    continue;

                if ((float)player.characterHealth.currentHP / player.characterStatus.MaxHP < 0.2f && Random.value < 0.5f)
                {
                    var action = new ActionData(BaseActionType.Defence, player);
                    playableCharacterActionManager.SetPlayableCharacterAction(player, action);
                }
                else
                {
                    var target = enemyManager.enemies.RandomGet();
                    var action = new ActionData(BaseActionType.NormalAttack, player, target);
                    playableCharacterActionManager.SetPlayableCharacterAction(player, action);
                }
            }
        }
    }
}

