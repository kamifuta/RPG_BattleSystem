using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestButtle
{
    public class ButtleManager : MonoBehaviour
    {
        public Character player { get; private set; } = new Character("プレイヤー", 4, 3, 1, 1);
        public Character enemy { get; private set; } = new Character("エネミー", 10, 1, 1, 1);

        public void Init()
        {
            player= new Character("プレイヤー", 4, 4, 1, 1);
            enemy= new Character("エネミー", 10, 1, 1, 1);
        }
    }
}

