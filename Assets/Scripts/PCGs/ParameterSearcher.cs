using System.Collections;
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
using InGame.Parties;
using Log;
using System.Text;
using System.Threading.Tasks;
using InGame.Agents.Players;

namespace PCGs
{
    public class ParameterSearcher : IStartable, IDisposable
    {
        private readonly CharacterManager characterManager;
        private readonly EnemyFactory enemyFactory;
        private readonly PlayerAgentFactory playerAgentFactory;

        private BattleController battleController;

        private EvaluationFunctions evaluationFunctions=new EvaluationFunctions();

        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        private readonly int searchTimes = 5000;
        private readonly int battleTimes = 10;

        [Inject]
        public ParameterSearcher(CharacterManager characterManager, EnemyFactory enemyFactory, PlayerAgentFactory playerAgentFactory)
        {
            this.characterManager = characterManager;
            this.enemyFactory = enemyFactory;
            this.playerAgentFactory = playerAgentFactory;
        }

        public void Start()
        {
            StartSearch().Forget();
        }

        public async UniTaskVoid StartSearch()
        {
            //プレイヤーキャラクターを生成する
            characterManager.GenerateCharacterStatuses(10);
            LogParameter();

            battleController = new BattleController(playerAgentFactory, enemyFactory);

            //探索を行う
            for (int i = 0; i < searchTimes; i++)
            {
                //調整の対象となるキャラクターをランダムに取得する
                int characterIndex = Random.Range(0, 10);

                //調整対象となるキャラクターを含むパーティの組み合わせをすべて取得する
                IEnumerable<IEnumerable<int>> partyCharacterIndexList = Enumerable.Range(0, 9).Combination(characterIndex, 4);

                List<Party> partyList = new List<Party>();

                //すべてのパーティに対して評価を行う
                //Parallel.ForEach(partyCharacterIndexList, async partyCharacterIndex =>
                foreach(var partyCharacterIndex in partyCharacterIndexList)
                {
                    PlayableCharacter[] partyCharacterArray = characterManager.GenerateCharacters(partyCharacterIndex).ToArray();
                    battleController.SetPartyCharacters(partyCharacterArray);
                    var party = new Party(partyCharacterArray);
                    partyList.Add(party);

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
                        await battleController.ResultObservable;

                        //すべてのキャラクターに対して初期化する
                        characterManager.SetItems(partyCharacterArray);
                        for (int k = 0; k < 4; k++)
                        {
                            partyCharacterArray[k].FullHeal();
                        }
                    }

                    party.SetWinningParcentage((float)winCount / battleTimes);
                }

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

                characterManager.AddNewCharacterStatus(variantStatus);

                partyCharacterIndexList = Enumerable.Range(0, 10).Combination(10, new int[1] { characterIndex }, 4);
                partyList.Clear();

                //すべてのパーティに対して評価を行う
                //Parallel.ForEach(partyCharacterIndexList, async partyCharacterIndexList =>
                foreach (var partyCharacterIndex in partyCharacterIndexList)
                {
                    PlayableCharacter[] partyCharacterArray = characterManager.GenerateCharacters(partyCharacterIndex).ToArray();
                    battleController.SetPartyCharacters(partyCharacterArray);
                    var party = new Party(partyCharacterArray);
                    partyList.Add(party);

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
                        await battleController.ResultObservable;

                        //すべてのキャラクターに対して初期化する
                        characterManager.SetItems(party.partyCharacters);
                        for (int k = 0; k < 4; k++)
                        {
                            party.partyCharacters[k].FullHeal();
                        }
                    }

                    party.SetWinningParcentage((float)winCount / battleTimes);
                    await UniTask.DelayFrame(1, cancellationToken: tokenSource.Token);
                }

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
            }

            battleController.Dispose();
        }

        private void LogParameter()
        {
            StringBuilder log = new StringBuilder(700);
            int i = 0;
            foreach(var status in characterManager.PlayableCharacterStatusList)
            {
                log.Append($"(Character{i.ToString()}) HP:{status.MaxHP.ToString()} MP:{status.MaxMP.ToString()} " +
                    $"攻撃力{status.AttackValue.ToString()} 魔力{status.MagicValue.ToString()} 防御力{status.DefecnceValue.ToString()} 魔法防御力{status.MagicDefecnceValue.ToString()} 素早さ{status.Agility.ToString()}\n");
                i++;
            }
            PCGLogWriter.WriteLog(log.ToString());
        }

        public void Dispose()
        {
            tokenSource?.Cancel();
            battleController?.Dispose();
        }
    }
}
