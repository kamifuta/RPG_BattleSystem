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

            //�����ΏۂƂȂ�L�����N�^�[���܂ރp�[�e�B�̑g�ݍ��킹�����ׂĎ擾����
            IEnumerable<IEnumerable<int>> partyCharacterIndexList = Enumerable.Range(0, characterCount).Combination(4);
            partyList.Clear();

            //���ׂẴp�[�e�B�ɑ΂��ĕ]�����s��
            foreach (var partyCharacterIndex in partyCharacterIndexList)
            {
                ExecuteBattle(partyCharacterIndex).Forget();
            }

            //���ׂẴp�[�e�B�̕]�����I������܂őҋ@
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
        /// �o�g�����J�n����
        /// </summary>
        /// <param name="partyCharacterIndex"></param>
        /// <returns></returns>
        private async UniTask ExecuteBattle(IEnumerable<int> partyCharacterIndex)
        {
            //�X�e�[�^�X����L�����N�^�[�𐶐����ăp�[�e�B�[�ɃZ�b�g
            PlayableCharacter[] partyCharacterArray = characterManager.GenerateCharacters(partyCharacterIndex).ToArray();
            var party = new Party(partyCharacterArray);
            partyList.Add(party);

            var battleController = new BattleController(playerAgentFactory, enemyFactory, partyCharacterArray, battleID);
            battleID++;

            disposables.Add(battleController);

            //�퓬���s���A�������擾����
            int winCount = 0;
            for (int j = 0; j < battleTimes; j++)
            {
                //�o�g�������s����
                battleController.Encount();

                //�o�g���I���܂őҋ@����
                await battleController.ResultObservable.ToUniTask(cancellationToken: tokenSource.Token);

                //���ׂẴL�����N�^�[�ɑ΂��ď���������
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

