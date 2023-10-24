using System.Collections.Generic;
using UnityEngine;
using MyUtil;
using System.Linq;
using InGame.Characters;
using InGame.Characters.PlayableCharacters;
using InGame.Buttles;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Random = UnityEngine.Random;
using UniRx;
using VContainer;
using VContainer.Unity;
using Log;
using System.Text;
using InGame.Agents.Players;
using UnityEngine.SceneManagement;

namespace PCGs
{
    public class ParameterSearcher : IStartable, IDisposable
    {
        private readonly CharacterManager characterManager;
        private readonly EnemyFactory enemyFactory;
        private readonly PlayerAgentFactory playerAgentFactory;

        private EvaluationFunctions evaluationFunctions=new EvaluationFunctions();
        private List<Party> partyList = new List<Party>(128);
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private CompositeDisposable disposables = new CompositeDisposable(64);

        private readonly int searchTimes = 1000;
        private readonly int battleTimes = 10;
        private readonly int characterCount = 8;

        private int battleID = 0;
        private int searchCount = 0;

        private bool IsDisposed = false;

        [Inject]
        public ParameterSearcher(CharacterManager characterManager, EnemyFactory enemyFactory, PlayerAgentFactory playerAgentFactory)
        {
            this.characterManager = characterManager;
            this.enemyFactory = enemyFactory;
            this.playerAgentFactory = playerAgentFactory;
        }

        public void Start()
        {
            LoadJSON();
            StartSearch().Forget();
        }

        private void LoadJSON()
        {
            if (!PCGLog.CheckExistJsonFile())
            {
                Debug.Log("ステータスファイルが存在しません");
                //プレイヤーキャラクターのステータスを生成する
                characterManager.GenerateCharacterStatuses(characterCount);
                LogParameter();
                return;
            }

            characterManager.ClearStatusList();
            var statusJSONs = PCGLog.ReadJSONLog().Split("\n");

            for (int i = 0; i < characterCount; i++)
            {
                var json = statusJSONs[i];
                var logStatus = JsonUtility.FromJson<LogStatus>(json);
                var status = new CharacterStatus(logStatus.MaxHP, logStatus.MaxMP, logStatus.AttackValue, logStatus.MagicValue, logStatus.DefenceValue, logStatus.MagicDefenceValue, logStatus.Agility);
                characterManager.AddNewCharacterStatus(status);
            }

            searchCount=Int32.Parse(statusJSONs[characterCount]);
            Debug.Log("ステータスの読み込みが完了しました");
        }

