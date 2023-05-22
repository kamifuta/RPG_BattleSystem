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

namespace InGame.Buttles
{
    public class BattleController : ControllerBase, IStartable, IDisposable
    {
        private enum ResultType
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
        private PlayerAI playerAI;

        private List<BaseCharacter> hadDoneActionCharacterList= new List<BaseCharacter>();
        private CancellationTokenSource cancellationTokenSource;

        [Inject]
        public BattleController(PartyManager partyManager, FieldManager fieldManager, PlayerAI playerAI)
        {
            this.partyManager = partyManager;
            this.fieldManager = fieldManager;
            this.playerAI = playerAI;

            enemyFactory = new EnemyFactory(partyManager);
        }

        public void Start()
        {
            ObserveEncountEnemy();
        }
        
        private void ObserveEncountEnemy()
        {
            fieldManager.EncountedEnemyObservable
                .Subscribe(enemyType =>
                {
                    LogWriter.SetFileName();

                    //���N���X�̏�����
                    enemyManager = new EnemyManager(enemyFactory);
                    playerAI.Init(enemyManager, playableCharacterActionManager);

                    //�t�B�[���h�̐���
                    GenerateEnemies(enemyType);
                    turnManager.StartTurn();
                    
                    StartBattle();
                })
                .AddTo(this);
        }

        private void GenerateEnemies(EnemyType encountedEnemyType)
        {
            //TODO: �l�X�Ȑ����̃p�^�[������������i������̐����⌈�߂�ꂽ�p�^�[���̐����j
            //NOTE: ���݂͈��ނ�����������
            enemyManager.GenerateEnemies(encountedEnemyType, 1);
        }

        private void SelectPlayableCharactersAction()
        {
            playableCharacterActionManager.ClearDic();
            //�v���C���[�ɃL�����N�^�[�̍s�������߂�����
            playerAI.SelectCharacterAction();
        }

        private void StartBattle()
        {
            cancellationTokenSource = new CancellationTokenSource();
            ProcessBattle(cancellationTokenSource.Token).Forget();
        }

        private async UniTask ProcessBattle(CancellationToken token)
        {
            while (true)
            {
                hadDoneActionCharacterList.Clear();

                LogCharacterStatus();

                SelectPlayableCharactersAction();

                try
                {
                    //�D��x�̍����s�����s��
                    await ExecuteDefenceAction(token);
                    await ExecuteHighPriorityAction(token);

                    await ExecuteNormalPriorityAction(token);

                    await ExecuteLowPriorityAction(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                
                ClearCharacterBuff();
                turnManager.NextTurn();
            }
        }

        private async UniTask ExecuteDefenceAction(CancellationToken token)
        {
            var actionPairs = playableCharacterActionManager.GetDefenceActionPairs().OrderByDescending(x=>x.Key.characterStatus.Agility);
            foreach (var actionPair in actionPairs)
            {
                actionPair.Value.ExecuteAction();
                hadDoneActionCharacterList.Add(actionPair.Key);

                await UniTask.DelayFrame(1, cancellationToken:token);
            }
        }

        private async UniTask ExecuteHighPriorityAction(CancellationToken token)
        {
            var characters = playableCharacterActionManager.GetHighPriorityActionCharacters().OrderByDescending(x=>x.characterStatus.Agility);
            foreach(var character in characters)
            {
                ExecuteCharacterAction(character);

                await UniTask.DelayFrame(1, cancellationToken: token);
            }
        }

        private async UniTask ExecuteNormalPriorityAction(CancellationToken token)
        {
            var players = playableCharacterActionManager.GetNormalPriorityActionCharacters();
            var sortedCharacters= enemyManager.enemies.Where(x => !x.characterHealth.IsDead)
                                                .Cast<BaseCharacter>()
                                                .Concat(players)
                                                .OrderByDescending(x => x.characterStatus.Agility);

            foreach(var character in sortedCharacters)
            {
                ExecuteCharacterAction(character);

                await UniTask.DelayFrame(1, cancellationToken: token);
            }
        }

        private async UniTask ExecuteLowPriorityAction(CancellationToken token)
        {
            var characters = playableCharacterActionManager.GetLowPriorityActionCharacters().OrderByDescending(x => x.characterStatus.Agility);
            foreach (var character in characters)
            {
                ExecuteCharacterAction(character);

                await UniTask.DelayFrame(1, cancellationToken: token);
            }
        }

        private void ExecuteCharacterAction(BaseCharacter character)
        {
            if(character is PlayableCharacter)
            {
                var actionData = playableCharacterActionManager.GetCharacterActionData(character as PlayableCharacter);
                var result = actionData.ExecuteAction();
                if (!result)
                {
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
            else if(character is EnemyCharacter)
            {
                var actionData = enemyManager.GetEnemyAI(character as EnemyCharacter).SelectAction();
                var result = actionData.ExecuteAction();
                if (!result)
                {
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
            hadDoneActionCharacterList.Add(character);

            CheckBattleResult();
        }

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

        private void CheckBattleResult()
        {
            if (enemyManager.enemies.All(x => x.characterHealth.IsDead))
            {
                FinishBattle(ResultType.Win);
                return;
            }

            if (partyManager.partyCharacters.All(x => x.characterHealth.IsDead))
            {
                FinishBattle(ResultType.Lose);
                return;
            }
        }

        private void ClearCharacterBuff()
        {
            foreach(var character in AllCharacters)
            {
                character.characterStatus.characterBuff.SetIsDefencing(false);
            }
        }

        private void FinishBattle(ResultType result)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;

            switch (result)
            {
                case ResultType.Win:
                    LogWriter.WriteLog($"\n����");
                    break;
                case ResultType.Lose:
                    LogWriter.WriteLog($"\n����");
                    break;
            }

            enemyManager.Dispose();

            Debug.Log("Finish Battle");
            LogCharacterStatus();
        }

        private IEnumerable<BaseCharacter> GetSortedCharacterByAgility()
            => enemyManager.enemies.Where(x=>!x.characterHealth.IsDead)
                .Cast<BaseCharacter>()
                .Concat(partyManager.partyCharacters)
                .OrderByDescending(x => x.characterStatus.Agility);

        private IEnumerable<BaseCharacter> AllCharacters
            => enemyManager.enemies.Cast<BaseCharacter>().Concat(partyManager.partyCharacters);

        private void LogCharacterStatus()
        {
            LogWriter.WriteLog($"�����̃X�e�[�^�X--------------------");
            foreach(var character in partyManager.partyCharacters)
            {
                LogWriter.WriteLog($"({character.characterName}) HP:{character.characterHealth.currentHP}/{character.characterStatus.MaxHP} MP{character.characterMagic.currentMP}/{character.characterStatus.MaxMP} " +
                    $"�U����{character.characterStatus.AttackValue} ����{character.characterStatus.MagicValue} �h���{character.characterStatus.DefecnceValue} ���@�h���{character.characterStatus.MagicDefecnceValue} �f����{character.characterStatus.Agility}");
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

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            enemyManager?.Dispose();
        }
    }
}

