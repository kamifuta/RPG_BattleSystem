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
    public class BattleController : ControllerBase,　IDisposable
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
            //プレイヤーに応じたエージェントを生成する
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
        /// 敵と接触した時に呼び出される
        /// </summary>
        public void Encount()
        {
            IsBattle = true;

            //他クラスの初期化
            enemyManager = new EnemyManager(enemyFactory, partyManager);
            GenerateEnemies(EnemyType.Golem);

            foreach (var agent in agentGroup.GetRegisteredAgents())
            {
                (agent as PlayerAgent).Init(partyManager, enemyManager);
            }
            resultSubject = new Subject<ResultType>();

            fileName = $"LogFile_{id.ToString()}_{battleCount.ToString()}";

            //フィールドの生成
            LogCharacterStatus();
            turnManager.StartTurn();
            allCharacters = agentGroup.GetRegisteredAgents().Cast<PlayerAgent>().Select(x => x.agentCharacter)
                                                            .Concat(enemyManager.enemies.Cast<BaseCharacter>());

            //戦闘を開始する
            StartBattle();
        }

        /// <summary>
        /// 敵を生成する
        /// </summary>
        /// <param name="encountedEnemyType"></param>
        private void GenerateEnemies(EnemyType encountedEnemyType)
        {
            //TODO: 様々な生成のパターンを実装する（複数種の生成や決められたパターンの生成）
            //NOTE: 現在は一種類だけ生成する
            enemyManager.GenerateEnemies(encountedEnemyType, 1);
        }

        /// <summary>
        /// 戦闘を開始する
        /// </summary>
        private void StartBattle()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            ProcessBattle(cancellationTokenSource.Token).Forget();
        }

        /// <summary>
        /// 戦闘を進める
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask ProcessBattle(CancellationToken token)
        {
            //戦闘が終了するまでループする
            while (IsBattle)
            {
                //キャラクターの状態出力
                LogCharacterHPAndMP();

                //生きているすべてのキャラクターを素早さの順にソート
                var sortedCharacters = allCharacters.Where(x => !x.characterHealth.IsDead).OrderByDescending(x => x.characterStatus.Agility);

                //素早さが速い順に行動決定させ、実行する
                foreach(var character in sortedCharacters)
                {
                    var actionData = await DecideAction(character, token);

                    if (actionData == null)
                        continue;

                    ExecuteCharacterAction(actionData);

                    //戦闘が終了しているかを確認する
                    CheckBattleResult();
                    if (!IsBattle)
                        break;
                }

                if (!IsBattle)
                    break;

                //ターン終了時の処理
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
            //キャラクターが死んでいるなら行動しない
            if (character.characterHealth.IsDead)
                return null;

            //キャラクターがプレイヤーのキャラクターだったとき
            if (character is PlayableCharacter)
            {
                IsWaitingPlayerDecision = true;
                var agent = agentGroup.GetRegisteredAgents().Cast<PlayerAgent>().Single(x => x.agentCharacter == character);

                agent.RequestDecision();

                await UniTask.WaitUntil(() => !IsWaitingPlayerDecision, cancellationToken: token);

                return selectedPlayerActionData;
            }
            //キャラクターが敵キャラだったとき
            else if (character is EnemyCharacter)
            {
                //敵AIに行動を決めさせる
                var actionData = enemyManager.GetEnemyAI(character as EnemyCharacter).SelectAction();
                return actionData;
            }

            return null;
        }

        /// <summary>
        /// キャラクターの行動を実行する
        /// </summary>
        /// <param name="character"></param>
        private void ExecuteCharacterAction(ActionData actionData)
        {
            actionData.ExecuteAction();
        }

        /// <summary>
        /// 戦闘が終了しているかを調べる
        /// </summary>
        private void CheckBattleResult()
        {
            //敵が全員死んでいるときプレイヤーの勝ち
            if (enemyManager.enemies.All(x => x.characterHealth.IsDead))
            {
                FinishBattle(ResultType.Win);
                return;
            }

            //見方が全員死んでいるときプレイヤーの負け
            if (partyManager.partyCharacters.All(x => x.characterHealth.IsDead))
            {
                FinishBattle(ResultType.Lose);
                return;
            }
        }

        /// <summary>
        /// キャラクターにかかっているバフを消す
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
        /// 戦闘終了時の処理
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
                    Debug.Log("<color=red>勝利</color>");
                    LogWriter.WriteLog($"\n勝利", fileName);
                    agentGroup.SetGroupReward(1f);
                    break;
                case ResultType.Lose:
                    Debug.Log("<color=blue>敗北</color>");
                    LogWriter.WriteLog($"\n負け", fileName);
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
        /// 敵味方のステータスをすべて書き込む
        /// </summary>
        private void LogCharacterStatus()
        {
            LogWriter.WriteLog($"味方のステータス--------------------", fileName);
            for(int i = 0; i < 4; i++)
            {
                var character = partyManager.partyCharacters[i];
                LogWriter.WriteLog($"({character.characterName}) HP:{character.characterHealth.currentHP.ToString()}/{character.characterStatus.MaxHP.ToString()} MP{character.characterMagic.currentMP.ToString()}/{character.characterStatus.MaxMP.ToString()} " +
                    $"攻撃力{character.characterStatus.AttackValue.ToString()} 魔力{character.characterStatus.MagicValue.ToString()} 防御力{character.characterStatus.DefenceValue.ToString()} 魔法防御力{character.characterStatus.MagicDefenceValue.ToString()} 素早さ{character.characterStatus.Agility.ToString()}" +
                    $"スキル({character.rememberSkills.Enumerate()}) 魔法({character.rememberMagics.Enumerate()})", fileName);
            }
            LogWriter.WriteLog($"------------------------------------", fileName);

            LogWriter.WriteLog($"敵のステータス--------------------", fileName);
            var count = enemyManager.enemies.Length;
            for(int i = 0; i < count; i++)
            {
                var enemy = enemyManager.enemies[i];
                LogWriter.WriteLog($"({enemy.characterName}) HP:{enemy.characterHealth.currentHP.ToString()}/{enemy.characterStatus.MaxHP.ToString()} MP{enemy.characterMagic.currentMP.ToString()}/{enemy.characterStatus.MaxMP.ToString()} " +
                    $"攻撃力{enemy.characterStatus.AttackValue.ToString()} 魔力{enemy.characterStatus.MagicValue.ToString()} 防御力{enemy.characterStatus.DefenceValue.ToString()} 魔法防御力{enemy.characterStatus.MagicDefenceValue.ToString()} 素早さ{enemy.characterStatus.Agility.ToString()}", fileName);
            }
            LogWriter.WriteLog($"------------------------------------", fileName);
        }

        /// <summary>
        /// 味方のHP,MPとアイテム
        /// 敵のHP,MPを表示する
        /// </summary>
        private void LogCharacterHPAndMP()
        {
            LogWriter.WriteLog($"味方のステータス--------------------", fileName);
            for (int i = 0; i < 4; i++)
            {
                var character = partyManager.partyCharacters[i];
                var buff = character.characterStatus.characterBuff;
                LogWriter.WriteLog($"({character.characterName}) HP:{character.characterHealth.currentHP.ToString()}/{character.characterStatus.MaxHP.ToString()} MP{character.characterMagic.currentMP.ToString()}/{character.characterStatus.MaxMP.ToString()}" +
                    $"所持アイテム({character.HaveItemList.Enumerate()}) バフレベル({buff.AttackBuffLevel},{buff.MagicBuffLevel},{buff.DefenceBuffLevel},{buff.MagicDefenceBuffLevel},{buff.AgilityBuffLevel})", fileName);
            }
            LogWriter.WriteLog($"------------------------------------", fileName);

            LogWriter.WriteLog($"敵のステータス--------------------", fileName);
            var count = enemyManager.enemies.Length;
            for (int i = 0; i < count; i++)
            {
                var enemy = enemyManager.enemies[i];
                var buff = enemy.characterStatus.characterBuff;
                LogWriter.WriteLog($"({enemy.characterName}) HP:{enemy.characterHealth.currentHP.ToString()}/{enemy.characterStatus.MaxHP.ToString()} MP{enemy.characterMagic.currentMP.ToString()}/{enemy.characterStatus.MaxMP.ToString()}" +
                    $"バフレベル({buff.AttackBuffLevel},{buff.MagicBuffLevel},{buff.DefenceBuffLevel},{buff.MagicDefenceBuffLevel},{buff.AgilityBuffLevel})", fileName);
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

