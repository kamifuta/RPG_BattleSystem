using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMonobehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var testThread = new TestThread();

        testThread.Test().Forget();
    }

}
