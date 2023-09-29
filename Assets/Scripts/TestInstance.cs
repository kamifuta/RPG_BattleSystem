using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class TestInstance : MonoBehaviour
{
    [SerializeField] private TestObject prefab;
    private List<TestObject> list = new List<TestObject>();

    // Start is called before the first frame update
    async void Start()
    {
        for(int i = 0; i < 20000; i++)
        {
            var obj=Instantiate(prefab);
            list.Add(obj);
        }
    }
}
