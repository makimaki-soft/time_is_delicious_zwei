using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class DummyHand : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // 手札にカードを追加するアニメーションのダミー関数
    public IObservable<Unit> AddHand(int playerIndex)
    {
        var coroutine = Observable.Timer(TimeSpan.FromSeconds(1)).Do(_=> Debug.Log("Opened : " + playerIndex.ToString())).Select(_ => Unit.Default).Publish().RefCount();
        coroutine.Subscribe();
        return coroutine;
    }
}
