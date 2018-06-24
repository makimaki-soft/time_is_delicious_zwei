using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class DummyCard : MonoBehaviour {

    

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
            //return _eventTrigger.OnMouseDragAsObservable()
            //    .Select(_ => Tuple.Create(this, Input.mousePosition));
            return Observable.Range(0,10).Select(_=>
            {
                var ret = Tuple.Create(this, Input.mousePosition);
                return ret;
            });
        }
    }
}
