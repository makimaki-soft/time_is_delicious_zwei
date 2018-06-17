using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using SpriteGlow;

public class CardControl : MonoBehaviour {

    ObservableEventTrigger _eventTrigger;
    SpriteGlowEffect _spriteGlow;

    // Use this for initialization
    void Start () {
        _eventTrigger = gameObject.AddComponent<ObservableEventTrigger>();
        _spriteGlow = GetComponent<SpriteGlowEffect>();
    }

    public IObservable<Unit> OnClickAsObservabale
    {
        get
        {
            return _eventTrigger.OnPointerDownAsObservable().Select(_=>Unit.Default);
        }
    }

    public IObservable<Unit> TestAnimation()
    {
        var coroutine = Observable.FromCoroutine(TestAnimationCoroutine).Publish().RefCount();
        coroutine.Subscribe();
        return coroutine;
    }

    IEnumerator TestAnimationCoroutine()
    {
        for(int i=0; i<200; i++)
        {
            transform.Translate(0, 0, 0.1f);
            transform.Rotate(0, 1, 0);
            if(i==100)
            {
                _spriteGlow.GlowColor = Color.blue;
            }
            yield return null;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
