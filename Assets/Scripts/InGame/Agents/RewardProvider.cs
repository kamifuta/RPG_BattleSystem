using InGame.Parties;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using InGame.Agents.Players;
using UniRx;
using MyUtil;
using VContainer;

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

            beforeHPArray = partyManager.partyCharacters.Select(x => x.characterHealth.currentHP).ToArray();
            AddRewardByHeal();
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
                    .Where(x => x == 0)
                    .Subscribe(_ =>
                    {
                        playerAgent.AddReward(-0.01f);
                    })
                    .AddTo(this);

                character.characterMagic.HealValueObservable
                    .Where(x => x == 0)
                    .Subscribe(_ =>
                    {
                        playerAgent.AddReward(-0.01f);
                    })
                    .AddTo(this);
            }
        }
    }
}

