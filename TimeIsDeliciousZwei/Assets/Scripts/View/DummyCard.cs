using System;
using System.Collections;
using System.Collections.Generic;

using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class DummyCard : MonoBehaviour {

    public TIDZ.MeatDef.ColorElement MeatColor { get; set; }
    public TIDZ.MeatDef.MeatType     Type { get; set; }
    public Guid ID { get; set; }

    ObservableEventTrigger _eventTrigger;

    // Use this for initialization
    void Start () {
        _eventTrigger = gameObject.AddComponent<ObservableEventTrigger>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public class CardEventData
    {
        public bool OnDrag { get; private set; }
        public Vector3 MousePosition { get; private set; }
        public DummyCard Source { get; private set; }
        public CardEventData(bool onDrag, Vector3 mousePosition, DummyCard source)
        {
            OnDrag = onDrag;
            MousePosition = mousePosition;
            Source = source;
        }
    }


    public IObservable<CardEventData> OnDragAsObservabale
    {
        get
        {
            var onDrag = _eventTrigger.OnMouseDragAsObservable().Select(_ => new CardEventData(true, Input.mousePosition, this));
            var endDrag = _eventTrigger.OnEndDragAsObservable().Select(_ => new CardEventData(false, Input.mousePosition, this));
            return Observable.Merge(onDrag, endDrag);
        }
    }
}
