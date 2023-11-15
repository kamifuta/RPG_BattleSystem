using InGame.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyUtil;
using System.Linq;
using Log;
using Cysharp.Threading.Tasks;
using VContainer;
using System.Threading;
using UniRx;
using InGame.Buttles;
using InGame.Agents.Players;
using VContainer.Unity;
using System;
using InGame.Characters.PlayableCharacters;

namespace PCGs
{
    public class CharacterAnalyser : IStartable, IDisposable
    {
        private List<CharacterStatus> statusList = new List<CharacterStatus>(16);
        private readonly CharacterManager characterManager;
        private readonly EnemyFactory enemyFactory;
        private readonly PlayerAgentFactory playerAgentFactory;

        private EvaluationFunctions evaluationFunctions;
        private List<Party> partyList = new List<Party>(128);
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private CompositeDisposable disposables = new CompositeDisposable(64);

        private readonly int battleTimes = 200;
        private int characterCount = 8;

        private int battleID = 0;
        private int searchCount = 0;

        private bool IsDisposed = false;

        private List<float> evaluatedValueList = new List<float>(1024);
        private CharacterStatusData characterStatusData;

        [Inject]
        public CharacterAnalyser(CharacterManager characterManager, EnemyFactory enemyFactory, PlayerAgentFactory playerAgentFactory, CharacterStatusData characterStatusData)
        {
            this.characterManager = characterManager;
            this.enemyFactory = enemyFactory;
            this.playerAgentFactory = playerAgentFactory;
            this.characterStatusData = characterStatusData;

            evaluationFunctions = new EvaluationFunctions(characterStatusData);
        }

        public void Start()
        {
            LoadJSON();
            StartAnalyze().Forget();
        }

        private void LoadJSON()
        {
            if (!PCGLog.CheckExistJsonFile())
            {
                Debug.Log("�X�e�[�^�X�t�@�C�������݂��܂���");
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

            characterCount = statusList.Count;
            Debug.Log("�X�e�[�^�X�̓ǂݍ��݂��������܂���");
        }

        private async UniTaskVoid StartAnalyze()
        {
            for (int i = 0; i < statusList.Count; i++)
            {
                var analyzedStatus = statusList[i];

                //���͑ΏۂƂȂ�L�����N�^�[���܂ރp�[�e�B�̑g�ݍ��킹�����ׂĎ擾����
                IEnumerable<IEnumerable<int>> partyCharacterIndexList = Enumerable.Range(0, characterCount).Combination(i, 4);
                partyList.Clear();

                //���ׂẴp�[�e�B�ɑ΂��ĕ��͂��s��
                foreach (var partyCharacterIndex in partyCharacterIndexList)
                {
                    ExecuteBattle(partyCharacterIndex).Forget();
                }

                await WaitEvaluateParties(tokenSource.Token);

                float allWinningRateAverage = partyList.Select(x => x.winningParcentage).Average();
                float synergy = evaluationFunctions.EvaluateSynergy(partyList);
                List<float> distanceList = new List<float>();
                foreach(var status in statusList)
                {
                    if (status == analyzedStatus)
                        continue;

                    var distance = evaluationFunctions.CalcParameterDistance(analyzedStatus, status);
                    distanceList.Add(distance);
                }
                float distanceAverage = distanceList.Average();

                PCGLog.WriteAnalyzeCSV(allWinningRateAverage, distanceAverage, synergy);

                disposables.Clear();
            }

            var list = statusList.Combination(2);
            foreach (var e in list)
            {
                var pair = e.ToArray();
                var cosineSimilarity = CalcCosineSimilarity(pair[0], pair[1]);
                PCGLog.WriteCosineSimilarity(statusList.IndexOf(pair[0]), statusList.IndexOf(pair[1]), cosineSimilarity);
            }
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
                battleController.ResultObservable
                    .Take(1)
                    .Where(result => result == BattleController.ResultType.Win)
                    .Subscribe(result =>
                    {
                        winCount++;
                    });

                //�o�g���I���܂őҋ@����
                await battleController.ResultObservable.ToUniTask(cancellationToken: tokenSource.Token);

                //���ׂẴL�����N�^�[�ɑ΂��ď���������
                characterManager.SetItems(partyCharacterArray);
                for (int k = 0; k < 4; k++)
                {
                    partyCharacterArray[k].FullHeal();
                }

            }

            //�������v�Z���ăp�[�e�B�ł̃V�~�����[�V�������I��
            party.SetWinningParcentage((float)winCount / battleTimes);
            Debug.Log($"����:{party.winningParcentage.ToString()}");
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

        private float CalcVectorSize(CharacterStatus status)
        {
            float squaredHPDifference = Mathf.Pow(status.MaxHP, 2);
            float squaredMPDifference = Mathf.Pow(status.MaxMP, 2);
            float squaredAttackDifference = Mathf.Pow(status.AttackValue, 2);
            float squaredMagicDifference = Mathf.Pow(status.MagicValue, 2);
            float squaredDefenceDifference = Mathf.Pow(status.DefenceValue, 2);
            float squaredMagicDefenceDifference = Mathf.Pow(status.MagicDefenceValue, 2);
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
            float defence = status1.DefenceValue * status2.DefenceValue;
            float magicDefence = status1.MagicDefenceValue * status2.MagicDefenceValue;
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

