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
        private readonly TurnManager turnManager = new TurnManager();
        private PlayableCharacterActionManager playableCharacterActionManager = new PlayableCharacterActionManager();

        private readonly EnemyFactory enemyFactory;
        private readonly PlayerAgentFactory playerAgentFactory;
        private PartyManager partyManager;
        private readonly SimpleMultiAgentGroup agentGroup = new SimpleMultiAgentGroup();

        private List<BaseCharacter> hadDoneActionCharacterList= new List<BaseCharacter>();
        private CancellationTokenSource cancellationTokenSource;

        private int battleCount = 0;
        private int winCount = 0;

        public static int s_id;
        private int id;
        private string fileName;

        private bool IsBattle = false;

        private ISubject<ResultType> resultSubject = new Subject<ResultType>();
        public IObservable<ResultType> ResultObservable => resultSubject;

        public BattleController(PlayerAgentFactory playerAgentFactory, EnemyFactory enemyFactory)
        {
            this.enemyFactory = enemyFactory;
            this.playerAgentFactory = playerAgentFactory;
            this.id = s_id;
            s_id++;
        }

        public void SetPartyCharacters(PlayableCharacter[] partyCharacters)
        {
            DestroyAgentObject();
            partyManager = new PartyManager(partyCharacters);
            var length = partyCharacters.Length;
            for(int i=0;i<length;i++)
            {
                var character = partyCharacters[i];
                var agent = playerAgentFactory.GeneratePlayerAgent(character);
                agentGroup.RegisterAgent(agent);
            }
        }

        /// <summary>
        /// �G�ƐڐG�������ɌĂяo�����
        /// </summary>
        public void Encount()
        {
            //LogWriter.SetFileName();
            IsBattle = true;

            //���N���X�̏�����
            enemyManager = new EnemyManager(enemyFactory, partyManager);
            GenerateEnemies(EnemyType.Golem);

            foreach (var agent in agentGroup.GetRegisteredAgents())
            {
                (agent as PlayerAgent).Init(partyManager, enemyManager, playableCharacterActionManager);
            }
            resultSubject = new Subject<ResultType>();

            fileName = $"LogFile_{id.ToString()}_{battleCount.ToString()}";

            //�t�B�[���h�̐���
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

                //�L�����N�^�[�̏�ԏo��
                LogCharacterHPAndMP();

                //�G�[�W�F���g�ɍs�������肳����
                SelectPlayableCharactersAction();
                
                //���ׂẴG�[�W�F���g���s�������肷��܂őҋ@
                await UniTask.WaitUntil(() => agentGroup.GetRegisteredAgents().Cast<PlayerAgent>().All(x=>x.HadSelectedAction), cancellationToken:token);

                //�L�����N�^�[�̍s�������s����
                ExecuteActions();
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

        //�v���C���[�ɍs�������肳����
        private void SelectPlayableCharactersAction()
        {
            playableCharacterActionManager.ClearDic();
            //�e�G�[�W�F���g�ɍs�������肳����
            foreach (var agent in agentGroup.GetRegisteredAgents())
            {
                (agent as PlayerAgent).RequestDecision();
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
            for (int i = 0; i < 4; i++)
            {
                var character = partyManager.partyCharacters[i];
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
                    Debug.Log("<color=red>����</color>");
                    LogWriter.WriteLog($"\n����", fileName);
                    agentGroup.SetGroupReward(1f);
                    winCount++;
                    break;
                case ResultType.Lose:
                    Debug.Log("<color=blue>�s�k</color>");
                    LogWriter.WriteLog($"\n����", fileName);
                    agentGroup.SetGroupReward(-1f);
                    break;
            }

            enemyManager.Dispose();
            
            //Debug.Log("Finish Battle");
            //Debug.Log("�����F" + (float)winCount / battleCount);
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
                    $"�U����{character.characterStatus.AttackValue.ToString()} ����{character.characterStatus.MagicValue.ToString()} �h���{character.characterStatus.DefecnceValue.ToString()} ���@�h���{character.characterStatus.MagicDefecnceValue.ToString()} �f����{character.characterStatus.Agility.ToString()}" +
                    $"�X�L��({character.rememberSkills.Enumerate()}) ���@({character.rememberMagics.Enumerate()})", fileName);
            }
            LogWriter.WriteLog($"------------------------------------", fileName);

            LogWriter.WriteLog($"�G�̃X�e�[�^�X--------------------", fileName);
            var count = enemyManager.enemies.Length;
            for(int i = 0; i < count; i++)
            {
                var enemy = enemyManager.enemies[i];
                LogWriter.WriteLog($"({enemy.characterName}) HP:{enemy.characterHealth.currentHP.ToString()}/{enemy.characterStatus.MaxHP.ToString()} MP{enemy.characterMagic.currentMP.ToString()}/{enemy.characterStatus.MaxMP.ToString()} " +
                    $"�U����{enemy.characterStatus.AttackValue.ToString()} ����{enemy.characterStatus.MagicValue.ToString()} �h���{enemy.characterStatus.DefecnceValue.ToString()} ���@�h���{enemy.characterStatus.MagicDefecnceValue.ToString()} �f����{enemy.characterStatus.Agility.ToString()}", fileName);
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
                var agent = agentArray[i];
                agentGroup.UnregisterAgent(agent);
                playerAgentFactory.DestroyPlayerAgent(agent as PlayerAgent);
            }
        }

        public void Dispose()
        {
            DestroyAgentObject();

            cancellationTokenSource?.Cancel();
            enemyManager?.Dispose();
        }
    }
}

