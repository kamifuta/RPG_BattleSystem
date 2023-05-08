using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Characters
{
    public class CharacterHealth : MonoBehaviour
    {
        private int currentHP;

        public bool IsDead => currentHP <= 0;

        public CharacterHealth(int maxHP)
        {
            currentHP = maxHP;
        }

        public void ApplyDamage(int damageValue)
        {
            if (damageValue < 0)
            {
                throw new ArgumentException("�_���[�W�l��0�����ł�");
            }
            currentHP -= damageValue;
            Debug.Log($"currentHP:{currentHP}");
        }
    }
}

