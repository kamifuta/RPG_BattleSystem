using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestButtle
{
    public class ButtleManager : MonoBehaviour
    {
        public Character player { get; private set; } = new Character("�v���C���[", 4, 3, 1, 1);
        public Character enemy { get; private set; } = new Character("�G�l�~�[", 10, 1, 1, 1);

        public void Init()
        {
            player= new Character("�v���C���[", 4, 4, 1, 1);
            enemy= new Character("�G�l�~�[", 10, 1, 1, 1);
        }
    }
}

