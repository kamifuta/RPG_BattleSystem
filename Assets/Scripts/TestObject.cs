using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class TestObject : MonoBehaviour
{
    private List<int> list = Enumerable.Range(0, 10000).ToList();

    private Subject<int> subject = new Subject<int>();
    public IObservable<int> observable => subject;

    // Start is called before the first frame update
    void Start()
    {
        subject.Subscribe(_ => Debug.Log("sss")).AddTo(this);

        Test().Forget();
        Test_2().Forget();
    }

    private async UniTaskVoid Test()
    {
        var token = this.GetCancellationTokenOnDestroy();
        await UniTask.WaitUntil(() => list.All(x => x > 50), cancellationToken: token);
    }

    private async UniTaskVoid Test_2()
    {
        var token = this.GetCancellationTokenOnDestroy();
        //await observable.ToUniTask(cancellationToken:token);
    }

    private void OnDestroy()
    {
        subject.OnCompleted();
        subject.Dispose();
    }
}
