using InGame.Parties;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using InGame.Agents.Players;
using UniRx;
using MyUtil;
using VContainer;
using InGame.Buttles;
using InGame.Magics;

namespace InGame.Agents
{
    public class RewardProvider : ControllerBase
    {
        private PartyManager partyManager;
        private PlayerAgent playerAgent;

        private int[] beforeHPArray;

        [Inject]
        public RewardProvider(PartyManager partyManager, PlayerAgent playerAgent)
        {
            this.partyManager = partyManager;
            this.playerAgent = playerAgent;

            //beforeHPArray = partyManager.partyCharacters.Select(x => x.characterHealth.currentHP).ToArray();
            //AddRewardByHeal();
            //AddRewardByPointlessMagic();
        }

        public void AddRewardByDefence()
        {
            for(int i=0;i<partyManager.partyCharacters.Length;i++)
            {
                var character = partyManager.partyCharacters[i];
                if (!character.characterStatus.characterBuff.IsDefencing)
                    continue;

                if (character.characterHealth.currentHP == beforeHPArray[i])
                {
                    playerAgent.AddReward(-0.01f);
                }
            }

            beforeHPArray = partyManager.partyCharacters.Select(x => x.characterHealth.currentHP).ToArray();
        }

        private void AddRewardByHeal()
        {
            foreach(var character in partyManager.partyCharacters)
            {
                character.characterHealth.HealValueObservable
                    //.Where(x => x > 0)
                    .Subscribe(x =>
                    {
                        if (x > 0)
                        {
                            playerAgent.AddReward(0.01f);
                        }
                        else
                        {
                            playerAgent.AddReward(-0.01f);
                        }
                    })
                    .AddTo(this);

                character.characterMagic.HealValueObservable
                    //.Where(x => x > 0)
                    .Subscribe(x =>
                    {
                        if (x > 0)
                        {
                            playerAgent.AddReward(0.01f);
                        }
                        else
                        {
                            playerAgent.AddReward(-0.01f);
                        }
                    })
                    .AddTo(this);
            }
        }

        public void AddRewardByAttack(EnemyManager enemyManager)
        {
            foreach(var enemy in enemyManager.enemies)
            {
                enemy.AttackerObservable
                    .TakeWhile(_=>!enemyManager.HadDisposed)
                    .TakeWhile(_=>!enemy.characterHealth.IsDead)
                    .Where(x => x.Item2 > 10)
                    .Subscribe(_ =>
                    {
                        playerAgent.AddReward(0.01f);
                    })
                    .AddTo(this);
            }
        }

        public void AddRewardByPointlessMagic()
        {
            foreach(var magic in MagicDataBase.MagicDatas)
            {
                magic.PointlessActionObservable
                    .Subscribe(_ =>
                    {
                        playerAgent.AddReward(-0.01f);
                    })
                    .AddTo(this);
            }
        }
    }
}

