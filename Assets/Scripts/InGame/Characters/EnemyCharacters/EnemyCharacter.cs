using InGame.Characters.PlayableCharacters;
using InGame.Damages;
using Log;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace InGame.Characters.Enemies
{
    public class EnemyCharacter : BaseCharacter
    {
        private ISubject<(PlayableCharacter, int)> attackerSubject = new Subject<(PlayableCharacter, int)>();
        public IObservable<(PlayableCharacter, int)> AttackerObservable => attackerSubject;

        public EnemyCharacter(CharacterStatus characterStatus) : base(characterStatus)
        {

        }

        public override void ApplyDamage(Damage damage)
        {
            var damageValue = CalcDamage(damage);
            characterHealth.ApplyDamage(damageValue);
            attackerSubject.OnNext((damage.attacker as PlayableCharacter, damageValue));
            //LogWriter.WriteLog($"{characterName}Ç…{damageValue.ToString()}ÇÃÉ_ÉÅÅ[ÉW");

            //if (characterHealth.IsDead)
                //LogWriter.WriteLog($"{characterName}ÇÕì|ÇÍÇΩ");
        }
    }
}

