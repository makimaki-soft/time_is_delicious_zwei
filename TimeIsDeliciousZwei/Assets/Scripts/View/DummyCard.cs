using System;
using System.Collections;
using System.Collections.Generic;

using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class DummyCard : MonoBehaviour {

    public TIDZ.MeatDef.ColorElement MeatColor { get; set; }
    public TIDZ.MeatDef.MeatType     Type { get; set; }

    ObservableEventTrigger _eventTrigger;

    // Use this for initialization
    void Start () {
        _eventTrigger = gameObject.AddComponent<ObservableEventTrigger>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public IObservable<Tuple<DummyCard,Vector3>> OnDragAsObservabale
    {
        get
        {

            return _eventTrigger.OnMouseDragAsObservable()
                                .TakeUntil(_eventTrigger.OnEndDragAsObservable())
                                .Select(_ => Tuple.Create(this, Input.mousePosition));
        }
    }
}
