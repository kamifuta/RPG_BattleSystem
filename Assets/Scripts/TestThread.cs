using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class TestThread
{
    private Stopwatch stopwatch = new Stopwatch();

    public async UniTask Test()
    {
        Debug.Log("Start");
        stopwatch.Start();
        await UniTask.SwitchToThreadPool();
        for (int i = 0; i < 10; i++)
        {
            Debug.Log("aaa" + i);
            for (int j = 0; j < 10; j++)
            {
                Debug.Log("bbb" + j);
                for (int k = 0; k < 10; k++)
                {
                    Debug.Log("ccc" + k);
                    await Test3();
                }
            }
        }
        await UniTask.SwitchToMainThread();
        stopwatch.Stop();
        Debug.Log(stopwatch.Elapsed);

        //stopwatch.Restart();
        //for (int i = 0; i < 10; i++)
        //{
        //    Debug.Log("aaa"+i);
        //    Parallel.For(0, 10, j =>
        //    {
        //        Debug.Log("bbb"+j);
        //        Parallel.For(0, 10, async k =>
        //        {
        //            Debug.Log("ccc" + k);
        //            await Test3();
        //        });
        //    });
        //}
        //stopwatch.Stop();
        //Debug.Log(stopwatch.Elapsed);
    }

    //private void Test2()
    //{
    //    var id = Thread.CurrentThread.ManagedThreadId;
    //    Debug.Log($"<color=red>{Thread.CurrentThread.ManagedThreadId}</color>");
    //    Test3(id).Forget();
    //}

    public async UniTask Test3()
    {
        //await UniTask.RunOnThreadPool(() => Test4(baseID));
        await UniTask.Delay(TimeSpan.FromSeconds(Random.Range(0.1f,0.5f)));
        //Debug.Log($"<color=green>{Thread.CurrentThread.ManagedThreadId}:{baseID}</color>");
    }

    private void Test4(int baseID)
    {
        Debug.Log($"<color=blue>{Thread.CurrentThread.ManagedThreadId}:{baseID}</color>");
    }
}
