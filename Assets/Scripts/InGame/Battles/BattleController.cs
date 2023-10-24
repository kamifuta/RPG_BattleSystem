using InGame.Characters;
using InGame.Parties;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using InGame.Characters.Enemies;
using InGame.Characters.PlayableCharacters;
using MyUtil;
using Cysharp.Threading.Tasks;
using Log;
using System;
using InGame.Skills;
using InGame.Buttles.Actions;
using InGame.Items;
using System.Threading;
using Unity.MLAgents;
using InGame.Agents.Players;
using InGame.Magics;

namespace InGame.Buttles
{
    public class BattleController : ControllerBase,�@IDisposable
    {
        public enum ResultType
        {
            None,
            Win,
            Lose,
        }

        private EnemyManager enemyManager;
        private PartyManager partyManager;
        private IEnumerable<BaseCharacter> allCharacters;

        private readonly EnemyFactory enemyFactory;
        private readonly PlayerAgentFactory playerAgentFactory;

        private readonly TurnManager turnManager = new TurnManager();
        private readonly SimpleMultiAgentGroup agentGroup = new SimpleMultiAgentGroup();

        private CancellationTokenSource cancellationTokenSource;

        private int battleCount = 0;
        private readonly int id=0;
        private string fileName;

        private bool IsBattle = false;
        private bool IsWaitingPlayerDecision = false;

        private ActionData selectedPlayerActionData = null;

        private Subject<ResultType> resultSubject;
        public IObservable<ResultType> ResultObservable => resultSubject;

        public BattleController(PlayerAgentFactory playerAgentFactory, EnemyFactory enemyFactory, PlayableCharacter[] partyCharacters, int battleID)
        {
            this.enemyFactory = enemyFactory;
            this.playerAgentFactory = playerAgentFactory;
            this.id = battleID;

            SetPartyCharacters(partyCharacters);
        }

        public void SetPartyCharacters(PlayableCharacter[] partyCharacters)
        {
            //�v���C���[�ɉ������G�[�W�F���g�𐶐�����
            DestroyAgentObject();
            partyManager = new PartyManager(partyCharacters);
            var length = partyCharacters.Length;
            for(int i=0;i<length;i++)
            {
                var character = partyCharacters[i];
                var agent = playerAgentFactory.GeneratePlayerAgent(character);
                agentGroup.RegisterAgent(agent);

                agent.SelectedActionDataObservable
                    .Subscribe(actionData =>
                    {
                        selectedPlayerActionData = actionData;
                        IsWaitingPlayerDecision = false;
                    })
                    .AddTo(this);
            }
        }

        /// <summary>
        /// �G�ƐڐG�������ɌĂяo�����
        /// </summary>
        public void Encount()
        {
            IsBattle = true;

            //���N���X�̏�����
            enemyManager = new EnemyManager(enemyFactory, partyManager);
            GenerateEnemies(EnemyType.Golem);

            foreach (var agent in agentGroup.GetRegisteredAgents())
            {
                (agent as PlayerAgent).Init(partyManager, enemyManager);
            }
            resultSubject = new Subject<ResultType>();

            fileName = $"LogFile_{id.ToString()}_{battleCount.ToString()}";

            //�t�B�[���h�̐���
            LogCharacterStatus();
            turnManager.StartTurn();
            allCharacters = agentGroup.GetRegisteredAgents().Cast<PlayerAgent>().Select(x => x.agentCharacter)
                                                            .Concat(enemyManager.enemies.Cast<BaseCharacter>());

            //�퓬���J�n����
            StartBattle();
        }

        /// <summary>
        /// �G�𐶐�����
        /// </summary>
        /// <param name="encountedEnemyType"></param>
        private void GenerateEnemies(EnemyType encountedEnemyType)
        {
            //TODO: �l�X�Ȑ����̃p�^�[������������i������̐����⌈�߂�ꂽ�p�^�[���̐����j
            //NOTE: ���݂͈��ނ�����������
            enemyManager.GenerateEnemies(encountedEnemyType, 1);
        }

        /// <summary>
        /// �퓬���J�n����
        /// </summary>
        private void StartBattle()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            ProcessBattle(cancellationTokenSource.Token).Forget();
        }

