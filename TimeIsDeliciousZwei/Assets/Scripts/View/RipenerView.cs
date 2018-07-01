﻿using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class RipenerView : MonoBehaviour {

    ObservableEventTrigger _eventTrigger;
    public Guid ModelID { get; set; }

    // Use this for initialization
    void Start()
    {
        _eventTrigger = gameObject.AddComponent<ObservableEventTrigger>();
    }

    public IObservable<MonoBehaviour> OnTouchAsObservabale
    {
        get
        {
            return _eventTrigger.OnPointerClickAsObservable().Select(_ => this);
        }
    }

    public IObservable<Unit> AddCardAnimation(CardView card)
    {
        return Observable.FromCoroutine(_ => AddCardAnimationCoroutine(card));
    }

    IEnumerator AddCardAnimationCoroutine(CardView card)
    {
        var targetpos = transform.position;
        targetpos.z -= 0.2f;
        var srcpos = card.transform.position;

        var d = new Vector3((targetpos.x - srcpos.x) / 20f, (targetpos.y - srcpos.y) / 20f, (targetpos.z - srcpos.z) / 20f);

        for (int i = 0; i < 20; i++)
        {
            card.transform.Translate(d);
            yield return null;
        }

        card.transform.position = targetpos;
    }
}