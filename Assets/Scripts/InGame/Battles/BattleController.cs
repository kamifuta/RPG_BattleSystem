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
using InGame.Characters.Skills;
using InGame.Buttles.Actions;

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

                    //他クラスの初期化
                    enemyManager = new EnemyManager(enemyFactory);
                    playerAI.Init(enemyManager, playableCharacterActionManager);

                    //フィールドの生成
                    GenerateEnemies(enemyType);
                    turnManager.StartTurn();
                    
                    StartBattle();
                })
                .AddTo(this);
        }

        private void GenerateEnemies(EnemyType encountedEnemyType)
        {
            //TODO: 様々な生成のパターンを実装する（複数種の生成や決められたパターンの生成）
            //NOTE: 現在は一種類だけ生成する
            enemyManager.GenerateEnemies(encountedEnemyType, 1);
        }

        private void SelectPlayableCharactersAction()
        {
            playableCharacterActionManager.ClearDic();
            //プレイヤーにキャラクターの行動を決めさせる
            playerAI.SelectCharacterAction();
        }

        private void StartBattle()
        {
            ProcessBattle().Forget();
        }

        private async UniTaskVoid ProcessBattle()
        {
            while (true)
            {
                LogCharacterStatus();

                SelectPlayableCharactersAction();
                ExecuteHighPriorityAction();

                var characters = GetSortedCharacterByAgility();
                foreach(var character in characters)
                {
                    if (character.characterHealth.IsDead)
                        continue;

                    if (character.HadDoneAction)
                        continue;

                    //それぞれのキャラの行動を実行する
                    ExecuteCharacterAction(character);

                    var result = CheckBattleResult();
                    if (result != ResultType.None)
                    {
                        FinishBattle(result);
                        return;
                    }

                    await UniTask.DelayFrame(1);
                }

                ResetFlag();
                ClearCharacterBuff();
                turnManager.NextTurn();
            }
        }

        private void ExecuteHighPriorityAction()
        {
            var actions=playableCharacterActionManager.GetHighPriorityAction();
            foreach(var action in actions)
            {
                action.Value.ExecuteAction();
                action.Key.SetHadDoneAction(true);
            }
        }

        private void ExecuteCharacterAction(BaseCharacter character)
        {
            if(character is PlayableCharacter)
            {
                var action = playableCharacterActionManager.GetCharacterAction(character as PlayableCharacter);
                var result = action.ExecuteAction();
                if (!result)
                {
                    ActionArgument arg;

                    switch (action.skillData.targetType)
                    {
                        case TargetType.Self:
                            break;
                        case TargetType.Friends:
                            arg = new ActionArgument(character, character);
                            action.ExecuteAction(arg);
                            break;
                        case TargetType.Enemy:
                            var target = enemyManager.enemies.Where(x=>!x.characterHealth.IsDead).RandomGet();
                            arg = new ActionArgument(character, target);
                            action.ExecuteAction(arg);
                            break;
                    }
                }
            }
            else if(character is EnemyCharacter)
            {
                var action = enemyManager.GetEnemyAI(character as EnemyCharacter).SelectAction();
                var result = action.ExecuteAction();
                if (!result)
                {
                    ActionArgument arg;

                    switch (action.skillData.targetType)
                    {
                        case TargetType.Self:
                            break;
                        case TargetType.Friends:
                            arg = new ActionArgument(character, character);
                            action.ExecuteAction(arg);
                            break;
                        case TargetType.Enemy:
                            var target = partyManager.partyCharacters.Where(x => !x.characterHealth.IsDead).RandomGet();
                            arg = new ActionArgument(character, target);
                            action.ExecuteAction(arg);
                            break;
                    }
                }
            }
            character.SetHadDoneAction(true);
        }

        private ResultType CheckBattleResult()
        {
            if (enemyManager.enemies.All(x => x.characterHealth.IsDead))
                return ResultType.Win;

            if (partyManager.partyCharacters.All(x => x.characterHealth.IsDead))
                return ResultType.Lose;

            return ResultType.None;
        }

        private void ClearCharacterBuff()
        {
            foreach(var character in AllCharacters)
            {
                character.characterStatus.characterBuff.SetIsDefencing(false);
            }
        }

        private void ResetFlag()
        {
            foreach (var character in AllCharacters)
            {
                character.ResetFlag();
            }
        }

        private void FinishBattle(ResultType result)
        {
            switch (result)
            {
                case ResultType.Win:
                    LogWriter.WriteLog($"勝利");
                    break;
                case ResultType.Lose:
                    LogWriter.WriteLog($"負け");
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
            LogWriter.WriteLog($"味方のステータス--------------------");
            foreach(var character in partyManager.partyCharacters)
            {
                LogWriter.WriteLog($"({character.characterName}) HP:{character.characterHealth.currentHP}/{character.characterStatus.MaxHP}");
            }
            LogWriter.WriteLog($"------------------------------------");

            LogWriter.WriteLog($"敵のステータス--------------------");
            foreach(var enemy in enemyManager.enemies)
            {
                LogWriter.WriteLog($"({enemy.characterName}) HP:{enemy.characterHealth.currentHP}/{enemy.characterStatus.MaxHP}");
            }
            LogWriter.WriteLog($"------------------------------------");
        }

        public void Dispose()
        {
            enemyManager.Dispose();
        }
    }
}

