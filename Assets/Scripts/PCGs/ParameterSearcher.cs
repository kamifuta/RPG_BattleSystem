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
        private readonly int battleTimes = 5;
        private readonly int characterCount = 8;

        private int battleID = 0;
        private int searchCount = 0;

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
                Debug.Log("�X�e�[�^�X�t�@�C�������݂��܂���");
                return;
            }

            var statusJSONs = PCGLog.ReadJSONLog().Split("\n");

            for (int i = 0; i < characterCount; i++)
            {
                var json = statusJSONs[i];
                var logStatus = JsonUtility.FromJson<LogStatus>(json);
                var status = new CharacterStatus(logStatus.MaxHP, logStatus.MaxMP, logStatus.AttackValue, logStatus.MagicValue, logStatus.DefenceValue, logStatus.MagicDefenceValue, logStatus.Agility);
                characterManager.AddNewCharacterStatus(status);
            }

            searchCount=Int32.Parse(statusJSONs[characterCount]);
            Debug.Log("�X�e�[�^�X�̓ǂݍ��݂��������܂���");
        }

        public async UniTaskVoid StartSearch()
        {
            //�v���C���[�L�����N�^�[�𐶐�����
            characterManager.GenerateCharacterStatuses(characterCount);
            LogParameter();

            //�T�����s��
            for (int i = 0; i < searchTimes; i++)
            {
                //�����̑ΏۂƂȂ�L�����N�^�[�������_���Ɏ擾����
                int characterIndex = Random.Range(0, characterCount);

                //�����ΏۂƂȂ�L�����N�^�[���܂ރp�[�e�B�̑g�ݍ��킹�����ׂĎ擾����
                IEnumerable<IEnumerable<int>> partyCharacterIndexList = Enumerable.Range(0, characterCount).Combination(characterIndex, 4);
                partyList.Clear();

                //���ׂẴp�[�e�B�ɑ΂��ĕ]�����s��
                foreach(var partyCharacterIndex in partyCharacterIndexList)
                {
                    ExecuteBattle(partyCharacterIndex).Forget();
                }

                //���ׂẴp�[�e�B�̕]�����I������܂őҋ@
                await WaitEvaluateParties(tokenSource.Token);
                disposables.Clear();
                Debug.Log("Finish First half of Battle");

                //�����Ώۂ̃L�����N�^�[�̃X�e�[�^�X���擾
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

                //�ˑR�ψق����L�����N�^�[�����X�g�ɒǉ�
                characterManager.AddNewCharacterStatus(variantStatus);

                //�ˑR�ψق����L�����N�^�[���܂ރp�[�e�B�̑g�ݍ��킹�����ׂĎ擾����
                partyCharacterIndexList = Enumerable.Range(0, characterCount+1).Combination(characterCount, new int[1] { characterIndex }, 4);
                partyList.Clear();

                //���ׂẴp�[�e�B�ɑ΂��ĕ]�����s��
                foreach (var partyCharacterIndex in partyCharacterIndexList)
                {
                    ExecuteBattle(partyCharacterIndex).Forget();
                }

                //���ׂẴp�[�e�B�[�̕]�����I������܂őҋ@
                await WaitEvaluateParties(tokenSource.Token);
                disposables.Clear();
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
                searchCount = i;
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
                await battleController.ResultObservable.ToUniTask(cancellationToken:tokenSource.Token);

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
            battleController.Dispose();
        }

        //�p�����[�^�̃��O���o�͂���
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
            Debug.Log("Json�ɕۑ�����܂���");
        }

        [Serializable]
        private class LogStatus
        {
            public int MaxHP;
            public int MaxMP;
            public int AttackValue;
            public int MagicValue;
            public int DefenceValue;
            public int MagicDefenceValue;
            public int Agility;

            public LogStatus(int maxHP, int maxMP, int attack, int magic, int defence, int magicDefence, int agility)
            {
                MaxHP = maxHP;
                MaxMP = maxMP;
                AttackValue = attack;
                MagicValue = magic;
                DefenceValue = defence;
                MagicDefenceValue = magicDefence;
                Agility = agility;
            }
        }

        public void Dispose()
        {
            LogStatusJSON();

            tokenSource?.Cancel();
            tokenSource?.Dispose();

            disposables.Dispose();
        }
    }
}
