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

        private EvaluationFunctions evaluationFunctions=new EvaluationFunctions();
        private List<Party> partyList = new List<Party>(128);
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        private readonly int searchTimes = 1000;
        private readonly int battleTimes = 5;
        private readonly int characterCount = 8;

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
            //�v���C���[�L�����N�^�[�𐶐�����
            characterManager.GenerateCharacterStatuses(characterCount);
            LogParameter();

            BattleController.s_id = 0;

            //battleController = new BattleController(playerAgentFactory, enemyFactory);

            //�T�����s��
            for (int i = 0; i < searchTimes; i++)
            {
                //�����̑ΏۂƂȂ�L�����N�^�[�������_���Ɏ擾����
                int characterIndex = Random.Range(0, characterCount);

                //�����ΏۂƂȂ�L�����N�^�[���܂ރp�[�e�B�̑g�ݍ��킹�����ׂĎ擾����
                IEnumerable<IEnumerable<int>> partyCharacterIndexList = Enumerable.Range(0, characterCount).Combination(characterIndex, 4);
                //Debug.Log(partyCharacterIndexList.Count());

                //List<Party> partyList = new List<Party>(128);
                partyList.Clear();

                //���ׂẴp�[�e�B�ɑ΂��ĕ]�����s��
                //Parallel.ForEach(partyCharacterIndexList, async partyCharacterIndex =>
                foreach(var partyCharacterIndex in partyCharacterIndexList)
                {
                    ExecuteBattle(partyCharacterIndex).Forget();
                }

                await UniTask.WaitUntil(() => partyList.TrueForAll(x => x.IsSimulated), cancellationToken:tokenSource.Token);
                Debug.Log("Finish First half of Battle");

                var characterStatus = characterManager.PlayableCharacterStatusList[characterIndex];

                //�I�������L�����N�^�[�ɑ΂��ĕ]�����s��
                var synergyPoint = evaluationFunctions.EvaluateSynergy(partyList);
                var distance = evaluationFunctions.EvaluateParameterDistance(characterManager.PlayableCharacterStatusList, characterStatus);
                var penaltyParty = evaluationFunctions.PenaltyForStrongParty(partyList);
                var penaltyCharacter = evaluationFunctions.PenaltyForStrongCharacter(partyList);
                var evaluation = evaluationFunctions.EvaluateCharacter(synergyPoint, distance, penaltyParty, penaltyCharacter);

                //�p�����[�^��ˑR�ψق�����
                var variantHP = Mathf.CeilToInt(characterStatus.baseMaxHP * Random.Range(0.9f, 1.1f));
                var variantMP = Mathf.CeilToInt(characterStatus.baseMaxMP * Random.Range(0.9f, 1.1f));
                var variantAttack = Mathf.CeilToInt(characterStatus.baseAttackValue * Random.Range(0.9f, 1.1f));
                var variantMagic = Mathf.CeilToInt(characterStatus.baseMagicValue * Random.Range(0.9f, 1.1f));
                var variantDefence = Mathf.CeilToInt(characterStatus.baseDefenceValue * Random.Range(0.9f, 1.1f));
                var variantMagicDefence = Mathf.CeilToInt(characterStatus.baseMagicDefenceValue * Random.Range(0.9f, 1.1f));
                var variantAgility = Mathf.CeilToInt(characterStatus.baseAgility * Random.Range(0.9f, 1.1f));
                var variantStatus = new CharacterStatus(variantHP, variantMP, variantAttack, variantMagic, variantDefence, variantMagicDefence, variantAgility);

                characterManager.AddNewCharacterStatus(variantStatus);

                partyCharacterIndexList = Enumerable.Range(0, characterCount+1).Combination(characterCount, new int[1] { characterIndex }, 4);
                partyList.Clear();

                //���ׂẴp�[�e�B�ɑ΂��ĕ]�����s��
                //Parallel.ForEach(partyCharacterIndexList, async partyCharacterIndex =>
                foreach (var partyCharacterIndex in partyCharacterIndexList)
                {
                    ExecuteBattle(partyCharacterIndex).Forget();
                }

                await UniTask.WaitUntil(() => partyList.TrueForAll(x => x.IsSimulated), cancellationToken: tokenSource.Token);
                Debug.Log("Finish Last half of Battle");

                //�ˑR�ψق����L�����N�^�[��]������
                var variantSynergyPoint = evaluationFunctions.EvaluateSynergy(partyList);
                var variantDistance = evaluationFunctions.EvaluateParameterDistance(characterManager.PlayableCharacterStatusList.Where(x=>x!=characterStatus), variantStatus);
                var variantPenaltyParty = evaluationFunctions.PenaltyForStrongParty(partyList);
                var variantPenaltyCharacter = evaluationFunctions.PenaltyForStrongCharacter(partyList);
                var variantEvaluation = evaluationFunctions.EvaluateCharacter(variantSynergyPoint, variantDistance, variantPenaltyParty, variantPenaltyCharacter);

                //�]���l�������ق����c��
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

            //battleController.Dispose();
        }

        private async UniTask ExecuteBattle(IEnumerable<int> partyCharacterIndex)
        {
            PlayableCharacter[] partyCharacterArray = characterManager.GenerateCharacters(partyCharacterIndex).ToArray();
            var party = new Party(partyCharacterArray);
            partyList.Add(party);

            var battleController = new BattleController(playerAgentFactory, enemyFactory);
            battleController.SetPartyCharacters(partyCharacterArray);

            //�퓬���s���A�������擾����
            int winCount = 0;
            //Parallel.For(0, battleTimes, async _ =>
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
                await battleController.ResultObservable;

                //���ׂẴL�����N�^�[�ɑ΂��ď���������
                characterManager.SetItems(partyCharacterArray);
                for (int k = 0; k < 4; k++)
                {
                    partyCharacterArray[k].FullHeal();
                }

            }

            party.SetWinningParcentage((float)winCount / battleTimes);
            Debug.Log($"����:{party.winningParcentage.ToString()}");
            party.SetIsSimulated(true);
            battleController?.Dispose();
        }

        private void LogParameter()
        {
            StringBuilder log = new StringBuilder(700);
            int i = 0;
            foreach(var status in characterManager.PlayableCharacterStatusList)
            {
                log.Append($"(Character{i.ToString()}) HP:{status.MaxHP.ToString()} MP:{status.MaxMP.ToString()} " +
                    $"�U����{status.AttackValue.ToString()} ����{status.MagicValue.ToString()} �h���{status.DefecnceValue.ToString()} ���@�h���{status.MagicDefecnceValue.ToString()} �f����{status.Agility.ToString()}\n");
                i++;
            }
            PCGLogWriter.WriteLog(log.ToString());
        }

        public void Dispose()
        {
            tokenSource?.Cancel();
            //battleController?.Dispose();
        }
    }
}