        /// <summary>
        /// �퓬��i�߂�
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask ProcessBattle(CancellationToken token)
        {
            //�퓬���I������܂Ń��[�v����
            while (IsBattle)
            {
                //�L�����N�^�[�̏�ԏo��
                LogCharacterHPAndMP();

                //�����Ă��邷�ׂẴL�����N�^�[��f�����̏��Ƀ\�[�g
                var sortedCharacters = allCharacters.Where(x => !x.characterHealth.IsDead).OrderByDescending(x => x.characterStatus.Agility);

                //�f�������������ɍs�����肳���A���s����
                foreach(var character in sortedCharacters)
                {
                    var actionData = await DecideAction(character, token);

                    if (actionData == null)
                        continue;

                    ExecuteCharacterAction(actionData);

                    //�퓬���I�����Ă��邩���m�F����
                    CheckBattleResult();
                    if (!IsBattle)
                        break;
                }

                if (!IsBattle)
                    break;

                //�^�[���I�����̏���
                ClearCharacterBuff();
                turnManager.NextTurn();
                foreach (var agent in agentGroup.GetRegisteredAgents())
                {
                    (agent as PlayerAgent).ClearFlag();
                }
            }
        }

        private async UniTask<ActionData> DecideAction(BaseCharacter character, CancellationToken token)
        {
            //�L�����N�^�[������ł���Ȃ�s�����Ȃ�
            if (character.characterHealth.IsDead)
                return null;

            //�L�����N�^�[���v���C���[�̃L�����N�^�[�������Ƃ�
            if (character is PlayableCharacter)
            {
                IsWaitingPlayerDecision = true;
                var agent = agentGroup.GetRegisteredAgents().Cast<PlayerAgent>().Single(x => x.agentCharacter == character);

                agent.RequestDecision();

                await UniTask.WaitUntil(() => !IsWaitingPlayerDecision, cancellationToken: token);

                return selectedPlayerActionData;
            }
            //�L�����N�^�[���G�L�����������Ƃ�
            else if (character is EnemyCharacter)
            {
                //�GAI�ɍs�������߂�����
                var actionData = enemyManager.GetEnemyAI(character as EnemyCharacter).SelectAction();
                return actionData;
            }

            return null;
        }

        /// <summary>
        /// �L�����N�^�[�̍s�������s����
        /// </summary>
        /// <param name="character"></param>
        private void ExecuteCharacterAction(ActionData actionData)
        {
            actionData.ExecuteAction();
        }

        /// <summary>
        /// �퓬���I�����Ă��邩�𒲂ׂ�
        /// </summary>
        private void CheckBattleResult()
        {
            //�G���S������ł���Ƃ��v���C���[�̏���
            if (enemyManager.enemies.All(x => x.characterHealth.IsDead))
            {
                FinishBattle(ResultType.Win);
                return;
            }

            //�������S������ł���Ƃ��v���C���[�̕���
            if (partyManager.partyCharacters.All(x => x.characterHealth.IsDead))
            {
                FinishBattle(ResultType.Lose);
                return;
            }
        }

        /// <summary>
        /// �L�����N�^�[�ɂ������Ă���o�t������
        /// </summary>
        private void ClearCharacterBuff()
        {
            for (int i = 0; i < 4; i++)
            {
                var character = partyManager.partyCharacters[i];
                character.characterStatus.characterBuff.TryDeleteBuff();
            }
        }

        /// <summary>
        /// �퓬�I�����̏���
        /// </summary>
        /// <param name="result"></param>
        private void FinishBattle(ResultType result)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;

            battleCount++;
            IsBattle = false;

            switch (result)
            {
                case ResultType.Win:
                    Debug.Log("<color=red>����</color>");
                    LogWriter.WriteLog($"\n����", fileName);
                    agentGroup.SetGroupReward(1f);
                    break;
                case ResultType.Lose:
                    Debug.Log("<color=blue>�s�k</color>");
                    LogWriter.WriteLog($"\n����", fileName);
                    agentGroup.SetGroupReward(-1f);
                    break;
            }

            enemyManager.Dispose();
            LogCharacterStatus();
            agentGroup.EndGroupEpisode();

