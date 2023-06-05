using InGame.Buttles;
using InGame.Buttles.Actions;
using InGame.Characters;
using InGame.Items;
using InGame.Magics;
using InGame.Parties;
using InGame.Skills;
using MyUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using VContainer;

namespace InGame.Agents.Players
{
    public class PlayerAgent : Agent
    {
        protected PartyManager partyManager;
        protected EnemyManager enemyManager;
        protected PlayableCharacterActionManager playableCharacterActionManager;

        public Action OnEpisodeBeginEvent;
        public bool HadSelectedAction { get; private set; } = false;

        private const int BaseAction = 0;
        private const int ItemAction = 4;
        private const int SkillAction = 8;
        private const int MagicAction = 12;
        private const int LivingEnemyAction = 16;
        private const int PlayerAction = 17;

        public void Init(PartyManager partyManager, EnemyManager enemyManager, PlayableCharacterActionManager playableCharacterActionManager)
        {
            this.partyManager = partyManager;
            this.enemyManager = enemyManager;
            this.playableCharacterActionManager = playableCharacterActionManager;
        }

        public override void OnEpisodeBegin()
        {
            OnEpisodeBeginEvent?.Invoke();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(partyManager.partyCharacters.Select(x => x.HPRate).ToList());
            sensor.AddObservation(partyManager.partyCharacters.Select(x => x.MPRate).ToList());
        }

        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            var itemEnumValues = Enum.GetValues(typeof(ItemType));
            var skillEnumValues = Enum.GetValues(typeof(SkillType));
            var magicEnumValues = Enum.GetValues(typeof(MagicType));

            for(int i = 0; i < partyManager.partyCharacters.Length; i++)
            {
                var character = partyManager.partyCharacters[i];

                //actionMask.SetActionEnabled(i, (int)BaseActionType.UseItem, false);
                //actionMask.SetActionEnabled(i, (int)BaseActionType.UseMagic, false);
                //actionMask.SetActionEnabled(i, (int)BaseActionType.UseSkill, false);

                if (character.HaveItemList.Count <= 0)
                {
                    actionMask.SetActionEnabled(i, (int)BaseActionType.UseItem, false);
                }
                else
                {
                    foreach (var item in itemEnumValues)
                    {
                        actionMask.SetActionEnabled(i + ItemAction, (int)item, character.HaveItemList.Any(x => x == (ItemType)item));
                    }
                }

                if (character.rememberSkills.Count > 0 && character.rememberSkills.Select(x => SkillDataBase.GetSkillData(x)).Any(x => x.consumeMP < character.characterMagic.currentMP))
                {
                    foreach (var skill in skillEnumValues)
                    {
                        var enable = character.rememberSkills.Any(x => x == (SkillType)skill) && SkillDataBase.GetSkillData((SkillType)skill).consumeMP < character.characterMagic.currentMP;
                        actionMask.SetActionEnabled(i + SkillAction, (int)skill, enable);
                    }
                }
                else
                {
                    actionMask.SetActionEnabled(i, (int)BaseActionType.UseSkill, false);
                }

                if (character.rememberMagics.Count > 0 && character.rememberMagics.Select(x => MagicDataBase.GetMagicData(x)).Any(x => x.consumeMP < character.characterMagic.currentMP))
                {
                    foreach (var magic in magicEnumValues)
                    {
                        var enable = character.rememberMagics.Any(x => x == (MagicType)magic) && MagicDataBase.GetMagicData((MagicType)magic).consumeMP < character.characterMagic.currentMP;
                        actionMask.SetActionEnabled(i + MagicAction, (int)magic, enable);
                    }
                }
                else
                {
                    actionMask.SetActionEnabled(i, (int)BaseActionType.UseMagic, false);
                }
            }

            for (int i = 0; i < enemyManager.enemies.Length; i++)
            {
                var character = enemyManager.enemies[i];
                actionMask.SetActionEnabled(LivingEnemyAction, i, !character.characterHealth.IsDead);
            }

        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            BaseCharacter target;

