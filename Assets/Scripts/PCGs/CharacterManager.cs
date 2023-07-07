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
        private readonly List<CharacterStatus> playableCharacterStatusList = new List<CharacterStatus>();
        public IReadOnlyList<CharacterStatus> PlayableCharacterStatusList => playableCharacterStatusList;

        private readonly CharacterStatusData characterStatusData;

        [Inject]
        public CharacterManager(CharacterStatusData characterStatusData)
        {
            this.characterStatusData = characterStatusData;
        }

        /// <summary>
        /// �����̐��̃v���C���[�̃p�����[�^�𐶐�����
        /// </summary>
        /// <param name="amount"></param>
        public void GenerateCharacterStatuses(int amount)
        {
            for(int i = 0; i < amount; i++)
            {
                //�v���C���[�̃p�����[�^�𐶐�����
                CharacterStatus status = new CharacterStatus(characterStatusData);
                playableCharacterStatusList.Add(status);
            }
        }

        /// <summary>
        /// �����̐������v���C���[�𐶐�����
        /// </summary>
        /// <param name="amount">��������v���C���[�̐�</param>
        public IEnumerable<PlayableCharacter> GenerateCharacters(IEnumerable<int> indexList)
        {
            foreach(var index in indexList)
            {
                //�v���C���[�̃p�����[�^�𐶐�����
                CharacterStatus status = playableCharacterStatusList[index];
                PlayableCharacter character = new PlayableCharacter(status);
                character.SetCharacterName("Player_" + (char)('A' + index));

                //�g����X�L�����Z�b�g����
                foreach(var skill in Enum.GetValues(typeof(SkillType)))
                {
                    character.AddSkill((SkillType)skill);
                }

                //�g���閂�@���Z�b�g����
                foreach(var magic in Enum.GetValues(typeof(MagicType)))
                {
                    character.AddMagic((MagicType)magic);
                }

                //�A�C�e������������
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
        /// �V�����L�����N�^�[��ǉ�����
        /// </summary>
        /// <param name="character"></param>
        public void AddNewCharacterStatus(CharacterStatus characterStatus)
        {
            playableCharacterStatusList.Add(characterStatus);
        }

        /// <summary>
        /// �v���C���[���폜����
        /// </summary>
        /// <param name="character">�폜����v���C���[</param>
        public void RemoveCharacter(CharacterStatus status)
        {
            //playerAgentFactory.DestroyPlayerAgent(playerDic[character]);
            playableCharacterStatusList.Remove(status);
        }

        /// <summary>
        /// �w�肵���L�����N�^�[�����ɃA�C�e����V������������
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
    }
}
