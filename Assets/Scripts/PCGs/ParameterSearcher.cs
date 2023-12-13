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

        private EvaluationFunctions evaluationFunctions;
        private List<Party> partyList = new List<Party>(128);
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private CompositeDisposable disposables = new CompositeDisposable(64);

        private readonly int searchTimes = 10000;
        private readonly int battleTimes = 10;
        private readonly int characterCount = 8;

        private int battleID = 0;
        private int searchCount = 0;

        private bool IsDisposed = false;

        private List<float> evaluatedValueList = new List<float>(1024);
        private CharacterStatusData characterStatusData;

        private PCGSettings PCGSettings;

        [Inject]
        public ParameterSearcher(CharacterManager characterManager, EnemyFactory enemyFactory, PlayerAgentFactory playerAgentFactory, PCGSettings PCGSettings)
        {
            this.characterManager = characterManager;
            this.enemyFactory = enemyFactory;
            this.playerAgentFactory = playerAgentFactory;
            this.PCGSettings = PCGSettings;

            characterStatusData = PCGSettings.StatusData;
            evaluationFunctions = new EvaluationFunctions(characterStatusData);
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
                //�v���C���[�L�����N�^�[�̃X�e�[�^�X�𐶐�����
                characterManager.GenerateCharacterStatuses(characterCount);
                LogParameter();
                return;
            }

            characterManager.ClearStatusList();

            var statusJSONs = PCGLog.ReadStatusJSONLog();
            LogStatus[] logStatusArray = JsonHelper.FromJson<LogStatus>(statusJSONs);
            foreach (var logStatus in logStatusArray)
            {
                var status = new CharacterStatus(logStatus.MaxHP, logStatus.MaxMP, logStatus.AttackValue, logStatus.MagicValue, logStatus.DefenceValue, logStatus.MagicDefenceValue, logStatus.Agility);
                characterManager.AddNewCharacterStatus(status);
            }

            searchCount = PCGLog.CountLogFile();
            Debug.Log("�X�e�[�^�X�̓ǂݍ��݂��������܂���");
        }

        public async UniTaskVoid StartSearch()
        {
            //�T�����s��
            for (int i = searchCount; i < searchTimes; i++)
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
                var penaltyLongBattle = evaluationFunctions.PenaltyForLongBattle(partyList);
                var evaluation = evaluationFunctions.EvaluateCharacter(synergyPoint, distance, penaltyParty, penaltyCharacter, penaltyLongBattle);

                //�p�����[�^��ˑR�ψق�����
                CharacterStatus variantStatus;
                var averageWinningPercentage = partyList.Select(x => x.winningParcentage).Average();
                if (averageWinningPercentage < 0.4f)
                {
                    variantStatus = CreateVariantStatus(characterStatus, 0.95f, 1.2f);
                }
                else if (averageWinningPercentage > 0.7f)
                {
                    variantStatus = CreateVariantStatus(characterStatus, 0.8f, 1.05f);
                }
                else
                {
                    variantStatus = CreateVariantStatus(characterStatus, 0.9f, 1.1f);
                }

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
                var variantPenaltyLongBattle = evaluationFunctions.PenaltyForLongBattle(partyList);
                var variantEvaluation = evaluationFunctions.EvaluateCharacter(variantSynergyPoint, variantDistance, variantPenaltyParty, variantPenaltyCharacter, variantPenaltyLongBattle);

                //�]���l�������ق����c��
                if (variantEvaluation > evaluation)
                {
                    characterManager.RemoveCharacter(characterStatus);
                    evaluatedValueList.Add(variantEvaluation);
                }
                else
                {
                    characterManager.RemoveCharacter(variantStatus);
                    evaluatedValueList.Add(evaluation);
                }

                PCGLog.WriteEvaluationCSV(evaluatedValueList.Average());
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

        public CharacterStatus CreateVariantStatus(CharacterStatus characterStatus, float minMagnification, float maxMagnification)
        {
            //�p�����[�^��ˑR�ψق�����
            var variantHP = Mathf.CeilToInt(characterStatus.baseMaxHP * Random.Range(minMagnification, maxMagnification));
            variantHP = Math.Clamp(variantHP, characterStatusData.maxHP_min, characterStatusData.maxHP_max);
            var variantMP = Mathf.CeilToInt(characterStatus.baseMaxMP * Random.Range(minMagnification, maxMagnification));
            variantMP = Math.Clamp(variantMP, characterStatusData.maxMP_min, characterStatusData.maxMP_max);
            var variantAttack = Mathf.CeilToInt(characterStatus.baseAttackValue * Random.Range(minMagnification, maxMagnification));
            variantAttack = Math.Clamp(variantAttack, characterStatusData.attackValue_min, characterStatusData.attackValue_max);
            var variantMagic = Mathf.CeilToInt(characterStatus.baseMagicValue * Random.Range(minMagnification, maxMagnification));
            variantMagic = Math.Clamp(variantMagic, characterStatusData.magicValue_min, characterStatusData.magicValue_max);
            var variantDefence = Mathf.CeilToInt(characterStatus.baseDefenceValue * Random.Range(minMagnification, maxMagnification));
            variantDefence = Math.Clamp(variantDefence, characterStatusData.defenceValue_min, characterStatusData.defenceValue_max);
            var variantMagicDefence = Mathf.CeilToInt(characterStatus.baseMagicDefenceValue * Random.Range(minMagnification, maxMagnification));
            variantMagicDefence = Math.Clamp(variantMagicDefence, characterStatusData.magicDefenceValue_min, characterStatusData.magicDefenceValue_max);
            var variantAgility = Mathf.CeilToInt(characterStatus.baseAgility * Random.Range(minMagnification, maxMagnification));
            variantAgility = Math.Clamp(variantAgility, characterStatusData.agility_min, characterStatusData.agility_max);
            var variantStatus = new CharacterStatus(variantHP, variantMP, variantAttack, variantMagic, variantDefence, variantMagicDefence, variantAgility);

            return variantStatus;
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
                var result=await UniTask.WhenAny(
                    UniTask.WaitUntil(()=>battleController.CurrentTurn>=PCGSettings.SimulateTurn, cancellationToken:tokenSource.Token),
                    battleController.ResultObservable.ToUniTask(cancellationToken: tokenSource.Token)
                    );

                switch (result)
                {
                    case 0:
                        Debug.Log("���f");
                        battleController.SuspendBattle();
                        party.SetHadSuspended(true);
                        break;
                    case 1:
                        break;
                }

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

        //�p�����[�^�̃��O���o�͂���
        private void LogParameter()
        {
            StringBuilder log = new StringBuilder(512);
            int i = 0;
            foreach(var status in characterManager.PlayableCharacterStatusList)
            {
                log.Append($"(Character{i.ToString()}) HP:{status.baseMaxHP.ToString()} MP:{status.baseMaxMP.ToString()} " +
                    $"�U����{status.baseAttackValue.ToString()} ����{status.baseMagicValue.ToString()} �h���{status.baseDefenceValue.ToString()} ���@�h���{status.baseMagicDefenceValue.ToString()} �f����{status.baseAgility.ToString()}\n");
                i++;
            }
            PCGLog.WriteStatusLog(log.ToString());
        }

        private void LogStatusJSON()
        {
            PCGLog.DeleteStatusJSONLog();
            var logStatusList = new List<LogStatus>();

            for(int i= 0; i < characterCount; i++)
            {
                var status = characterManager.PlayableCharacterStatusList[i];
                var logStatus = new LogStatus(status.baseMaxHP, status.baseMaxMP, status.baseAttackValue, status.baseMagicValue, status.baseDefenceValue, status.baseMagicDefenceValue, status.baseAgility);
                logStatusList.Add(logStatus);
            }

            var json = JsonHelper.ToJson(logStatusList);
            PCGLog.WriteStatusJSONLog(json);

            Debug.Log("Json�ɕۑ�����܂���");
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