            for(int i = 0; i < partyManager.partyCharacters.Length; i++)
            {
                var character = partyManager.partyCharacters[i];

                if (character.characterHealth.IsDead)
                    continue;

                var actionType = actionBuffers.DiscreteActions[i];
                int targetIndex;
                ActionData action = null;

                switch (actionType)
                {
                    case (int)BaseActionType.NormalAttack:
                        targetIndex = actionBuffers.DiscreteActions[LivingEnemyAction];
                        target = enemyManager.enemies[targetIndex];
                        action = new ActionData(BaseActionType.NormalAttack, character, target);
                        playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                        break;
                    case (int)BaseActionType.Defence:
                        action = new ActionData(BaseActionType.Defence, character);
                        playableCharacterActionManager.SetPlayableCharacterAction(character, action);
                        break;
                    case (int)BaseActionType.UseItem:
                        var itemType = (ItemType)Enum.ToObject(typeof(ItemType), actionBuffers.DiscreteActions[i + ItemAction]);
                        var itemData = ItemDataBase.GetItemData(itemType);
                        switch (itemData.targetType)
                        {
                            case TargetType.Self:
                                action = new ActionData(BaseActionType.UseItem, character, character, itemType);
                                break;
                            case TargetType.Friends:
                                targetIndex = actionBuffers.DiscreteActions[PlayerAction];
                                target = partyManager.partyCharacters[targetIndex];
                                action = new ActionData(BaseActionType.UseItem, character, target, itemType);
                                break;
                            case TargetType.Enemy:
                                targetIndex = actionBuffers.DiscreteActions[LivingEnemyAction];
                                target = enemyManager.enemies[targetIndex];
                                action = new ActionData(BaseActionType.UseItem, character, target, itemType);
                                break;
                            case TargetType.AllFriends:
                                break;
                            case TargetType.AllEnemy:
                                break;
                        }
                        playableCharacterActionManager.SetPlayableCharacterAction(partyManager.partyCharacters[i], action);
                        break;
                    case (int)BaseActionType.UseSkill:
                        var skillType = (SkillType)Enum.ToObject(typeof(ItemType), actionBuffers.DiscreteActions[i + SkillAction]);
                        var skillData = SkillDataBase.GetSkillData(skillType);
                        switch (skillData.targetType)
                        {
                            case TargetType.Self:
                                action = new ActionData(BaseActionType.UseSkill, character, character, skillType);
                                break;
                            case TargetType.Friends:
                                targetIndex = actionBuffers.DiscreteActions[PlayerAction];
                                target = partyManager.partyCharacters[targetIndex];
                                action = new ActionData(BaseActionType.UseSkill, character, target, skillType);
                                break;
                            case TargetType.Enemy:
                                targetIndex = actionBuffers.DiscreteActions[LivingEnemyAction];
                                target = enemyManager.enemies[targetIndex];
                                action = new ActionData(BaseActionType.UseSkill, character, target, skillType);
                                break;
                            case TargetType.AllFriends:
                                break;
                            case TargetType.AllEnemy:
                                break;
                        }
                        playableCharacterActionManager.SetPlayableCharacterAction(partyManager.partyCharacters[i], action);
                        break;
                    case (int)BaseActionType.UseMagic:
                        var magicType= (MagicType)Enum.ToObject(typeof(MagicType), actionBuffers.DiscreteActions[i + MagicAction]);
                        var magicData = MagicDataBase.GetMagicData(magicType);
                        switch (magicData.targetType)
                        {
                            case TargetType.Self:
                                action = new ActionData(BaseActionType.UseMagic, character, character, magicType);
                                break;
                            case TargetType.Friends:
                                targetIndex = actionBuffers.DiscreteActions[PlayerAction];
                                target = partyManager.partyCharacters[targetIndex];
                                action = new ActionData(BaseActionType.UseMagic, character, target, magicType);
                                break;
                            case TargetType.Enemy:
                                targetIndex = actionBuffers.DiscreteActions[LivingEnemyAction];
                                target = enemyManager.enemies[targetIndex];
                                action = new ActionData(BaseActionType.UseMagic, character, target, magicType);
                                break;
                            case TargetType.AllFriends:
                                break;
                            case TargetType.AllEnemy:
                                break;
                        }
                        playableCharacterActionManager.SetPlayableCharacterAction(partyManager.partyCharacters[i], action);
                        break;
                }
            }

            HadSelectedAction = true;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {

        }

        public void ClearFlag()
        {
            HadSelectedAction = false;
        }
    }
}

