using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TestThread
{
    public async UniTask Test()
    {
        var list = Enumerable.Range(0, 10);

        //foreach(var e in list)
        //{
        //    await UniTask.RunOnThreadPool(()=>Test3());
        //}

        Parallel.ForEach(list, element =>
        {
            Debug.Log(Thread.CurrentThread.ManagedThreadId);
            Test2();
        });
    }

    private void Test2()
    {
        var id = Thread.CurrentThread.ManagedThreadId;
        Debug.Log($"<color=red>{Thread.CurrentThread.ManagedThreadId}</color>");
        Test3(id).Forget();
    }

    public async UniTask Test3(int baseID)
    {
        await UniTask.RunOnThreadPool(() => Test4(baseID));
        await UniTask.DelayFrame(1);
        Debug.Log($"<color=green>{Thread.CurrentThread.ManagedThreadId}:{baseID}</color>");
    }

    private void Test4(int baseID)
    {
        Debug.Log($"<color=blue>{Thread.CurrentThread.ManagedThreadId}:{baseID}</color>");
    }
}
