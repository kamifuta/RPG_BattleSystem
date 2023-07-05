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

namespace PCGs
{
    public class ParameterSearcher : IStartable, IDisposable
    {
        private CharacterManager characterManager;
        private EvaluationFunctions evaluationFunctions=new EvaluationFunctions();
        private BattleController battleController;
        private PartyManager partyManager;

        private CancellationTokenSource tokenSource;

        private readonly int searchTimes = 5000;
        private readonly int battleTimes = 10;

        [Inject]
        public ParameterSearcher(CharacterManager characterManager, BattleController battleController, PartyManager partyManager)
        {
            this.characterManager = characterManager;
            this.battleController = battleController;
            this.partyManager = partyManager;

            tokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            StartSearch();
        }

        public async void StartSearch()
        {
            characterManager.GenerateCharacters(10);
            LogParameter();
            for (int i = 0; i < searchTimes; i++)
            {
                var character = characterManager.playableCharacters.RandomGet();
                var parties = characterManager.GetParties(character);
                foreach (var party in parties)
                {
                    partyManager.SetParty(party.partyCharacters);

                    int winCount = 0;
                    for (int j = 0; j < battleTimes; j++)
                    {
                        //�o�g�������s����
                        battleController.Encount();
                        battleController.ResultObservable
                            .Take(1)
                            .Subscribe(result =>
                            {
                                if (result == BattleController.ResultType.Win)
                                {
                                    winCount++;
                                }
                            });
                        await battleController.ResultObservable;

                        characterManager.SetItems();
                        foreach(var c in party.partyCharacters)
                        {
                            c.FullHeal();
                        }
                    }

                    party.SetWinningParcentage((float)winCount / battleTimes);
                }

                //Debug.Log("aaa");
                var synergyPoint = evaluationFunctions.EvaluateSynergy(parties);
                var distance = evaluationFunctions.EvaluateParameterDistance(characterManager.playableCharacters, character);
                var penaltyParty = evaluationFunctions.PenaltyForStrongParty(parties);
                var penaltyCharacter = evaluationFunctions.PenaltyForStrongCharacter(parties);
                var evaluation = evaluationFunctions.EvaluateCharacter(synergyPoint, distance, penaltyParty, penaltyCharacter);

                var variantHP = Mathf.CeilToInt(character.characterStatus.baseMaxHP * Random.Range(0.9f, 1.1f));
                var variantMP = Mathf.CeilToInt(character.characterStatus.baseMaxMP * Random.Range(0.9f, 1.1f));
                var variantAttack = Mathf.CeilToInt(character.characterStatus.baseAttackValue * Random.Range(0.9f, 1.1f));
                var variantMagic = Mathf.CeilToInt(character.characterStatus.baseMagicValue * Random.Range(0.9f, 1.1f));
                var variantDefence = Mathf.CeilToInt(character.characterStatus.baseDefenceValue * Random.Range(0.9f, 1.1f));
                var variantMagicDefence = Mathf.CeilToInt(character.characterStatus.baseMagicDefenceValue * Random.Range(0.9f, 1.1f));
                var variantAgility = Mathf.CeilToInt(character.characterStatus.baseAgility * Random.Range(0.9f, 1.1f));
                var variantStatus = new CharacterStatus(variantHP, variantMP, variantAttack, variantMagic, variantDefence, variantMagicDefence, variantAgility);

                var variantCharacter = new PlayableCharacter(variantStatus);
                variantCharacter.SetCharacterName(character.characterName);

                characterManager.playableCharacters.Add(variantCharacter);
                var variantParties = characterManager.GetParties(variantCharacter, new PlayableCharacter[1] { character});
                foreach (var party in variantParties)
                {
                    partyManager.SetParty(party.partyCharacters);

                    int winCount = 0;
                    for (int j = 0; j < battleTimes; j++)
                    {
                        //�o�g�������s����
                        battleController.Encount();
                        battleController.ResultObservable
                            .Take(1)
                            .Subscribe(result =>
                            {
                                if (result == BattleController.ResultType.Win)
                                {
                                    winCount++;
                                }
                            });
                        await battleController.ResultObservable;
                        characterManager.SetItems();

                        foreach (var c in party.partyCharacters)
                        {
                            c.FullHeal();
                        }
                    }

                    party.SetWinningParcentage((float)winCount / battleTimes);
                }

                var variantSynergyPoint = evaluationFunctions.EvaluateSynergy(variantParties);
                var variantDistance = evaluationFunctions.EvaluateParameterDistance(characterManager.playableCharacters, variantCharacter);
                var variantPenaltyParty = evaluationFunctions.PenaltyForStrongParty(variantParties);
                var variantPenaltyCharacter = evaluationFunctions.PenaltyForStrongCharacter(variantParties);
                var variantEvaluation = evaluationFunctions.EvaluateCharacter(variantSynergyPoint, variantDistance, variantPenaltyParty, variantPenaltyCharacter);

                if (variantEvaluation > evaluation)
                {
                    characterManager.playableCharacters.Remove(character);
                    Debug.Log(variantEvaluation);
                }
                else
                {
                    characterManager.playableCharacters.Remove(variantCharacter);
                    Debug.Log(evaluation);
                }

                //Debug.Log("ssss");
                LogParameter();
            }
        }

        private void LogParameter()
        {
            StringBuilder log = new StringBuilder();
            foreach(var character in characterManager.playableCharacters)
            {
                log.Append($"({character.characterName}) HP:{character.characterHealth.currentHP}/{character.characterStatus.MaxHP} MP{character.characterMagic.currentMP}/{character.characterStatus.MaxMP} " +
                    $"�U����{character.characterStatus.AttackValue} ����{character.characterStatus.MagicValue} �h���{character.characterStatus.DefecnceValue} ���@�h���{character.characterStatus.MagicDefecnceValue} �f����{character.characterStatus.Agility}\n");
            }
            PCGLogWriter.WriteLog(log.ToString());
        }

        public void Dispose()
        {
            tokenSource?.Cancel();
        }
    }
}
