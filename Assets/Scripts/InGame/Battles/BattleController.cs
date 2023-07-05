using InGame.Characters;
using InGame.Fields;
using InGame.Parties;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using InGame.Characters.Enemies;
using InGame.Buttles.PlayerAIs;
using InGame.Characters.PlayableCharacters;
using MyUtil;
using VContainer.Unity;
using VContainer;
using Cysharp.Threading.Tasks;
using Log;
using System;
using InGame.Skills;
using InGame.Buttles.Actions;
using InGame.Items;
using System.Threading;
using Unity.MLAgents;
using InGame.Agents.Players;
using InGame.Agents;

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
        private TurnManager turnManager = new TurnManager();
        private PlayableCharacterActionManager playableCharacterActionManager = new PlayableCharacterActionManager();

        private EnemyFactory enemyFactory;
        private PartyManager partyManager;
        private FieldManager fieldManager;
        //private PlayerAI playerAI;
        //private PlayerAgent playerAgent;
        private SimpleMultiAgentGroup agentGroup = new SimpleMultiAgentGroup();
        //private RewardProvider rewardProvider;

        private List<BaseCharacter> hadDoneActionCharacterList= new List<BaseCharacter>();
        private CancellationTokenSource cancellationTokenSource;

        private int battleCount = 0;
        private int winCount = 0;

        private bool IsBattle = false;

        private ISubject<ResultType> resultSubject = new Subject<ResultType>();
        public IObservable<ResultType> ResultObservable => resultSubject;

        [Inject]
        public BattleController(PartyManager partyManager, FieldManager fieldManager, PlayerAgent[] playerAgents, EnemyFactory enemyFactory)
        {
            this.partyManager = partyManager;
            this.fieldManager = fieldManager;
            foreach(var agent in playerAgents)
            {
                agentGroup.RegisterAgent(agent);
            }

            this.enemyFactory = enemyFactory;
        }

        /// <summary>
        /// �G�ƐڐG�������ɌĂяo�����
        /// </summary>
        public void Encount()
        {
            LogWriter.SetFileName();
            IsBattle = true;

            //���N���X�̏�����
            enemyManager = new EnemyManager(enemyFactory);
            int i = 0;
            foreach(var agent in agentGroup.GetRegisteredAgents())
            {
                (agent as PlayerAgent).Init(partyManager.partyCharacters[i], partyManager, enemyManager, playableCharacterActionManager);
                i++;
            }
            resultSubject = new Subject<ResultType>();

            //�t�B�[���h�̐���
            GenerateEnemies(EnemyType.Golem);
            LogCharacterStatus();
            turnManager.StartTurn();

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
                //�^�[���J�n���̏�����
                hadDoneActionCharacterList.Clear();

                LogCharacterHPAndMP();

                try
                {
                    await SelectPlayableCharactersAction(token);
                }
                catch(InvalidOperationException)
                {
                    break;
                }
                
                //���ׂẴG�[�W�F���g���s�������肷��܂őҋ@
                await UniTask.WaitUntil(() => agentGroup.GetRegisteredAgents().Cast<PlayerAgent>().All(x=>x.HadSelectedAction));

                ExecuteActions();
                if (!IsBattle)
                    break;

                await UniTask.DelayFrame(1, cancellationToken: token);
                
                ClearCharacterBuff();
                turnManager.NextTurn();
                foreach (var agent in agentGroup.GetRegisteredAgents())
                {
                    (agent as PlayerAgent).ClearFlag();
                }
            }
        }

        //�v���C���[�ɍs�������肳����
        private async UniTask SelectPlayableCharactersAction(CancellationToken token)
        {
            playableCharacterActionManager.ClearDic();
            //�e�G�[�W�F���g�ɍs�������肳����
            foreach (var agent in agentGroup.GetRegisteredAgents())
            {
                (agent as PlayerAgent).RequestDecision();
                await UniTask.DelayFrame(1, cancellationToken: token);
            }
        }

        private void ExecuteActions()
        {
            //�h�䂷��L�����N�^�[
            IEnumerable<BaseCharacter> defenceActionCharacters = playableCharacterActionManager.GetDefenceActionPairs().OrderByDescending(x => x.characterStatus.Agility);
            //�D��x�̍����s��������L�����N�^�[
            IEnumerable<BaseCharacter> highPriorityActionCharacters= playableCharacterActionManager.GetHighPriorityActionCharacters().OrderByDescending(x => x.characterStatus.Agility);
            //�ʏ�D��x�̍s��������v���C���[�L�����N�^�[
            IEnumerable<BaseCharacter> normalPriorityActionPlayers = playableCharacterActionManager.GetNormalPriorityActionCharacters();
            //�ʏ�D��x�̍s��������L�����N�^�[
            IEnumerable<BaseCharacter> normalPriorityActionCharacters= enemyManager.enemies.Where(x => !x.characterHealth.IsDead)
                                                .Cast<BaseCharacter>()
                                                .Concat(normalPriorityActionPlayers)
                                                .OrderByDescending(x => x.characterStatus.Agility);
            //�D��x�̒Ⴂ�s��������L�����N�^�[
            IEnumerable<BaseCharacter> lowPriorityActionCharacters= playableCharacterActionManager.GetLowPriorityActionCharacters().OrderByDescending(x => x.characterStatus.Agility);

            //�s�����Ƀ\�[�g���ꂽ�L�����N�^�[
            IEnumerable<BaseCharacter> sortedCharacters = defenceActionCharacters.Concat(highPriorityActionCharacters).Concat(normalPriorityActionCharacters).Concat(lowPriorityActionCharacters);
            foreach(var character in sortedCharacters)
            {
                ExecuteCharacterAction(character);

                if (!IsBattle)
                    return;
            }
        }

        /// <summary>
        /// �L�����N�^�[�̍s�������s����
        /// </summary>
        /// <param name="character"></param>
        private void ExecuteCharacterAction(BaseCharacter character)
        {
            //�L�����N�^�[������ł���Ȃ�s�����Ȃ�
            if (character.characterHealth.IsDead)
                return;

            //�L�����N�^�[���v���C���[�̃L�����N�^�[�������Ƃ�
            if(character is PlayableCharacter)
            {
                var actionData = playableCharacterActionManager.GetCharacterActionData(character as PlayableCharacter);
                var success = actionData.ExecuteAction();
                //�A�N�V�������������Ȃ������Ƃ��i�^�[�Q�b�g�����łɎ���ł���Ȃǁj
                if (!success)
                {
                    //�����Ă���G�L��������^�[�Q�b�g��I�тȂ���
                    BaseCharacter target = Retarget(actionData, enemyManager.enemies);
                    if (target == null)
                    {
                        actionData.ExecuteAction(character);
                    }
                    else
                    {
                        actionData.ExecuteAction(target);
                    }
                }
            }
            //�L�����N�^�[���G�L�����������Ƃ�
            else if(character is EnemyCharacter)
            {
                //�GAI�ɍs�������߂�����
                var actionData = enemyManager.GetEnemyAI(character as EnemyCharacter).SelectAction();
                var success = actionData.ExecuteAction();
                //�A�N�V�������������Ȃ������Ƃ��i�^�[�Q�b�g�����łɎ���ł���Ȃǁj
                if (!success)
                {
                    //�����Ă���v���C���[�L�����N�^�[����^�[�Q�b�g��I�тȂ���
                    BaseCharacter target = Retarget(actionData, partyManager.partyCharacters);
                    if (target == null)
                    {
                        actionData.ExecuteAction(character);
                    }
                    else
                    {
                        actionData.ExecuteAction(target);
                    }
                }
            }

            //�s���I�����Ă���L�����N�^�[�̃��X�g�ɒǉ�����
            hadDoneActionCharacterList.Add(character);

            //�퓬���I�����Ă��邩���m�F����
            CheckBattleResult();
        }

        /// <summary>
        /// �^�[�Q�b�g��ύX����
        /// </summary>
        /// <param name="actionData"></param>
        /// <param name="targetGroup"></param>
        /// <returns>
        /// �V�����^�[�Q�b�g��Ԃ�
        /// �V�����^�[�Q�b�g���s���҂ƈ�v����Ȃ�null��Ԃ�
        /// </returns>
        private BaseCharacter Retarget(ActionData actionData, IEnumerable<BaseCharacter> targetGroup)
        {
            switch (actionData.actionType)
            {
                case BaseActionType.NormalAttack:
                    return targetGroup.Where(x => !x.characterHealth.IsDead).RandomGet();
                case BaseActionType.UseItem:
                    var item = ItemDataBase.GetItemData(actionData.itemType);
                    switch (item.targetType)
                    {
                        case TargetType.Friends:
                            return null;
                        case TargetType.Enemy:
                            return targetGroup.Where(x => !x.characterHealth.IsDead).RandomGet();
                    }
                    break;
                case BaseActionType.UseSkill:
                    break;
            }
            return null;
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
            foreach (var character in AllCharacters)
            {
                character.characterStatus.characterBuff.SetIsDefencing(false);
            }
        }

        /// <summary>
        /// �퓬�I�����̏���
        /// </summary>
        /// <param name="result"></param>
        private void FinishBattle(ResultType result)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;

            battleCount++;
            IsBattle = false;

            switch (result)
            {
                case ResultType.Win:
                    Debug.Log("����");
                    LogWriter.WriteLog($"\n����");
                    agentGroup.SetGroupReward(1f);
                    winCount++;
                    break;
                case ResultType.Lose:
                    Debug.Log("�s�k");
                    LogWriter.WriteLog($"\n����");
                    agentGroup.SetGroupReward(-1f);
                    break;
            }

            enemyManager.Dispose();
            
            Debug.Log("Finish Battle");
            Debug.Log("�����F" + (float)winCount / battleCount);
            LogCharacterStatus();

            agentGroup.EndGroupEpisode();

            resultSubject.OnNext(result);
            resultSubject.OnCompleted();
        }

        /// <summary>
        /// �G�������ׂẴL�����N�^�[�̃��X�g��Ԃ�
        /// </summary>
        private IEnumerable<BaseCharacter> AllCharacters
            => enemyManager.enemies.Cast<BaseCharacter>().Concat(partyManager.partyCharacters);

        /// <summary>
        /// �G�����̃X�e�[�^�X�����ׂď�������
        /// </summary>
        private void LogCharacterStatus()
        {
            LogWriter.WriteLog($"�����̃X�e�[�^�X--------------------");
            foreach(var character in partyManager.partyCharacters)
            {
                LogWriter.WriteLog($"({character.characterName}) HP:{character.characterHealth.currentHP}/{character.characterStatus.MaxHP} MP{character.characterMagic.currentMP}/{character.characterStatus.MaxMP} " +
                    $"�U����{character.characterStatus.AttackValue} ����{character.characterStatus.MagicValue} �h���{character.characterStatus.DefecnceValue} ���@�h���{character.characterStatus.MagicDefecnceValue} �f����{character.characterStatus.Agility}" +
                    $"�X�L��({character.rememberSkills.Enumerate()}) ���@({character.rememberMagics.Enumerate()})");
            }
            LogWriter.WriteLog($"------------------------------------");

            LogWriter.WriteLog($"�G�̃X�e�[�^�X--------------------");
            foreach(var enemy in enemyManager.enemies)
            {
                LogWriter.WriteLog($"({enemy.characterName}) HP:{enemy.characterHealth.currentHP}/{enemy.characterStatus.MaxHP} MP{enemy.characterMagic.currentMP}/{enemy.characterStatus.MaxMP} " +
                    $"�U����{enemy.characterStatus.AttackValue} ����{enemy.characterStatus.MagicValue} �h���{enemy.characterStatus.DefecnceValue} ���@�h���{enemy.characterStatus.MagicDefecnceValue} �f����{enemy.characterStatus.Agility}");
            }
            LogWriter.WriteLog($"------------------------------------");
        }

        /// <summary>
        /// ������HP,MP�ƃA�C�e��
        /// �G��HP,MP��\������
        /// </summary>
        private void LogCharacterHPAndMP()
        {
            LogWriter.WriteLog($"�����̃X�e�[�^�X--------------------");
            foreach (var character in partyManager.partyCharacters)
            {
                LogWriter.WriteLog($"({character.characterName}) HP:{character.characterHealth.currentHP}/{character.characterStatus.MaxHP} MP{character.characterMagic.currentMP}/{character.characterStatus.MaxMP}" +
                    $"�����A�C�e��({character.HaveItemList.Enumerate()})");
            }
            LogWriter.WriteLog($"------------------------------------");

            LogWriter.WriteLog($"�G�̃X�e�[�^�X--------------------");
            foreach (var enemy in enemyManager.enemies)
            {
                LogWriter.WriteLog($"({enemy.characterName}) HP:{enemy.characterHealth.currentHP}/{enemy.characterStatus.MaxHP} MP{enemy.characterMagic.currentMP}/{enemy.characterStatus.MaxMP}");
            }
            LogWriter.WriteLog($"------------------------------------");
        }

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            enemyManager?.Dispose();
        }
    }
}

