using InGame.Characters;
using InGame.Characters.PlayableCharacters;
using InGame.Items;
using InGame.Magics;
using InGame.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using VContainer;

namespace PCGs
{
    public class CharacterManager
    {
        private List<CharacterStatus> playableCharacterStatusList = new List<CharacterStatus>(12);
        public IReadOnlyList<CharacterStatus> PlayableCharacterStatusList => playableCharacterStatusList;

        private readonly CharacterStatusData characterStatusData;

        [Inject]
        public CharacterManager(CharacterStatusData characterStatusData)
        {
            this.characterStatusData = characterStatusData;
        }

        /// <summary>
        /// 引数の数のプレイヤーのパラメータを生成する
        /// </summary>
        /// <param name="amount"></param>
        public void GenerateCharacterStatuses(int amount)
        {
            playableCharacterStatusList.Clear();

            for(int i = 0; i < amount; i++)
            {
                //プレイヤーのパラメータを生成する
                CharacterStatus status = new CharacterStatus(characterStatusData);
                playableCharacterStatusList.Add(status);
            }
        }

        public void SetStatusList(List<CharacterStatus> statusList)
        {
            playableCharacterStatusList = statusList;
        }

        /// <summary>
        /// 引数の数だけプレイヤーを生成する
        /// </summary>
        /// <param name="amount">生成するプレイヤーの数</param>
        public IEnumerable<PlayableCharacter> GenerateCharacters(IEnumerable<int> indexList)
        {
            foreach(var index in indexList)
            {
                //プレイヤーのパラメータを生成する
                CharacterStatus status = playableCharacterStatusList[index];
                PlayableCharacter character = new PlayableCharacter(status);
                character.SetCharacterName("Player_" + (char)('A' + index));

                //使えるスキルをセットする
                foreach(var skill in Enum.GetValues(typeof(SkillType)))
                {
                    character.AddSkill((SkillType)skill);
                }

                //使える魔法をセットする
                foreach(var magic in Enum.GetValues(typeof(MagicType)))
                {
                    character.AddMagic((MagicType)magic);
                }

                //アイテムを持たせる
                var randam = new System.Random();
                for (int j = 0; j < 3; j++)
                {
                    var enumValues = Enum.GetValues(typeof(ItemType));
                    var itemType = (ItemType)enumValues.GetValue(randam.Next(enumValues.Length));
                    character.AddItem(itemType);
                }

                yield return character;
            }
        }

        /// <summary>
        /// 新しくキャラクターを追加する
        /// </summary>
        /// <param name="character"></param>
        public void AddNewCharacterStatus(CharacterStatus characterStatus)
        {
            playableCharacterStatusList.Add(characterStatus);
        }

        /// <summary>
        /// プレイヤーを削除する
        /// </summary>
        /// <param name="character">削除するプレイヤー</param>
        public void RemoveCharacter(CharacterStatus status)
        {
            //playerAgentFactory.DestroyPlayerAgent(playerDic[character]);
            playableCharacterStatusList.Remove(status);
        }

        /// <summary>
        /// 指定したキャラクターたちにアイテムを新しく持たせる
        /// </summary>
        /// <param name="targetCharacters"></param>
        public void SetItems(IEnumerable<PlayableCharacter> targetCharacters)
        {
            foreach(var character in targetCharacters)
            {
                character.CleanItems();
                var randam = new System.Random();
                for (int j = 0; j < 3; j++)
                {
                    var enumValues = Enum.GetValues(typeof(ItemType));
                    var itemType = (ItemType)enumValues.GetValue(randam.Next(enumValues.Length));
                    character.AddItem(itemType);
                }
            }
        }

        public void ClearStatusList()
        {
            playableCharacterStatusList.Clear();
        }
    }
}