        public async UniTaskVoid StartSearch()
        {
            //探索を行う
            for (int i = searchCount; i < searchTimes; i++)
            {
                //調整の対象となるキャラクターをランダムに取得する
                int characterIndex = Random.Range(0, characterCount);

                //調整対象となるキャラクターを含むパーティの組み合わせをすべて取得する
                IEnumerable<IEnumerable<int>> partyCharacterIndexList = Enumerable.Range(0, characterCount).Combination(characterIndex, 4);
                partyList.Clear();

                //すべてのパーティに対して評価を行う
                foreach(var partyCharacterIndex in partyCharacterIndexList)
                {
                    ExecuteBattle(partyCharacterIndex).Forget();
                }

                //すべてのパーティの評価が終了するまで待機
                await WaitEvaluateParties(tokenSource.Token);
                disposables.Clear();
                Debug.Log("Finish First half of Battle");

                //調整対象のキャラクターのステータスを取得
                var characterStatus = characterManager.PlayableCharacterStatusList[characterIndex];

                //選択したキャラクターに対して評価を行う
                var synergyPoint = evaluationFunctions.EvaluateSynergy(partyList);
                var distance = evaluationFunctions.EvaluateParameterDistance(characterManager.PlayableCharacterStatusList, characterStatus);
                var penaltyParty = evaluationFunctions.PenaltyForStrongParty(partyList);
                var penaltyCharacter = evaluationFunctions.PenaltyForStrongCharacter(partyList);
                var evaluation = evaluationFunctions.EvaluateCharacter(synergyPoint, distance, penaltyParty, penaltyCharacter);

                //パラメータを突然変異させる
                var variantHP = Mathf.CeilToInt(characterStatus.baseMaxHP * Random.Range(0.9f, 1.1f));
                var variantMP = Mathf.CeilToInt(characterStatus.baseMaxMP * Random.Range(0.9f, 1.1f));
                var variantAttack = Mathf.CeilToInt(characterStatus.baseAttackValue * Random.Range(0.9f, 1.1f));
                var variantMagic = Mathf.CeilToInt(characterStatus.baseMagicValue * Random.Range(0.9f, 1.1f));
                var variantDefence = Mathf.CeilToInt(characterStatus.baseDefenceValue * Random.Range(0.9f, 1.1f));
                var variantMagicDefence = Mathf.CeilToInt(characterStatus.baseMagicDefenceValue * Random.Range(0.9f, 1.1f));
                var variantAgility = Mathf.CeilToInt(characterStatus.baseAgility * Random.Range(0.9f, 1.1f));
                var variantStatus = new CharacterStatus(variantHP, variantMP, variantAttack, variantMagic, variantDefence, variantMagicDefence, variantAgility);

                //突然変異したキャラクターをリストに追加
                characterManager.AddNewCharacterStatus(variantStatus);

                //突然変異したキャラクターを含むパーティの組み合わせをすべて取得する
                partyCharacterIndexList = Enumerable.Range(0, characterCount+1).Combination(characterCount, new int[1] { characterIndex }, 4);
                partyList.Clear();

                //すべてのパーティに対して評価を行う
                foreach (var partyCharacterIndex in partyCharacterIndexList)
                {
                    ExecuteBattle(partyCharacterIndex).Forget();
                }

                //すべてのパーティーの評価が終了するまで待機
                await WaitEvaluateParties(tokenSource.Token);
                disposables.Clear();
                Debug.Log("Finish Last half of Battle");

                //突然変異したキャラクターを評価する
                var variantSynergyPoint = evaluationFunctions.EvaluateSynergy(partyList);
                var variantDistance = evaluationFunctions.EvaluateParameterDistance(characterManager.PlayableCharacterStatusList.Where(x=>x!=characterStatus), variantStatus);
                var variantPenaltyParty = evaluationFunctions.PenaltyForStrongParty(partyList);
                var variantPenaltyCharacter = evaluationFunctions.PenaltyForStrongCharacter(partyList);
                var variantEvaluation = evaluationFunctions.EvaluateCharacter(variantSynergyPoint, variantDistance, variantPenaltyParty, variantPenaltyCharacter);

                //評価値が高いほうを残す
                if (variantEvaluation > evaluation)
                {
                    characterManager.RemoveCharacter(characterStatus);
                }
                else
                {
                    characterManager.RemoveCharacter(variantStatus);
                }

                LogParameter();
                searchCount = i;

                if (searchCount !=0 && searchCount % 100 == 0)
                {
                    Dispose();
                    await SceneManager.LoadSceneAsync("SampleScene");
                    return;
                }
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
                await battleController.ResultObservable.ToUniTask(cancellationToken:tokenSource.Token);

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

        //パラメータのログを出力する
        private void LogParameter()
        {
            StringBuilder log = new StringBuilder(700);
            int i = 0;
            foreach(var status in characterManager.PlayableCharacterStatusList)
            {
                log.Append($"(Character{i.ToString()}) HP:{status.MaxHP.ToString()} MP:{status.MaxMP.ToString()} " +
                    $"攻撃力{status.AttackValue.ToString()} 魔力{status.MagicValue.ToString()} 防御力{status.DefenceValue.ToString()} 魔法防御力{status.MagicDefenceValue.ToString()} 素早さ{status.Agility.ToString()}\n");
                i++;
            }
            PCGLog.WriteLog(log.ToString());
        }

        private void LogStatusJSON()
        {
            PCGLog.DeleteJSONLog();

            for(int i= 0; i < characterCount; i++)
            {
                var status = characterManager.PlayableCharacterStatusList[i];
                var logStatus = new LogStatus(status.baseMaxHP, status.baseMaxMP, status.baseAttackValue, status.baseMagicValue, status.baseDefenceValue, status.baseMagicDefenceValue, status.baseAgility);
                var json=JsonUtility.ToJson(logStatus);
                PCGLog.WriteJSONLog(json);
            }

            PCGLog.WriteJSONLog(searchCount.ToString());
            Debug.Log("Jsonに保存されました");
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            LogStatusJSON();

            tokenSource?.Cancel();
            tokenSource?.Dispose();

            if (!disposables.IsDisposed)
            {
                disposables.Dispose();
            }
        }
    }
}
