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
        private readonly List<CharacterStatus> statusList = new List<CharacterStatus>(16);
        private readonly CharacterManager characterManager;
        private readonly EnemyFactory enemyFactory;
        private readonly PlayerAgentFactory playerAgentFactory;

        private readonly EvaluationFunctions evaluationFunctions;
        private readonly List<Party> partyList = new List<Party>(128);
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly CompositeDisposable disposables = new CompositeDisposable(64);

        private readonly int battleTimes = 200;
        private int characterCount = 8;

        private int battleID = 0;

        private bool IsDisposed = false;

        [Inject]
        public CharacterAnalyser(CharacterManager characterManager, EnemyFactory enemyFactory, PlayerAgentFactory playerAgentFactory, CharacterStatusData characterStatusData)
        {
            this.characterManager = characterManager;
            this.enemyFactory = enemyFactory;
            this.playerAgentFactory = playerAgentFactory;

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
                Debug.Log("ステータスファイルが存在しません");
                return;
            }

            var statusJSONs = PCGLog.ReadStatusJSONLog();
            LogStatus[] logStatusArray = JsonHelper.FromJson<LogStatus>(statusJSONs);
            foreach(var logStatus in logStatusArray)
            {
                var status = new CharacterStatus(logStatus.MaxHP, logStatus.MaxMP, logStatus.AttackValue, logStatus.MagicValue, logStatus.DefenceValue, logStatus.MagicDefenceValue, logStatus.Agility);
                statusList.Add(status);
            }

            characterCount = statusList.Count;
            Debug.Log("ステータスの読み込みが完了しました");

            characterManager.SetStatusList(statusList);
        }

        private async UniTaskVoid StartAnalyze()
        {
            for (int i = 0; i < statusList.Count; i++)
            {
                var analyzedStatus = statusList[i];

                //分析対象となるキャラクターを含むパーティの組み合わせをすべて取得する
                IEnumerable<IEnumerable<int>> partyCharacterIndexList = Enumerable.Range(0, characterCount).Combination(i, 4);
                partyList.Clear();

                //すべてのパーティに対して分析を行う
                foreach (var partyCharacterIndex in partyCharacterIndexList)
                {
                    ExecuteBattle(partyCharacterIndex).Forget();
                }

                await WaitEvaluateParties(tokenSource.Token);

                float allWinningRateAverage = partyList.Select(x => x.winningParcentage).Average();
                float synergy = evaluationFunctions.EvaluateSynergy(partyList);
                List<float> distanceList = new List<float>();
                foreach (var status in statusList)
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
                battleController.ResultObservable
                    .Take(1)
                    .Where(result => result == BattleController.ResultType.Win)
                    .Subscribe(result =>
                    {
                        winCount++;
                    });

                //バトル終了まで待機する
                await battleController.ResultObservable.ToUniTask(cancellationToken: tokenSource.Token);

                //すべてのキャラクターに対して初期化する
                characterManager.SetItems(partyCharacterArray);
                for (int k = 0; k < 4; k++)
                {
                    partyCharacterArray[k].FullHeal();
                }

            }

            //勝率を計算してパーティでのシミュレーションを終了
            party.SetWinningParcentage((float)winCount / battleTimes);
            Debug.Log($"勝率:{party.winningParcentage.ToString()}");
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

        private float CalcVectorSize(NormalizedStatus status)
        {
            float squaredHPDifference = Mathf.Pow(status.HP, 2);
            float squaredMPDifference = Mathf.Pow(status.MP, 2);
            float squaredAttackDifference = Mathf.Pow(status.Attack, 2);
            float squaredMagicDifference = Mathf.Pow(status.Magic, 2);
            float squaredDefenceDifference = Mathf.Pow(status.Defence, 2);
            float squaredMagicDefenceDifference = Mathf.Pow(status.MagicDefence, 2);
            float squaredAgilityDifference = Mathf.Pow(status.Agility, 2);

            var vectorSize = Mathf.Pow(squaredHPDifference + squaredMPDifference + squaredAttackDifference + squaredMagicDifference + squaredDefenceDifference + squaredMagicDefenceDifference + squaredAgilityDifference, 1f / 2f);
            return vectorSize;
        }

        private float CalcInnerProduct(NormalizedStatus status1, NormalizedStatus status2)
        {
            float hp = status1.HP * status2.HP;
            float mp = status1.MP * status2.MP;
            float attack = status1.Attack * status2.Attack;
            float magic = status1.Magic * status2.Magic;
            float defence = status1.Defence * status2.Defence;
            float magicDefence = status1.MagicDefence * status2.MagicDefence;
            float agility = status1.Agility * status2.Agility;

            var innerProduct = hp + mp + attack + magic + defence + magicDefence + agility;
            return innerProduct;
        }

        private float CalcCosineSimilarity(CharacterStatus status1, CharacterStatus status2)
        {
            NormalizedStatus normalizedStatus1 = EvaluationFunctions.GetNormalizedStatus(status1);
            NormalizedStatus normalizedStatus2 = EvaluationFunctions.GetNormalizedStatus(status2);

            return CalcInnerProduct(normalizedStatus1, normalizedStatus2) / (CalcVectorSize(normalizedStatus1) * CalcVectorSize(normalizedStatus2));
        }
    }
}

