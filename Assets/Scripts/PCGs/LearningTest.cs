using Cysharp.Threading.Tasks;
using InGame.Agents.Players;
using InGame.Buttles;
using InGame.Characters;
using InGame.Characters.PlayableCharacters;
using Log;
using MyUtil;
using PCGs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PCGs
{
    public class LearningTest : IStartable, IDisposable
    {
        private readonly CharacterManager characterManager;
        private readonly EnemyFactory enemyFactory;
        private readonly PlayerAgentFactory playerAgentFactory;

        private readonly List<Party> partyList = new List<Party>(128);
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly CompositeDisposable disposables = new CompositeDisposable(64);

        private readonly int battleTimes = 100000;
        private readonly int characterCount = 8;

        private int battleID = 0;

        private bool IsDisposed = false;

        [Inject]
        public LearningTest(CharacterManager characterManager, EnemyFactory enemyFactory, PlayerAgentFactory playerAgentFactory)
        {
            this.characterManager = characterManager;
            this.enemyFactory = enemyFactory;
            this.playerAgentFactory = playerAgentFactory;
        }

        public void Start()
        {
            StartLearning().Forget();
        }

        private async UniTaskVoid StartLearning()
        {
            characterManager.GenerateCharacterStatuses(characterCount);


            PCGLog.DeleteStatusJSONLog();
            var logStatusList = new List<LogStatus>();
            for (int i = 0; i < characterCount; i++)
            {
                var status = characterManager.PlayableCharacterStatusList[i];
                var logStatus = new LogStatus(status.baseMaxHP, status.baseMaxMP, status.baseAttackValue, status.baseMagicValue, status.baseDefenceValue, status.baseMagicDefenceValue, status.baseAgility);
                logStatusList.Add(logStatus);
            }

            var json = JsonHelper.ToJson(logStatusList);
            PCGLog.WriteStatusJSONLog(json);

            //調整対象となるキャラクターを含むパーティの組み合わせをすべて取得する
            IEnumerable<IEnumerable<int>> partyCharacterIndexList = Enumerable.Range(0, characterCount).Combination(4);
            partyList.Clear();

            //すべてのパーティに対して評価を行う
            foreach (var partyCharacterIndex in partyCharacterIndexList)
            {
                ExecuteBattle(partyCharacterIndex).Forget();
            }

            //すべてのパーティの評価が終了するまで待機
            await WaitEvaluateParties(tokenSource.Token);
            disposables.Clear();
        }

        private async UniTask WaitEvaluateParties(CancellationToken token)
        {
            while (true)
            {
                if (partyList.All(x => x.IsSimulated))
                    return;

                await UniTask.Yield(token);
            }
        }

        /// <summary>
        /// バトルを開始する
        /// </summary>
        /// <param name="partyCharacterIndex"></param>
        /// <returns></returns>
        private async UniTask ExecuteBattle(IEnumerable<int> partyCharacterIndex)
        {
            //ステータスからキャラクターを生成してパーティーにセット
            PlayableCharacter[] partyCharacterArray = characterManager.GenerateCharacters(partyCharacterIndex).ToArray();
            var party = new Party(partyCharacterArray);
            partyList.Add(party);

            var battleController = new BattleController(playerAgentFactory, enemyFactory, partyCharacterArray, battleID);
            battleID++;

            disposables.Add(battleController);

            //戦闘を行い、勝率を取得する
            int winCount = 0;
            for (int j = 0; j < battleTimes; j++)
            {
                //バトルを実行する
                battleController.Encount();

                //バトル終了まで待機する
                await battleController.ResultObservable.ToUniTask(cancellationToken: tokenSource.Token);

                //すべてのキャラクターに対して初期化する
                characterManager.SetItems(partyCharacterArray);
                for (int k = 0; k < 4; k++)
                {
                    partyCharacterArray[k].FullHeal();
                }

            }

            party.SetIsSimulated(true);

            partyCharacterArray.ForEach(x => x.Dispose());
            battleController.Dispose();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            tokenSource?.Cancel();
            tokenSource?.Dispose();

            if (!disposables.IsDisposed)
            {
                disposables.Dispose();
            }
        }
    }
}