            resultSubject.OnNext(result);
            resultSubject.OnCompleted();
        }

        /// <summary>
        /// �G�����̃X�e�[�^�X�����ׂď�������
        /// </summary>
        private void LogCharacterStatus()
        {
            LogWriter.WriteLog($"�����̃X�e�[�^�X--------------------", fileName);
            for(int i = 0; i < 4; i++)
            {
                var character = partyManager.partyCharacters[i];
                LogWriter.WriteLog($"({character.characterName}) HP:{character.characterHealth.currentHP.ToString()}/{character.characterStatus.MaxHP.ToString()} MP{character.characterMagic.currentMP.ToString()}/{character.characterStatus.MaxMP.ToString()} " +
                    $"�U����{character.characterStatus.AttackValue.ToString()} ����{character.characterStatus.MagicValue.ToString()} �h���{character.characterStatus.DefenceValue.ToString()} ���@�h���{character.characterStatus.MagicDefenceValue.ToString()} �f����{character.characterStatus.Agility.ToString()}" +
                    $"�X�L��({character.rememberSkills.Enumerate()}) ���@({character.rememberMagics.Enumerate()})", fileName);
            }
            LogWriter.WriteLog($"------------------------------------", fileName);

            LogWriter.WriteLog($"�G�̃X�e�[�^�X--------------------", fileName);
            var count = enemyManager.enemies.Length;
            for(int i = 0; i < count; i++)
            {
                var enemy = enemyManager.enemies[i];
                LogWriter.WriteLog($"({enemy.characterName}) HP:{enemy.characterHealth.currentHP.ToString()}/{enemy.characterStatus.MaxHP.ToString()} MP{enemy.characterMagic.currentMP.ToString()}/{enemy.characterStatus.MaxMP.ToString()} " +
                    $"�U����{enemy.characterStatus.AttackValue.ToString()} ����{enemy.characterStatus.MagicValue.ToString()} �h���{enemy.characterStatus.DefenceValue.ToString()} ���@�h���{enemy.characterStatus.MagicDefenceValue.ToString()} �f����{enemy.characterStatus.Agility.ToString()}", fileName);
            }
            LogWriter.WriteLog($"------------------------------------", fileName);
        }

        /// <summary>
        /// ������HP,MP�ƃA�C�e��
        /// �G��HP,MP��\������
        /// </summary>
        private void LogCharacterHPAndMP()
        {
            LogWriter.WriteLog($"�����̃X�e�[�^�X--------------------", fileName);
            for (int i = 0; i < 4; i++)
            {
                var character = partyManager.partyCharacters[i];
                LogWriter.WriteLog($"({character.characterName}) HP:{character.characterHealth.currentHP.ToString()}/{character.characterStatus.MaxHP.ToString()} MP{character.characterMagic.currentMP.ToString()}/{character.characterStatus.MaxMP.ToString()}" +
                    $"�����A�C�e��({character.HaveItemList.Enumerate()})", fileName);
            }
            LogWriter.WriteLog($"------------------------------------", fileName);

            LogWriter.WriteLog($"�G�̃X�e�[�^�X--------------------", fileName);
            var count = enemyManager.enemies.Length;
            for (int i = 0; i < count; i++)
            {
                var enemy = enemyManager.enemies[i];
                LogWriter.WriteLog($"({enemy.characterName}) HP:{enemy.characterHealth.currentHP.ToString()}/{enemy.characterStatus.MaxHP.ToString()} MP{enemy.characterMagic.currentMP.ToString()}/{enemy.characterStatus.MaxMP.ToString()}", fileName);
            }
            LogWriter.WriteLog($"------------------------------------", fileName);
        }

        private void DestroyAgentObject()
        {
            var agentArray = agentGroup.GetRegisteredAgents().ToArray();
            if (agentArray.Length <= 0)
                return;
            for(int i = 0; i < 4; i++)
            {
                try
                {
                    var agent = agentArray[i];
                    agentGroup.UnregisterAgent(agent);
                    playerAgentFactory.DestroyPlayerAgent(agent as PlayerAgent);
                }
                catch (IndexOutOfRangeException)
                {
                    break;
                }
            }
        }

        public void Dispose()
        {
            DestroyAgentObject();

            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            resultSubject.Dispose();
            enemyManager?.Dispose();
        }
    }
}

