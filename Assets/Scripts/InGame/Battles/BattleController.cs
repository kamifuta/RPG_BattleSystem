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

        private PlayableCharacterActionManager playableCharacterActionManager = new PlayableCharacterActionManager();

        private readonly EnemyFactory enemyFactory;
        private readonly PlayerAgentFactory playerAgentFactory;

        private readonly TurnManager turnManager = new TurnManager();
        private readonly SimpleMultiAgentGroup agentGroup = new SimpleMultiAgentGroup();

        private List<BaseCharacter> hadDoneActionCharacterList= new List<BaseCharacter>();
        private CancellationTokenSource cancellationTokenSource;

        private int battleCount = 0;
        private readonly int id=0;
        private string fileName;

        private bool IsBattle = false;

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
                (agent as PlayerAgent).Init(partyManager, enemyManager, playableCharacterActionManager);
            }
            resultSubject = new Subject<ResultType>();

            fileName = $"LogFile_{id.ToString()}_{battleCount.ToString()}";

            //フィールドの生成
            LogCharacterStatus();
            turnManager.StartTurn();

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
                //ターン開始時の初期化
                hadDoneActionCharacterList.Clear();

                //キャラクターの状態出力
                LogCharacterHPAndMP();

                //エージェントに行動を決定させる
                SelectPlayableCharactersAction();

                //すべてのエージェントが行動を決定するまで待機
                //await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: token);
                var agentList = agentGroup.GetRegisteredAgents().Cast<PlayerAgent>().ToArray();
                await UniTask.WaitUntil(() => agentList.All(x=>x.HadSelectedAction), cancellationToken:token);
                //await WaitSelectAction(token);

                //キャラクターの行動を実行する
                ExecuteActions();
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

        private async UniTask WaitSelectAction(CancellationToken token)
        {
            var agentList = agentGroup.GetRegisteredAgents().Cast<PlayerAgent>().ToArray();

            while (true)
            {
                if (agentList.All(x => x.HadSelectedAction))
                    return;
                await UniTask.Yield(token);
            }
        }

        //プレイヤーに行動を決定させる
        private void SelectPlayableCharactersAction()
        {
            playableCharacterActionManager.ClearDic();
            //各エージェントに行動を決定させる
            foreach (var agent in agentGroup.GetRegisteredAgents())
            {
                (agent as PlayerAgent).RequestDecision();
            }
        }

        private void ExecuteActions()
        {
            //防御するキャラクター
            IEnumerable<BaseCharacter> defenceActionCharacters = playableCharacterActionManager.GetDefenceActionPairs().OrderByDescending(x => x.characterStatus.Agility);
            //優先度の高い行動をするキャラクター
            IEnumerable<BaseCharacter> highPriorityActionCharacters= playableCharacterActionManager.GetHighPriorityActionCharacters().OrderByDescending(x => x.characterStatus.Agility);
            //通常優先度の行動をするプレイヤーキャラクター
            IEnumerable<BaseCharacter> normalPriorityActionPlayers = playableCharacterActionManager.GetNormalPriorityActionCharacters();
            //通常優先度の行動をするキャラクター
            IEnumerable<BaseCharacter> normalPriorityActionCharacters= enemyManager.enemies.Where(x => !x.characterHealth.IsDead)
                                                .Cast<BaseCharacter>()
                                                .Concat(normalPriorityActionPlayers)
                                                .OrderByDescending(x => x.characterStatus.Agility);
            //優先度の低い行動をするキャラクター
            IEnumerable<BaseCharacter> lowPriorityActionCharacters= playableCharacterActionManager.GetLowPriorityActionCharacters().OrderByDescending(x => x.characterStatus.Agility);

            //行動順にソートされたキャラクター
            IEnumerable<BaseCharacter> sortedCharacters = defenceActionCharacters.Concat(highPriorityActionCharacters).Concat(normalPriorityActionCharacters).Concat(lowPriorityActionCharacters);
            foreach(var character in sortedCharacters)
            {
                if (!IsBattle)
                    return;
                try
                {
                    ExecuteCharacterAction(character);
                }
                catch (InvalidOperationException)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// キャラクターの行動を実行する
        /// </summary>
        /// <param name="character"></param>
        private void ExecuteCharacterAction(BaseCharacter character)
        {
            //キャラクターが死んでいるなら行動しない
            if (character.characterHealth.IsDead)
                return;

            //キャラクターがプレイヤーのキャラクターだったとき
            if(character is PlayableCharacter)
            {
                var actionData = playableCharacterActionManager.GetCharacterActionData(character as PlayableCharacter);
                var success = actionData.ExecuteAction();
                //アクションが成功しなかったとき（ターゲットがすでに死んでいるなど）
                if (!success)
                {
                    //生きている敵キャラからターゲットを選びなおす
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
            //キャラクターが敵キャラだったとき
            else if(character is EnemyCharacter)
            {
                //敵AIに行動を決めさせる
                var actionData = enemyManager.GetEnemyAI(character as EnemyCharacter).SelectAction();
                var success = actionData.ExecuteAction();
                //アクションが成功しなかったとき（ターゲットがすでに死んでいるなど）
                if (!success)
                {
                    //生きているプレイヤーキャラクターからターゲットを選びなおす
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

            //行動終了しているキャラクターのリストに追加する
            hadDoneActionCharacterList.Add(character);

            //戦闘が終了しているかを確認する
            CheckBattleResult();
        }

        /// <summary>
        /// ターゲットを変更する
        /// </summary>
        /// <param name="actionData"></param>
        /// <param name="targetGroup"></param>
        /// <returns>
        /// 新しいターゲットを返す
        /// 新しいターゲットが行動者と一致するならnullを返す
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
                character.characterStatus.characterBuff.SetIsDefencing(false);
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
                    $"攻撃力{character.characterStatus.AttackValue.ToString()} 魔力{character.characterStatus.MagicValue.ToString()} 防御力{character.characterStatus.DefecnceValue.ToString()} 魔法防御力{character.characterStatus.MagicDefecnceValue.ToString()} 素早さ{character.characterStatus.Agility.ToString()}" +
                    $"スキル({character.rememberSkills.Enumerate()}) 魔法({character.rememberMagics.Enumerate()})", fileName);
            }
            LogWriter.WriteLog($"------------------------------------", fileName);

            LogWriter.WriteLog($"敵のステータス--------------------", fileName);
            var count = enemyManager.enemies.Length;
            for(int i = 0; i < count; i++)
            {
                var enemy = enemyManager.enemies[i];
                LogWriter.WriteLog($"({enemy.characterName}) HP:{enemy.characterHealth.currentHP.ToString()}/{enemy.characterStatus.MaxHP.ToString()} MP{enemy.characterMagic.currentMP.ToString()}/{enemy.characterStatus.MaxMP.ToString()} " +
                    $"攻撃力{enemy.characterStatus.AttackValue.ToString()} 魔力{enemy.characterStatus.MagicValue.ToString()} 防御力{enemy.characterStatus.DefecnceValue.ToString()} 魔法防御力{enemy.characterStatus.MagicDefecnceValue.ToString()} 素早さ{enemy.characterStatus.Agility.ToString()}", fileName);
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
                LogWriter.WriteLog($"({character.characterName}) HP:{character.characterHealth.currentHP.ToString()}/{character.characterStatus.MaxHP.ToString()} MP{character.characterMagic.currentMP.ToString()}/{character.characterStatus.MaxMP.ToString()}" +
                    $"所持アイテム({character.HaveItemList.Enumerate()})", fileName);
            }
            LogWriter.WriteLog($"------------------------------------", fileName);

            LogWriter.WriteLog($"敵のステータス--------------------", fileName);
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

