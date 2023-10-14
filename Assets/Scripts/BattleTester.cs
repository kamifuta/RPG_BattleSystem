using Cysharp.Threading.Tasks;
using InGame.Agents.Players;
using InGame.Buttles;
using InGame.Characters.PlayableCharacters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PCGs
{
    public class BattleTester : IStartable, IDisposable
    {
        private readonly CharacterManager characterManager;
        private readonly EnemyFactory enemyFactory;
        private readonly PlayerAgentFactory playerAgentFactory;

        private BattleController battleController;

        [Inject]
        public BattleTester(CharacterManager characterManager, EnemyFactory enemyFactory, PlayerAgentFactory playerAgentFactory)
        {
            this.characterManager = characterManager;
            this.enemyFactory = enemyFactory;
            this.playerAgentFactory = playerAgentFactory;
        }

        public void Start()
        {
            characterManager.GenerateCharacterStatuses(4);
            StartTest();
        }

        private void StartTest()
        {
            ExecuteBattle(Enumerable.Range(0, 4));
        }

        private void ExecuteBattle(IEnumerable<int> partyCharacterIndex)
        {
            //ステータスからキャラクターを生成してパーティーにセット
            PlayableCharacter[] partyCharacterArray = characterManager.GenerateCharacters(partyCharacterIndex).ToArray();

            battleController = new BattleController(playerAgentFactory, enemyFactory, partyCharacterArray, 0);
            battleController.Encount();

            //battleController.Dispose();
        }

        public void Dispose()
        {
            battleController.Dispose();
        }
    }
}

