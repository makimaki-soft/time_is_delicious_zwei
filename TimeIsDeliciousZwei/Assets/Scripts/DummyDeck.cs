using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using static TIDZ.MeatDef;

public class DummyDeck : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // 山札からカードをオープンするアニメーションのダミー関数
    public IObservable<Unit> OpenCard(MeatType type, ColorElement color)
    {
        return OpenCard(type, color, default(Unit));
    }

    public IObservable<T> OpenCard<T>(MeatType type, ColorElement color, T bypass)
    {
        var coroutine = Observable.Timer(TimeSpan.FromSeconds(1)).Do(_=>Debug.Log("Opened : " + type.ToString() + " " + color.ToString())).Select(_ => bypass).Publish().RefCount();
        coroutine.Subscribe();
        return coroutine;
    }
}
