using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken(), this.GetCancellationTokenOnDestroy());
        //tokenSource.Cancel();
        

        try
        {
            AAA(tokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("fff");
        }

        await UniTask.DelayFrame(2, cancellationToken: tokenSource.Token);

        tokenSource.Cancel();
        Debug.Log(tokenSource.Token.IsCancellationRequested);

        tokenSource.Dispose();
    }

    private async UniTaskVoid AAA(CancellationToken token)
    {
        Debug.Log("aaa");
        //await UniTask.DelayFrame(10, cancellationToken: token);
        //await BBB(token);

        try
        {
            //await UniTask.DelayFrame(10, cancellationToken: token);
            //await BBB(token);
        }
        catch (OperationCanceledException e)
        {
            Debug.Log("sss");
            throw;
        }

        Debug.Log(token.IsCancellationRequested);
    }

    private async UniTask BBB(CancellationToken token)
    {
        //await UniTask.DelayFrame(10, cancellationToken: token);

        //token.ThrowIfCancellationRequested();
    }
}
