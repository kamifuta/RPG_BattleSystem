using Cysharp.Threading.Tasks;
using MyUtil;
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
        IEnumerable<IEnumerable<int>> partyCharacterIndexList;

        partyCharacterIndexList = Enumerable.Range(0, 10).Combination(0, 4);
        Debug.Log(partyCharacterIndexList.Count());
        foreach (var e in partyCharacterIndexList)
        {
            Debug.Log(e.Enumerate());
        }

        partyCharacterIndexList = Enumerable.Range(0, 9).Combination(1, 4);
        Debug.Log(partyCharacterIndexList.Count());

        partyCharacterIndexList = Enumerable.Range(0, 9).Combination(2, 4);
        Debug.Log(partyCharacterIndexList.Count());

        partyCharacterIndexList = Enumerable.Range(0, 9).Combination(3, 4);
        Debug.Log(partyCharacterIndexList.Count());

        partyCharacterIndexList = Enumerable.Range(0, 9).Combination(4, 4);
        Debug.Log(partyCharacterIndexList.Count());

        partyCharacterIndexList = Enumerable.Range(0, 9).Combination(5, 4);
        Debug.Log(partyCharacterIndexList.Count());

        partyCharacterIndexList = Enumerable.Range(0, 9).Combination(6, 4);
        Debug.Log(partyCharacterIndexList.Count());

        partyCharacterIndexList = Enumerable.Range(0, 9).Combination(7, 4);
        Debug.Log(partyCharacterIndexList.Count());

        partyCharacterIndexList = Enumerable.Range(0, 9).Combination(8, 4);
        Debug.Log(partyCharacterIndexList.Count());

        partyCharacterIndexList = Enumerable.Range(0, 9).Combination(9, 4);
        Debug.Log(partyCharacterIndexList.Count());

        var list = Enumerable.Range(0, 10);

        //Debug.Log("Start");
        //stopwatch.Start();
        //await UniTask.SwitchToThreadPool();
        //for (int i = 0; i < 10; i++)
        //{
        //    Debug.Log("aaa" + i);
        //    for (int j = 0; j < 10; j++)
        //    {
        //        Debug.Log("bbb" + j);
        //        for (int k = 0; k < 10; k++)
        //        {
        //            Debug.Log("ccc" + k);
        //            await Test3();
        //        }
        //    }
        //}
        //await UniTask.SwitchToMainThread();
        //stopwatch.Stop();
        //Debug.Log(stopwatch.Elapsed);

        //Parallel.ForEach(list, () => 0, (i, loop, ret) =>
        //  {
        //      Debug.Log($"<color=red>{Thread.GetCurrentProcessorId()}</color>");
        //      Debug.Log("Set Party_" + i);
        //      for (int k = 0; k < 10; k++)
        //      {
        //          Debug.Log(Thread.GetCurrentProcessorId());
        //          Debug.Log($"Start Battle_{i}_{k}");
        //          await Test3();
        //          Debug.Log($"End Battle_{i}_{k}");
        //      }
        //      return 0;
        //  },
        //(ret) =>
        //    {
        //        Debug.Log("All Complete");
        //    }
        //);

        //Debug.Log("Await All Battle");

        //int[] nums = Enumerable.Range(0, 100).ToArray();
        //long total = 0;

        //// First type parameter is the type of the source elements
        //// Second type parameter is the type of the thread-local variable (partition subtotal)
        //Parallel.ForEach<int, long>(nums, // source collection
        //                            () => 0, // method to initialize the local variable
        //                            (j, loop, subtotal) => // method invoked by the loop on each iteration
        //                             {
        //                                 Debug.Log("sss");
        //                                subtotal += j; //modify local variable
        //                                 return subtotal; // value to be passed to next iteration
        //                             },
        //                            // Method to be executed when each partition has completed.
        //                            // finalResult is the final value of subtotal for a particular partition.
        //                            (finalResult) =>
        //                            {
        //                                Interlocked.Add(ref total, finalResult);
        //                                Debug.Log("aaa");
        //                            }
        //                            );

        //Debug.Log($"The total from Parallel.ForEach is {total}");

        //Debug.Log("Start");
        //stopwatch.Restart();
        //Parallel.ForEach(list, async i =>
        //{
        //    Debug.Log(i);
        //    await Test3();
        //    Debug.Log(i+"_");
        //});
        //stopwatch.Stop();
        //Debug.Log("End");
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
        //Debug.Log("ee");
        await UniTask.Delay(TimeSpan.FromSeconds(Random.Range(0.1f,0.5f)));
    }

    private void Test4(int baseID)
    {
        Debug.Log($"<color=blue>{Thread.CurrentThread.ManagedThreadId}:{baseID}</color>");
    }
}
