using InGame.Characters;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Buttles.Actions
{
    public class ActionArgument
    {
        public BaseCharacter user { get; private set; }
        public IEnumerable<BaseCharacter> targets { get; private set; }

        public ActionArgument(BaseCharacter user, IEnumerable<BaseCharacter> targets)
        {
            this.user = user;
            this.targets = targets;
        }

        public ActionArgument(BaseCharacter user, BaseCharacter target)
        {
            this.user = user;
            this.targets = new List<BaseCharacter>() { target };
        }
    }
}

